using System;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Caching
{
    internal class PrefabCacheProcessor : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string imported in importedAssets)
            {
                if (!imported.EndsWith(".prefab"))
                    continue;

                var go = AssetDatabase.LoadAssetAtPath<GameObject>(imported);
                Component[] all = go.GetComponents<Component>();

                foreach (Component c in all)
                {
                    if (c == null)
                        continue;
                    
                    PrefabCache.Instance.AddPath(c.GetType(), imported);
                }
            }

            foreach (string del in deletedAssets)
            {
                if (!del.EndsWith(".prefab"))
                    continue;
                
                PrefabCache.Instance.RemovePath(del);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                string from = movedFromAssetPaths[i];
                string to = movedAssets[i];
                
                if (!from.EndsWith(".prefab") || !to.EndsWith(".prefab"))
                    continue;
                
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(to);
                Component[] all = go.GetComponents<Component>();

                foreach (Component c in all)
                {
                    if (c == null)
                        continue;
                    
                    Type type = c.GetType();
                    PrefabCache.Instance.RemovePath(type, from);
                    PrefabCache.Instance.AddPath(type, to);
                }
            }
            
            PrefabCache.Instance.Save();
        }
    }
}