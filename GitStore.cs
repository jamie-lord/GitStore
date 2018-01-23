using System;
using System.Collections.Generic;
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

        private Signature _signature
        {
            get { return new Signature(_name, _email, DateTime.Now); }
        }

        public async Task Save<T>(T obj)
        {
            var json = ToJson(obj);
            var objId = GetIdValue(obj);
            var path = PathFor<T>(objId);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllTextAsync(path, json);

                using (var repo = new Repository(_repoDirectory))
                {
                    Commands.Stage(repo, path);

                    if (!repo.RetrieveStatus().IsDirty)
                    {
                        return;
                    }

                    var signature = _signature;
                    repo.Commit($"Added object of type {typeof(T)} with id {objId}", signature, signature, new CommitOptions { PrettifyMessage = true });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task Save<T>(IEnumerable<T> objs)
        {
            var paths = new List<string>();

            foreach (var obj in objs)
            {
                var json = ToJson(obj);
                var objId = GetIdValue(obj);
                var path = PathFor<T>(objId);
                paths.Add(path);

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    await File.WriteAllTextAsync(path, json);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            try
            {
                using (var repo = new Repository(_repoDirectory))
                {
                    Commands.Stage(repo, paths);

                    if (!repo.RetrieveStatus().IsDirty)
                    {
                        return;
                    }

                    var signature = _signature;
                    repo.Commit($"Added {paths.Count} objects of type {typeof(T)}", signature, signature, new CommitOptions { PrettifyMessage = true });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public IEnumerable<T> Get<T>(Predicate<T> predicate)
        {
            var dir = $"{_repoDirectory}/{typeof(T)}";

            if (!Directory.Exists(dir))
            {
                yield break;
            }

            foreach (var path in Directory.EnumerateFiles(dir))
            {
                var obj = ToObject<T>(path).Result;

                if (obj == null)
                {
                    continue;
                }

                if (predicate.Invoke(obj))
                {
                    yield return obj;
                }
            }
        }

        private string PathFor<T>(object objId)
        {
            var path = $"{typeof(T)}/{objId}.json";

            foreach (var invalidPathChar in Path.GetInvalidPathChars())
            {
                path = path.Replace(invalidPathChar, '_');
            }

            return $"{_repoDirectory}/{path}";
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
                    Console.WriteLine("Failed to deserialize json to object. " + ex.Message);
                }
            }
            return default(T);
        }

        private object GetIdValue<T>(T obj)
        {
            var props = obj.GetType().GetProperties().Where(prop => prop.Name == "Id");

            if (props.Count() != 1)
            {
                return null;
            }

            return props.First().GetValue(obj);
        }
    }
}
