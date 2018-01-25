using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime;
using Foundation.ObjectHydrator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class GitStoreTests
    {
        private GitStore.GitStore _store;
        private readonly string _tempRepoDir = Path.GetTempPath() + @"\GitStore_Unit_Tests_" + DateTime.Now.Ticks;

        [TestInitialize]
        public void Initialize()
        {
            Directory.CreateDirectory(_tempRepoDir);

            _store = GitStore.GitStore.Instance;
            _store.RepoDirectory = _tempRepoDir;
            _store.Name = "Unit tester";
            _store.Email = "unit@test.com";
        }

        [TestMethod]
        public void SaveObject()
        {
            var obj = _hydrator.GetSingle();

            _store.Save(obj);

            Assert.IsTrue(Directory.Exists(_tempRepoDir + @"\" + obj.GetType()));
            Assert.IsTrue(File.Exists(_tempRepoDir + @"\" + obj.GetType() + @"\" + obj.Id + ".json"));
        }

        [TestMethod]
        public void SaveFile()
        {
            using (WebClient webClient = new WebClient())
            {
                var iData = webClient.DownloadData("https://picsum.photos/500/500?random");

                _store.Save(iData, "image.jpg");
            }

            Assert.IsTrue(Directory.Exists(_tempRepoDir + @"\Files"));
            Assert.IsTrue(File.Exists(_tempRepoDir + @"\Files\image.jpg"));
        }

        [TestMethod]
        public void GetFile()
        {
            using (WebClient webClient = new WebClient())
            {
                var iData = webClient.DownloadData("https://picsum.photos/500/500?random");

                _store.Save(iData, "image.jpg");
            }

            var file = _store.Get("image.jpg");

            Assert.IsNotNull(file);
        }

        [TestMethod]
        public void GetObject()
        {
            var obj = _hydrator.GetSingle();

            _store.Save(obj);

            var result = _store.Get<TestObject>(obj.Id);

            Assert.IsNotNull(result);

            CompareObjects(obj, result);
        }

        private void CompareObjects(object o1, object o2)
        {
            foreach (var propertyInfo in o1.GetType().GetProperties())
            {
                if (propertyInfo.PropertyType == typeof(byte[]))
                {
                    Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(propertyInfo.GetValue(o1), propertyInfo.GetValue(o2)));
                }
                else
                {
                    Assert.AreEqual(propertyInfo.GetValue(o1), propertyInfo.GetValue(o2));
                }
            }
        }

        [TestMethod]
        public void GetPredicateObject()
        {
            var obj = _hydrator.GetSingle();
            _store.Save(obj);

            var result = _store.Get<TestObject>(x => x.String.Contains(obj.String));

            Assert.IsNotNull(result);
            CompareObjects(obj, result.First());
        }

        [TestMethod]
        public void DeleteObject()
        {
            var obj = _hydrator.GetSingle();

            _store.Save(obj);

            var result = _store.Get<TestObject>(obj.Id);

            Assert.IsNotNull(result);

            CompareObjects(obj, result);

            _store.Delete(obj);

            result = _store.Get<TestObject>(obj.Id);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void DeleteFile()
        {
            using (WebClient webClient = new WebClient())
            {
                var iData = webClient.DownloadData("https://picsum.photos/500/500?random");

                _store.Save(iData, "image.jpg");
            }

            var result = _store.Get("image.jpg");

            Assert.IsNotNull(result);

            result.Close();
            result.Dispose();

            _store.Delete("image.jpg");

            result = _store.Get("image.jpg");

            Assert.IsNull(result);
            Assert.IsFalse(Directory.Exists(_tempRepoDir + @"\Files"));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _store = null;
        }

        private Hydrator<TestObject> _hydrator = new Hydrator<TestObject>();

        public class TestObject
        {
            public int Id { get; set; }
            public string String { get; set; }
            public decimal Decimal { get; set; }
            public bool Bool { get; set; }
            public byte[] Bytes { get; set; }
            public char Char { get; set; }
            public double Double { get; set; }
            public float Float { get; set; }
            public long Long { get; set; }
            public sbyte Sbyte { get; set; }
            public uint Uint { get; set; }
            public ushort Ushort { get; set; }
            public Guid Guid { get; set; }
        }
    }
}
