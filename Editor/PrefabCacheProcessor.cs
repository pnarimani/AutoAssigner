using System;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner
{
    internal class PrefabCacheProcessor : AssetPostprocessor
    {
        public static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var cache = PrefabCache.Instance;
            
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
                    
                    cache.AddPath(c.GetType(), imported);
                }
            }

            foreach (string del in deletedAssets)
            {
                if (!del.EndsWith(".prefab"))
                    continue;
                
                cache.RemovePath(del);
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
                    cache.RemovePath(type, from);
                    cache.AddPath(type, to);
                }
            }
            
            cache.Save();
        }
    }
}