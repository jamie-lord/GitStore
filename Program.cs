using System.IO;

namespace GitStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var store = new GitStore(@"/Users/jamie/Downloads/Test repo", "GitStore test", "test@test.com");

            for (int i = 0; i < 100; i++)
            {
                var obj = new TestObject
                {
                    Id = i,
                    Text = Path.GetRandomFileName()
                };

                store.Save(obj).Wait();

                var tObj = store.Get<TestObject>(obj.Id).Result;
            }
        }
    }

    public class TestObject
    {
        [ObjectId]
        public int Id { get; set; }
        public string Text { get; set; }
    }
}
