using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace GitStore
{
    public sealed class GitStore
    {
        private static readonly GitStore instance = new GitStore();

        static GitStore()
        {
        }

        private GitStore()
        {
        }

        public static GitStore Instance
        {
            get
            {
                return instance;
            }
        }

        private string _repoDirectory;

        public string RepoDirectory
        {
            get
            {
                return _repoDirectory;
            }
            set
            {
                _repoDirectory = value;
                Repository.Init(_repoDirectory);
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _email;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public void Save<T>(T obj)
        {
            var path = SaveObject(obj);
            Commit(path, $"Added object of type {typeof(T)} with id {GetIdValue(obj)}");
        }

        public void Save<T>(IEnumerable<T> objs)
        {
            var paths = new List<string>();

            foreach (var obj in objs)
            {
                var path = SaveObject(obj);
                if (!string.IsNullOrEmpty(path))
                {
                    paths.Add(path);
                }
            }
            Commit(paths, $"Added {paths.Count} objects of type {typeof(T)}");
        }

        private string SaveObject<T>(T obj)
        {
            var json = ToJson(obj);
            var objId = GetIdValue(obj);
            var path = PathFor<T>(objId);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);

                return path;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public void Save(Stream stream, string name)
        {
            var path = SaveFile(stream, name);
            Commit(path, $"Added file called {name}");
        }

        public void Save(List<(Stream, string)> streams)
        {
            var paths = new List<string>();

            foreach (var t in streams)
            {
                var r = SaveFile(t.Item1, t.Item2);
                if (!string.IsNullOrEmpty(r))
                {
                    paths.Add(r);
                }
            }

            if (paths.Count == 1)
            {
                Commit(paths, $"Added 1 file");
            }
            else
            {
                Commit(paths, $"Added {paths.Count} files");
            }
        }

        private string SaveFile(Stream stream, string name)
        {
            var path = PathForFile(name);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                using (var fileStream = File.Create(path))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

                return path;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public Stream Get(string name)
        {
            var path = PathForFile(name);

            try
            {
                if (File.Exists(path))
                {
                    return File.Open(path, FileMode.Open);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public void Delete(string name)
        {
            try
            {
                var path = PathForFile(name);
                var sucess = DeleteFile(path);
                if (sucess)
                {
                    Commit(path, $"Deleted 1 file");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Delete(IEnumerable<string> names)
        {
            var paths = new List<string>();
            foreach (var name in names)
            {
                var path = PathForFile(name);
                var success = DeleteFile(path);
                if (success)
                {
                    paths.Add(path);
                }
            }
            if (paths.Count == 1)
            {
                Commit(paths, $"Deleted 1 file");
            }
            else
            {
                Commit(paths, $"Deleted {paths.Count} files");
            }
        }

        private void Commit(string path, string message)
        {
            Commit(new List<string> { path }, message);
        }

        private void Commit(List<string> paths, string message)
        {
            if (!paths.Any())
            {
                return;
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

                    var signature = new Signature(_name, _email, DateTime.Now);
                    repo.Commit(message, signature, signature, new CommitOptions { PrettifyMessage = true });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public T Get<T>(object objId)
        {
            var path = PathFor<T>(objId);
            if (File.Exists(path))
            {
                return ToObject<T>(path);
            }
            return default(T);
        }

        public void Delete<T>(T obj)
        {
            var objId = GetIdValue(obj);

            Delete<T>(objId);
        }

        public void Delete<T>(object objId)
        {
            var path = PathFor<T>(objId);
            var success = DeleteFile(path);
            if (success)
            {
                Commit(path, $"Deleted object of type {typeof(T)}");
            }
        }

        public void Delete<T>(IEnumerable<object> objIds)
        {
            var paths = new List<string>();
            foreach (var objId in objIds)
            {
                var path = PathFor<T>(objId);
                var success = DeleteFile(path);
                if (success)
                {
                    paths.Add(path);
                }
            }
            if (paths.Count == 1)
            {
                Commit(paths, $"Deleted 1 object of type {typeof(T)}");
            }
            else
            {
                Commit(paths, $"Deleted {paths.Count} objects of type {typeof(T)}");
            }
        }

        public void Delete<T>(IEnumerable<T> objs)
        {
            var objIds = new List<object>();
            foreach (var obj in objs)
            {
                var objId = GetIdValue(obj);
                if (objId != null)
                {
                    objIds.Add(objId);
                }
            }
            Delete<T>(objIds);
        }

        private bool DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
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
                var obj = ToObject<T>(path);

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

        private string PathForFile(string name)
        {
            foreach (var invalidPathChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidPathChar, '_');
            }

            return $"{_repoDirectory}/Files/{name}";
        }

        private string ToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private T ToObject<T>(string path)
        {
            var s = File.ReadAllText(path);

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
