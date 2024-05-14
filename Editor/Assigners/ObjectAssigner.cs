using System;
using AutoAssigner.Providers;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AutoAssigner.Assigners
{
    public class ObjectAssigner : ISubAssigner
    {
        public bool TryAssign(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return false;

            if (property.isArray || property.IsArrayElement())
                return false;

            property.GetFieldInfoAndStaticType(out Type fieldType);

            if (!typeof(Object).IsAssignableFrom(fieldType))
                return false;

            if (property.objectReferenceValue != null)
                return true;

            property.objectReferenceValue = ObjectProvider.GetOne(fieldType, property.GetTargetName());

            return true;
        }
    }
}