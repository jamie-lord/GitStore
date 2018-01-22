using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace GitStore
{
    public class GitStore
    {
        private readonly string _repoDirectory;
        private readonly string _name;
        private readonly string _email;

        public GitStore(string repoDirectory, string name, string email)
        {
            _repoDirectory = repoDirectory;
            _name = name;
            _email = email;

            Repository.Init(_repoDirectory);
        }

        public async Task Save<T>(T obj)
        {
            var json = ToJson(obj);
            var objId = GetIdValue(obj);
            var path = PathFor<T>(objId);

            await File.WriteAllTextAsync(path, json);

            using (var repo = new Repository(_repoDirectory))
            {
                Commands.Stage(repo, path);

                if (!repo.RetrieveStatus().IsDirty)
                {
                    return;
                }

                var signature = new Signature(_name, _email, DateTime.Now);
                try
                {
                    repo.Commit($"Added object of type {obj.GetType()} with id {objId}", signature, signature, new CommitOptions { PrettifyMessage = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task<T> Get<T>(object objId)
        {
            var path = PathFor<T>(objId);
            if (File.Exists(path))
            {
                return await ToObject<T>(path);
            }
            return default(T);
        }

        private string PathFor<T>(object objId)
        {
            return $"{_repoDirectory}/{objId}_{typeof(T)}.json";
        }

        private string ToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private async Task<T> ToObject<T>(string path)
        {
            var s = await File.ReadAllTextAsync(path);

            if (!string.IsNullOrEmpty(s))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(s);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return default(T);
        }

        private object GetIdValue<T>(T obj)
        {
            var props = obj.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ObjectId)));

            if (props.Count() != 1)
            {
                return null;
            }

            return props.First().GetValue(obj);
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ObjectId : Attribute
    {
    }
}
