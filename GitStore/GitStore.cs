using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace GitStore
{
    public sealed class GitStore
    {
        private string _repoDirectory;

        static GitStore()
        {
        }

        private GitStore()
        {
        }

        public static GitStore Instance { get; } = new GitStore();

        public string RepoDirectory
        {
            get { return _repoDirectory; }
            set
            {
                _repoDirectory = value;
                Repository.Init(_repoDirectory);
            }
        }

        public string Name { get; set; }

        public string Email { get; set; }

        public void SaveObject<T>(T obj)
        {
            ExecuteWrite(() =>
            {
                var path = SaveObjectAction(obj);
                Commit(path, $"Added object of type {typeof(T)} with id {GetIdPropertyValue(obj)}");
            });
        }

        private static readonly Object _writeLock = new Object();

        public void SaveObjects<T>(IEnumerable<T> objs)
        {
            ExecuteWrite(() =>
            {
                var paths = new List<string>();

                foreach (var obj in objs)
                {
                    var path = SaveObjectAction(obj);
                    if (!string.IsNullOrEmpty(path))
                    {
                        paths.Add(path);
                    }
                }

                Commit(paths, $"Added {paths.Count} objects of type {typeof(T)}");
            });
        }

        private void ExecuteWrite(Action action)
        {
            var task = new Task(action);
            lock (_writeLock)
            {
                task.RunSynchronously();
            }
        }

        private string SaveObjectAction<T>(T obj)
        {
            var json = ObjectToJson(obj);
            var objId = GetIdPropertyValue(obj);
            var path = PathForObjectWithId<T>(objId);

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

        public void SaveData(byte[] data, string name)
        {
            var stream = new MemoryStream(data);
            SaveStream(stream, name);
        }

        public void SaveString(string data, string name)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(data);
            writer.Flush();
            SaveStream(stream, name);
        }

        public void SaveStream(Stream data, string name)
        {
            ExecuteWrite(() =>
            {
                var path = SaveFile(data, name);
                Commit(path, $"Added file called {name}");
            });
        }

        public void SaveStreams(List<(Stream, string)> streams)
        {
            ExecuteWrite(() =>
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
            });
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

                stream.Close();
                stream.Dispose();
                return path;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            stream.Close();
            stream.Dispose();
            return null;
        }

        public Stream GetStream(string name)
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

        public void DeleteFile(string name)
        {
            ExecuteWrite(() =>
            {
                try
                {
                    var path = PathForFile(name);
                    var sucess = DeleteFileAction(path);
                    if (sucess)
                    {
                        Commit(path, $"Deleted 1 file");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        public void DeleteFiles(IEnumerable<string> names)
        {
            ExecuteWrite(() =>
            {
                var paths = new List<string>();
                foreach (var name in names)
                {
                    var path = PathForFile(name);
                    var success = DeleteFileAction(path);
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
            });
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

                    var signature = new Signature(Name, Email, DateTime.Now);
                    repo.Commit(message, signature, signature, new CommitOptions { PrettifyMessage = true });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public T GetObject<T>(object objId)
        {
            var path = PathForObjectWithId<T>(objId);
            if (File.Exists(path))
            {
                return JsonToObject<T>(path);
            }

            return default(T);
        }

        public void DeleteObject<T>(T obj)
        {
            var objId = GetIdPropertyValue(obj);

            DeleteObjectWithId<T>(objId);
        }

        public void DeleteObjectWithId<T>(object objId)
        {
            ExecuteWrite(() =>
            {
                var path = PathForObjectWithId<T>(objId);
                var success = DeleteFileAction(path);
                if (success)
                {
                    Commit(path, $"Deleted object of type {typeof(T)}");
                }
            });
        }

        public void DeleteObjectsWithIds<T>(IEnumerable<object> objIds)
        {
            ExecuteWrite(() =>
            {
                var paths = new List<string>();
                foreach (var objId in objIds)
                {
                    var path = PathForObjectWithId<T>(objId);
                    var success = DeleteFileAction(path);
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
            });
        }

        public void DeleteObjects<T>(IEnumerable<T> objs)
        {
            var objIds = new List<object>();
            foreach (var obj in objs)
            {
                var objId = GetIdPropertyValue(obj);
                if (objId != null)
                {
                    objIds.Add(objId);
                }
            }

            DeleteObjectsWithIds<T>(objIds);
        }

        private bool DeleteFileAction(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);

                if (Directory.GetFiles(Path.GetDirectoryName(path)).Length == 0)
                {
                    Directory.Delete(Path.GetDirectoryName(path));
                }

                return true;
            }

            return false;
        }

        public IEnumerable<T> GetObjects<T>(Predicate<T> predicate)
        {
            var dir = PathForType<T>();

            if (!Directory.Exists(dir))
            {
                yield break;
            }

            foreach (var path in Directory.EnumerateFiles(dir))
            {
                var obj = JsonToObject<T>(path);

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

        public IEnumerable<T> GetAllObjects<T>()
        {
            var dir = PathForType<T>();

            if (!Directory.Exists(dir))
            {
                yield break;
            }

            foreach (var path in Directory.EnumerateFiles(dir))
            {
                var obj = JsonToObject<T>(path);

                if (obj == null)
                {
                    continue;
                }

                yield return obj;
            }
        }

        private string PathForType<T>()
        {
            var type = typeof(T).ToString();
            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            {
                type = type.Replace(invalidFileNameChar, '_');
            }

            return $"{_repoDirectory}/{type}";
        }

        private string PathForObjectWithId<T>(object objId)
        {
            var type = typeof(T).ToString();
            var id = objId.ToString();

            foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            {
                type = type.Replace(invalidFileNameChar, '_');
                id = id.Replace(invalidFileNameChar, '_');
            }

            return $"{_repoDirectory}/{type}/{id}.json";
        }

        private string PathForFile(string name)
        {
            foreach (var invalidPathChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidPathChar, '_');
            }

            return $"{_repoDirectory}/Files/{name}";
        }

        private string ObjectToJson<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        private T JsonToObject<T>(string path)
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

        private object GetIdPropertyValue<T>(T obj)
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
