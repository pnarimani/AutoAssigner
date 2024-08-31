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
        public static List<Component> GetAll(Type t, PropertyIdentifiers targetName)
        {
            var paths = PrefabCache.Instance.GetPrefabs(t);

            if (paths == null)
                return null;

            NameProcessor.CutLowQualityPaths(paths, targetName);

            var all = paths
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(asset => asset != null)
                .ToList();

            var prefabs = new List<Component>();

            foreach (var o in all)
            {
                var type = PrefabUtility.GetPrefabAssetType(o);
                if (type == PrefabAssetType.MissingAsset || type == PrefabAssetType.NotAPrefab)
                    continue;
                var c = o.GetComponent(t);
                if (c != null)
                    prefabs.Add(c);
            }

            return prefabs;
        }

        public static Component GetOne(Type t, PropertyIdentifiers targetName)
        {
            var paths = PrefabCache.Instance.GetPrefabs(t);

            if (paths == null)
                return null;

            var (bestPath, _) = NameProcessor.GetMatching(paths, targetName);

            return AssetDatabase.LoadAssetAtPath<GameObject>(bestPath).GetComponent(t);
        }

        public static GameObject GetOne(PropertyIdentifiers targetName)
        {
            var (bestType, typeScore) = NameProcessor.GetMatching(PrefabCache.Instance.AllTypes, targetName);
            var (bestPath, pathScore) = NameProcessor.GetMatching(PrefabCache.Instance.AllPaths, targetName);

            if (typeScore > pathScore)
            {
                var paths = PrefabCache.Instance.GetPrefabs(bestType);

                if (paths != null)
                    (bestPath, _) = NameProcessor.GetMatching(paths, targetName);
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(bestPath);
        }
    }
}