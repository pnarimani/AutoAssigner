using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Providers
{
    internal class PrefabProvider
    {
        public static Component[] GetAll(Type t)
        {
            ValidateCache();
            
            List<string> paths = PrefabCache.Instance.GetPrefabs(t);

            List<GameObject> all = paths
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(asset => asset != null)
                .ToList();

            var prefabs = new List<Component>();

            foreach (GameObject o in all)
            {
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(o);
                if (type == PrefabAssetType.MissingAsset || type == PrefabAssetType.NotAPrefab)
                    continue;
                Component c = o.GetComponent(t);
                if (c != null)
                    prefabs.Add(c);
            }

            return prefabs.ToArray();
        }

        public static Component GetOne(Type t, string targetName)
        {
            ValidateCache();
            
            List<string> paths = PrefabCache.Instance.GetPrefabs(t);

            string bestPath = NameProcessor.GetMatching(paths, targetName);

            return AssetDatabase.LoadAssetAtPath<GameObject>(bestPath).GetComponent(t);
        }
        
        public static GameObject GetOne(string targetName)
        {
            ValidateCache();

            List<Type> allTypes = PrefabCache.Instance.AllTypes;

            Type bestType = NameProcessor.GetMatching(allTypes, targetName);

            List<string> paths = PrefabCache.Instance.GetPrefabs(bestType);

            string bestPath = NameProcessor.GetMatching(paths, targetName);

            return AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
        }

        private static void ValidateCache()
        {
            var cache = PrefabCache.Instance;

            if (cache.IsValid)
                return;
            
            List<string> paths = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();

            if (paths.Count == cache.PathCount && paths.All(cache.HasPath)) 
                return;
            
            foreach (string path in paths)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Component[] all = go.GetComponents<Component>();

                foreach (Component c in all)
                {
                    if (c == null)
                        continue;

                    cache.AddPath(c.GetType(), path);
                }
            }

            cache.Save();
            cache.IsValid = true;
        }
    }
}