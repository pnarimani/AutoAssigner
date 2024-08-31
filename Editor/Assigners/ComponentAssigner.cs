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

            var propertyId = property.GetTargetName();
            
            if (!property.HasPrefabInTheName())
            {
                var root = (Component)property.serializedObject.targetObject;
                Component[] children = root.GetComponentsInChildren(fieldType, true);

                if (children.Length != 0)
                {
                    property.objectReferenceValue = NameProcessor.GetMatching(children, propertyId);
                }
            }
            else
            {
                property.objectReferenceValue = PrefabProvider.GetOne(fieldType, propertyId);
            }

            return true;
        }
    }
}