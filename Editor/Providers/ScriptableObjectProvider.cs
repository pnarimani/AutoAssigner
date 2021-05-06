using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Providers
{
    internal class ScriptableObjectProvider
    {
        public static ScriptableObject[] GetAll(Type t)
        {
            return AssetDatabase.FindAssets("t:" + t.Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath(path, t))
                .Cast<ScriptableObject>()
                .ToArray();
        }

        public static ScriptableObject GetOne(Type t, string targetName)
        {
            string[] paths = AssetDatabase.FindAssets("t:" + t.Name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();

            if (paths.Length == 0)
                return null;

            string bestPath = NameProcessor.GetMatching(paths, targetName);

            return (ScriptableObject) AssetDatabase.LoadAssetAtPath(bestPath, t);
        }
    }
}