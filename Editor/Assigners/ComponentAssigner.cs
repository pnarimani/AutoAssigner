using System;
using AutoAssigner.Providers;
using AutoAssigner.Scoring;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Assigners
{
    public class ComponentAssigner : ISubAssigner
    {
        public bool TryAssign(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            if (property.isArray || property.IsArrayElement())
                return false;

            property.GetFieldInfoAndStaticType(out Type fieldType);

            if (!typeof(Component).IsAssignableFrom(fieldType))
                return false;

            if (property.objectReferenceValue != null)
                return true;

            var isComponent = property.serializedObject.targetObject is Component;
            if (!property.HasPrefabInTheName() && isComponent)
            {
                var root = (Component)property.serializedObject.targetObject;
                Component[] children = root.GetComponentsInChildren(fieldType, true);

                if (children.Length != 0)
                {
                    (property.objectReferenceValue, _) = NameProcessor.GetMatching(children, property.GetTargetName());
                    if (property.objectReferenceValue != null)
                        return true;
                }
            }

            property.objectReferenceValue = PrefabProvider.GetOne(fieldType, property.GetTargetName());
            return true;
        }
    }
}