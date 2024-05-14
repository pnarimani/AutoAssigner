using System;
using AutoAssigner.Providers;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Assigners
{
    public class GameObjectAssigner : ISubAssigner
    {
        public bool TryAssign(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return false;
            
            if (property.isArray || property.IsArrayElement())
                return false;

            property.GetFieldInfoAndStaticType(out Type fieldType);

            if (!typeof(GameObject).IsAssignableFrom(fieldType))
                return false;

            if (property.objectReferenceValue != null)
                return true;
            
            property.objectReferenceValue = PrefabProvider.GetOne(property.GetTargetName());

            return true;
        }
    }
}