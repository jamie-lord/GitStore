using System;
using System.IO;
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
