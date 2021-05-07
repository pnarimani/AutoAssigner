using System;
using System.Collections.Generic;
using System.Linq;
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

            string targetName = typeof(T).Name + " " + property.name;

            if (typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
                targetName = obj.targetObject.name + " " + targetName;
            
            if (property.isArray)
            {
                if (property.arraySize != 0) return;

                // Nasty trick incoming
                property.InsertArrayElementAtIndex(0);
                SerializedProperty elementProperty = property.GetArrayElementAtIndex(0);
                elementProperty.GetFieldInfoAndStaticType(out Type element);
                property.DeleteArrayElementAtIndex(0);
                // Nasty trick over

                if (typeof(Component).IsAssignableFrom(element))
                {
                    List<Component> all;

                    if (root != null && !hasPrefabInName)
                    {
                        all = root.GetComponentsInChildren(element, true).ToList();

                        if (all.Count == 0)
                            all = PrefabProvider.GetAll(element, targetName);
                    }
                    else
                    {
                        all = PrefabProvider.GetAll(element, targetName);
                    }

                    property.arraySize = all.Count;

                    for (int i = 0; i < all.Count; i++)
                    {
                        property.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                    }
                }
                else if (typeof(Object).IsAssignableFrom(element))
                {
                    List<Object> all = ObjectProvider.GetAll(element, targetName);
                    
                    property.arraySize = all.Count;
                    for (var i = 0; i < all.Count; i++)
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

                if (typeof(Component).IsAssignableFrom(fieldType))
                {
                    if (root != null && !hasPrefabInName)
                    {
                        Component[] children = root.GetComponentsInChildren(fieldType, true);

                        if (children.Length != 0)
                        {
                            (property.objectReferenceValue, _) = NameProcessor.GetMatching(children, targetName);
                            return;
                        }
                    }

                    property.objectReferenceValue = PrefabProvider.GetOne(fieldType, targetName);
                }
                else if (typeof(GameObject).IsAssignableFrom(fieldType))
                {
                    property.objectReferenceValue = PrefabProvider.GetOne(targetName);
                }
                else if (typeof(Object).IsAssignableFrom(fieldType))
                {
                    property.objectReferenceValue = ObjectProvider.GetOne(fieldType, targetName);
                }
            }
        }
    }
}