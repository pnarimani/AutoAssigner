using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Providers
{
    internal class PrefabProvider
    {
        public static List<Component> GetAll(Type t, string targetName)
        {
            ValidateCache();
            
            List<string> paths = PrefabCache.Instance.GetPrefabs(t);

            if (paths == null)
                return null;
            
            NameProcessor.CutLowQualityPaths(paths, targetName);
            
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

            return prefabs;
        }

        public static Component GetOne(Type t, string targetName)
        {
            ValidateCache();
            
            List<string> paths = PrefabCache.Instance.GetPrefabs(t);
            
            if (paths == null)
                return null;

            string bestPath = NameProcessor.GetMatching(paths, targetName);

            return AssetDatabase.LoadAssetAtPath<GameObject>(bestPath).GetComponent(t);
        }
        
        public static GameObject GetOne(string targetName)
        {
            ValidateCache();

            List<Type> allTypes = PrefabCache.Instance.AllTypes;

            if (allTypes.Count == 0)
                return null;

            Type bestType = NameProcessor.GetMatching(allTypes, targetName);

            List<string> paths = PrefabCache.Instance.GetPrefabs(bestType);
            
            if (paths == null)
                return null;

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