using System;
using System.IO;
using System.Linq;
using System.Net;
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
            var obj = new TestObject()
            {
                Id = 1,
                Data = "This is only a test"
            };

            _store.Save(obj);

            Assert.IsTrue(Directory.Exists(_tempRepoDir + @"\" + obj.GetType()));
            Assert.IsTrue(File.Exists(_tempRepoDir + @"\" + obj.GetType() + @"\1.json"));
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
            var obj = new TestObject()
            {
                Id = 1,
                Data = "This is only a test"
            };

            _store.Save(obj);

            var result = _store.Get<TestObject>(1);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("This is only a test", result.Data);
        }

        [TestMethod]
        public void GetPredicateObject()
        {
            var obj = new TestObject()
            {
                Id = 1,
                Data = "This is only a test"
            };

            _store.Save(obj);

            var result = _store.Get<TestObject>(x => x.Data.Contains("test"));

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("This is only a test", result.First().Data);
        }

        [TestMethod]
        public void DeleteObject()
        {
            var obj = new TestObject()
            {
                Id = 1,
                Data = "This is only a test"
            };

            _store.Save(obj);

            var result = _store.Get<TestObject>(1);

            Assert.IsNotNull(result);

            _store.Delete(obj);

            result = _store.Get<TestObject>(1);

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

        public class TestObject
        {
            public int Id { get; set; }
            public string Data { get; set; }
        }
    }
}
