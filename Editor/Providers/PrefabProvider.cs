using System;
using System.Collections.Generic;
using System.Linq;
using AutoAssigner.Caching;
using AutoAssigner.Scoring;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Providers
{
    internal class PrefabProvider
    {
        public static List<Component> GetAll(Type t, string targetName)
        {
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
            List<string> paths = PrefabCache.Instance.GetPrefabs(t);

            if (paths == null)
                return null;

            (string bestPath, _) = NameProcessor.GetMatching(paths, targetName);

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
            return prefab != null ? prefab.GetComponent(t) : null;
        }

        public static GameObject GetOne(string targetName)
        {
            (Type bestType, int typeScore) = NameProcessor.GetMatching(PrefabCache.Instance.AllTypes, targetName);
            (string bestPath, int pathScore) = NameProcessor.GetMatching(PrefabCache.Instance.AllPaths, targetName);

            if (typeScore > pathScore)
            {
                List<string> paths = PrefabCache.Instance.GetPrefabs(bestType);

                if (paths != null)
                    (bestPath, _) = NameProcessor.GetMatching(paths, targetName);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
        }
    }
}