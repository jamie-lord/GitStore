using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Bogus;

namespace GitStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var faker = new Faker();

            var store = GitStore.Instance;
            store.RepoDirectory = @"/Users/jamie/Downloads/Test repo";
            store.Name = "GitStore test";
            store.Email = "test@test.com";

            //var urls = new List<string>
            //{
            //    "https://github.com/facebookresearch/Detectron/blob/master/demo/16004479832_a748d55f21_k.jpg",
            //    "https://github.com/facebookresearch/Detectron/blob/master/demo/34501842524_3c858b3080_k.jpg",
            //    "https://raw.githubusercontent.com/facebookresearch/Detectron/master/GETTING_STARTED.md"
            //};

            //var streams = new List<(Stream, string)>();
            //foreach (var url in urls)
            //{
            //    using (WebClient webClient = new WebClient())
            //    {
            //        var data = webClient.DownloadData(url);

            //        Stream stream = new MemoryStream(data);

            //        streams.Add((stream, Path.GetFileName(url)));
            //    }
            //}

            //store.Save(streams);

            //store.Delete("GETTING_STARTED.md");


            //var r = store.Get<TestObject>(x => x.Urls.Exists(y => y.Contains(".com")) && (x.Timestamp - DateTime.Now).TotalDays < 30);

            //foreach (var testObject in r)
            //{

            //}


            var objs = new List<TestObject>();
            for (int i = 0; i < 1000; i++)
            {
                var urls = new List<string>();

                for (int j = 0; j < faker.Random.Int(0, 100); j++)
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

            var objectsToRemove = store.Get<TestObject>(x => x.UserName.Contains("13"));

            store.Delete<TestObject>(objectsToRemove);
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
