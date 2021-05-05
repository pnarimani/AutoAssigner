using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                            FieldInfo field = serialized.targetObject.GetType().GetField(property.propertyPath);

                            if (field != null)
                            {
                                if (property.isArray)
                                {
                                    if (property.arraySize == 0)
                                    {
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
                                            if (serialized.targetObject is Component root)
                                            {
                                                Component[] all = root.GetComponentsInChildren(element, true);
                                                property.arraySize = all.Length;

                                                for (var i = 0; i < all.Length; i++)
                                                {
                                                    property.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (typeof(ScriptableObject).IsAssignableFrom(field.FieldType))
                                    {
                                        if (property.objectReferenceValue == null)
                                        {
                                            ScriptableObject[] all = GetAllScriptableObjects(field.FieldType);
                                            property.objectReferenceValue = GetMatching(all, property.name);
                                        }
                                    }
                                    else if (typeof(Component).IsAssignableFrom(field.FieldType))
                                    {
                                        if (property.objectReferenceValue == null &&
                                            serialized.targetObject is Component root)
                                        {
                                            Component[] all = root.GetComponentsInChildren(field.FieldType, true);
                                            property.objectReferenceValue = GetMatching(all, property.name);
                                        }
                                    }
                                }
                            }

                            if (!property.NextVisible(true))
                                break;
                        }
                    }

                    serialized.ApplyModifiedProperties();
                }
            }, -100000f, new OnInspectorGUIAttribute("InjectedAutoAssign"));
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

        private static TO GetMatching<TO>(IEnumerable<TO> all, string targetName) where TO : Object
        {
            return all.Select(s => new Tuple<TO, int>(s, GetScore(s.name, targetName)))
                .OrderByDescending(t => t.Item2)
                .FirstOrDefault()?.Item1;
        }

        private static int GetScore(string name, string target)
        {
            int score = 0;

            var nameParts = name.SplitPascalCase().Split(' ');
            var targetParts = target.SplitPascalCase().Split(' ');

            foreach (var namePart in nameParts)
            {
                foreach (var targetPart in targetParts)
                {
                    if (namePart.Equals(targetPart, StringComparison.CurrentCulture))
                        score += 10;
                    else if (namePart.Equals(targetPart, StringComparison.InvariantCulture))
                        score += 5;
                    else if (namePart.Equals(targetPart, StringComparison.InvariantCultureIgnoreCase))
                        score += 2;
                }
            }

            return score;
        }
    }
}