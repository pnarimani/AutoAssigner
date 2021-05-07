using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner
{
    [Serializable]
    internal class PrefabCache : ISerializationCallbackReceiver
    {
        private static PrefabCache _instance;

        [SerializeField] private List<Pair> _prefabsList = new List<Pair>();
        [SerializeField] private List<PathLookupPair> _pathLookupList = new List<PathLookupPair>();

        private readonly Dictionary<Type, List<string>> _prefabs = new Dictionary<Type, List<string>>();
        private readonly Dictionary<string, List<Type>> _pathLookUp = new Dictionary<string, List<Type>>();

        private static string LibPath => Path.Combine(
            Path.GetDirectoryName(Application.dataPath),
            "Library",
            "AutoAssignPrefabCache.asset"
        );

        public static PrefabCache Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = new PrefabCache();

                if (File.Exists(LibPath))
                {
                    string json = File.ReadAllText(LibPath);
                    EditorJsonUtility.FromJsonOverwrite(json, _instance);
                }

                return _instance;
            }
        }

        public int PathCount => _pathLookUp.Count;
        
        public bool IsValid { get; set; }

        public List<string> AllPaths => _pathLookUp.Keys.ToList();

        public List<Type> AllTypes => _prefabs.Keys.ToList();

        public void Save()
        {
            string json = EditorJsonUtility.ToJson(this, false);
            File.WriteAllText(LibPath, json);
        }

        public bool HasPath(string path)
        {
            return _pathLookUp.ContainsKey(path);
        }

        public List<string> GetPrefabs(Type component)
        {
            if (component == null)
                return AllPaths;
            
            _prefabs.TryGetValue(component, out List<string> list);
            return list;
        }

        public void AddPath(Type component, string path)
        {
            if (_prefabs.TryGetValue(component, out List<string> list))
            {
                if (!list.Contains(path))
                    list.Add(path);
            }
            else
            {
                _prefabs.Add(component, new List<string> {path});
            }

            if (_pathLookUp.TryGetValue(path, out var typesList))
            {
                if (!typesList.Contains(component))
                    typesList.Add(component);
            }
            else
            {
                _pathLookUp.Add(path, new List<Type> {component});
            }
        }

        public void RemovePath(Type c, string path)
        {
            if (_prefabs.TryGetValue(c, out List<string> list))
            {
                if (list.Contains(path))
                    list.Remove(path);
            }
        }

        public void RemovePath(string path)
        {
            if (_pathLookUp.TryGetValue(path, out List<Type> types))
            {
                foreach (Type t in types)
                {
                    RemovePath(t, path);
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _prefabsList.Clear();
            _pathLookupList.Clear();

            foreach (KeyValuePair<Type, List<string>> kvp in _prefabs)
            {
                if (kvp.Key == null)
                    continue;

                if (kvp.Value == null)
                    continue;

                if (kvp.Value.Count == 0)
                    continue;
                
                _prefabsList.Add(new Pair
                {
                    TypeName = kvp.Key.AssemblyQualifiedName,
                    Paths = kvp.Value
                });
            }

            foreach (KeyValuePair<string, List<Type>> kvp in _pathLookUp)
            {
                if (kvp.Key == null)
                    continue;

                if (kvp.Value == null)
                    continue;

                if (kvp.Value.Count == 0)
                    continue;
                
                _pathLookupList.Add(new PathLookupPair
                {
                    Path = kvp.Key,
                    TypeNames = kvp.Value.Where(x => x != null).Select(x => x.AssemblyQualifiedName).ToList()
                });
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _prefabs.Clear();
            _pathLookUp.Clear();

            foreach (Pair pair in _prefabsList)
            {
                var type = Type.GetType(pair.TypeName);
                if (type == null)
                    continue;

                _prefabs.Add(type, pair.Paths);
            }

            foreach (PathLookupPair pair in _pathLookupList)
            {
                _pathLookUp.Add(pair.Path, pair.TypeNames.Select(Type.GetType).Where(t => t != null).ToList());
            }
        }

        [Serializable]
        private class Pair
        {
            public string TypeName;
            public List<string> Paths;
        }


        [Serializable]
        private class PathLookupPair
        {
            public string Path;
            public List<string> TypeNames;
        }
    }
}