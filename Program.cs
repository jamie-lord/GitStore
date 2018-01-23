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

            var faker = new Faker();

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

                store.Save(obj).Wait();

                var tObj = store.Get<TestObject>(obj.Id).Result;
            }
        }
    }

    public class TestObject
    {
        [ObjectId]
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Urls { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
