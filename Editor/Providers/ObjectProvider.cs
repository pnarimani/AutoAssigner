using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoAssigner.Providers
{
    internal class ObjectProvider
    {
        public static List<Object> GetAll(Type t, string targetName)
        {
            List<string> paths = AssetDatabase.FindAssets("t:" + t.Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();

            NameProcessor.CutLowQualityPaths(paths, targetName);
            
            return paths
                .Select(path => AssetDatabase.LoadAssetAtPath(path, t))
                .ToList();
        }

        public static Object GetOne(Type t, string targetName)
        {
            List<string> paths = AssetDatabase.FindAssets("t:" + t.Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToList();

            if (paths.Count == 0)
                return null;

            string bestPath = NameProcessor.GetMatching(paths, targetName);

            return AssetDatabase.LoadAssetAtPath(bestPath, t);
        }
    }
}