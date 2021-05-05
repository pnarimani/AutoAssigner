using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoAssigner
{
    [ResolverPriority(-12.0)]
    public class AutoAssignProcessor<T> : OdinPropertyProcessor<T, AutoAssignAttribute>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
        {
            memberInfos.AddDelegate("InjectedAutoAssign", () =>
            {
                if (!GUILayout.Button("Auto Assign")) return;

                foreach (Object v in Property.ValueEntry.WeakValues)
                {
                    var serialized = new SerializedObject(v);

                    SerializedProperty property = serialized.GetIterator();
                    if (property.NextVisible(true))
                    {
                        while (true)
                        {
                            AssignProperty(serialized, property);

                            if (!property.NextVisible(true))
                                break;
                        }
                    }

                    serialized.ApplyModifiedProperties();
                }
            }, -100000f, new OnInspectorGUIAttribute("InjectedAutoAssign"));
        }

        private static void AssignProperty(SerializedObject obj, SerializedProperty property)
        {
            property.GetFieldInfoAndStaticType(out Type fieldType);

            var root = obj.targetObject as Component;
            bool hasPrefabInName = property.name.Contains("prefab", StringComparison.InvariantCultureIgnoreCase);

            if (property.isArray)
            {
                if (property.arraySize != 0) return;

                // Nasty trick incoming
                property.InsertArrayElementAtIndex(0);
                SerializedProperty elementProperty = property.GetArrayElementAtIndex(0);
                elementProperty.GetFieldInfoAndStaticType(out Type element);
                property.DeleteArrayElementAtIndex(0);
                // Nasty trick over

                if (typeof(ScriptableObject).IsAssignableFrom(element))
                {
                    ScriptableObject[] all = GetAllScriptableObjects(element);
                    property.arraySize = all.Length;

                    for (var i = 0; i < all.Length; i++)
                    {
                        property.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                    }
                }
                else if (typeof(Component).IsAssignableFrom(element))
                {
                    Component[] all;

                    if (root != null && !hasPrefabInName)
                    {
                        all = root.GetComponentsInChildren(element, true);

                        if (all.Length == 0)
                            all = GetAllPrefabs(element);
                    }
                    else
                    {
                        all = GetAllPrefabs(element);
                    }

                    property.arraySize = all.Length;

                    for (var i = 0; i < all.Length; i++)
                    {
                        property.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                    }
                }
            }
            else
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference)
                    return;

                if (property.objectReferenceValue != null)
                    return;

                if (typeof(ScriptableObject).IsAssignableFrom(fieldType))
                {
                    property.objectReferenceValue = GetMatching(GetAllScriptableObjects(fieldType), property.name);
                }
                else if (typeof(Component).IsAssignableFrom(fieldType))
                {
                    Component[] all;

                    if (root != null && !hasPrefabInName)
                    {
                        all = root.GetComponentsInChildren(fieldType, true);

                        if (all.Length == 0)
                            all = GetAllPrefabs(fieldType);
                    }
                    else
                    {
                        all = GetAllPrefabs(fieldType);
                    }

                    property.objectReferenceValue = GetMatching(all, property.name);
                }
            }
        }

        private static ScriptableObject[] GetAllScriptableObjects(Type t)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + t.Name);
            var a = new ScriptableObject[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a[i] = (ScriptableObject) AssetDatabase.LoadAssetAtPath(path, t);
            }

            return a;
        }

        private static Component[] GetAllPrefabs(Type t)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            List<Object> all = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath(path, t))
                .Where(asset => asset != null)
                .ToList();

            var prefabs = new List<Component>();

            foreach (Object o in all)
            {
                PrefabAssetType type = PrefabUtility.GetPrefabAssetType(o);
                if (type == PrefabAssetType.MissingAsset || type == PrefabAssetType.NotAPrefab)
                    continue;

                if (o is Component comp)
                {
                    if (comp.GetType() == t)
                        prefabs.Add(comp);
                }
                else if (o is GameObject go)
                {
                    Component c = go.GetComponent(t);
                    if (c != null)
                        prefabs.Add(c);
                }
                else
                    Debug.LogError($"Could not add prefab {o} to list");
            }

            return prefabs.ToArray();
        }

        private static TO GetMatching<TO>(IEnumerable<TO> all, string targetName) where TO : Object
        {
            return all.Select(s => new Tuple<TO, int>(s, GetScore(s.name, targetName)))
                .OrderByDescending(t => t.Item2)
                .FirstOrDefault()?.Item1;
        }

        public static int GetScore(string name, string target)
        {
            int score = 0;

            name = name.Trim(' ', '_');
            target = target.Trim(' ', '_');

            string[] nameParts = name.SplitPascalCase().Split(' ');
            string[] targetParts = target.SplitPascalCase().Split(' ');

            foreach (string namePart in nameParts)
            {
                foreach (string targetPart in targetParts)
                {
                    if (namePart.Equals(targetPart, StringComparison.CurrentCulture))
                        score += 100;
                    else if (namePart.Equals(targetPart, StringComparison.InvariantCulture))
                        score += 90;
                    else if (namePart.Equals(targetPart, StringComparison.InvariantCultureIgnoreCase))
                        score += 80;
                    else if (namePart.Equals(targetPart + "s", StringComparison.InvariantCultureIgnoreCase))
                        score += 70;
                    else if (namePart.Equals(targetPart + "es", StringComparison.InvariantCultureIgnoreCase))
                        score += 70;
                    else if (targetPart.Equals(namePart + "s", StringComparison.InvariantCultureIgnoreCase))
                        score += 70;
                    else if (targetPart.Equals(namePart + "es", StringComparison.InvariantCultureIgnoreCase))
                        score += 70;
                }
            }

            return score;
        }
    }
}