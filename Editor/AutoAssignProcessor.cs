using System;
using System.Collections.Generic;
using AutoAssigner.Providers;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoAssigner
{
    [ResolverPriority(-12.0)]
    public class AutoAssignProcessor<T> : OdinPropertyProcessor<T> where T : Object
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
        {
            if (Property.GetAttribute<NoAutoAssignAttribute>() != null)
                return;
            
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
                    ScriptableObject[] all = ScriptableObjectProvider.GetAll(element);
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
                            all = PrefabProvider.GetAll(element);
                    }
                    else
                    {
                        all = PrefabProvider.GetAll(element);
                    }

                    property.arraySize = all.Length;

                    for (int i = 0; i < all.Length; i++)
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
                    property.objectReferenceValue = ScriptableObjectProvider.GetOne(fieldType, property.name);
                }
                else if (typeof(Component).IsAssignableFrom(fieldType))
                {
                    if (root != null && !hasPrefabInName)
                    {
                        Component[] children = root.GetComponentsInChildren(fieldType, true);

                        if (children.Length != 0)
                        {
                            property.objectReferenceValue = NameProcessor.GetMatching(children, property.name);
                            return;
                        }
                    }

                    property.objectReferenceValue = PrefabProvider.GetOne(fieldType, property.name);
                }
                else if (typeof(GameObject).IsAssignableFrom(fieldType))
                {
                    property.objectReferenceValue = PrefabProvider.GetOne(property.name);
                }
            }
        }
    }
}