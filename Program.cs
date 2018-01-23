using System;
using System.Collections.Generic;
using Bogus;

namespace GitStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var store = new GitStore(@"/Users/jamie/Downloads/Test repo", "GitStore test", "test@test.com");

            var r = store.Get<TestObject>(x => x.Urls.Exists(y => y.Contains(".com")) && (x.Timestamp - DateTime.Now).TotalDays < 30);

            foreach (var testObject in r)
            {
                
            }

            var faker = new Faker();

            var objs = new List<TestObject>();
            for (int i = 0; i < 1000; i++)
            {
                var urls = new List<string>();

                for (int j = 0; j < faker.Random.Int(0, 10000); j++)
                {
                    urls.Add(faker.Internet.UrlWithPath());
                }

                var obj = new TestObject
                {
                    Id = faker.Random.Uuid(),
                    FirstName = faker.Person.FirstName,
                    LastName = faker.Person.LastName,
                    Email = faker.Internet.Email(),
                    UserName = faker.Internet.UserName(),
                    Urls = urls,
                    Timestamp = faker.Date.Past(20)
                };

                objs.Add(obj);

                // store.Save(obj).Wait();

                // var tObj = store.Get<TestObject>(obj.Id).Result;
            }

            store.Save<TestObject>(objs);
        }
    }

    public class TestObject
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Urls { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
