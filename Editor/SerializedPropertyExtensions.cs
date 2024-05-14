using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner
{
    internal static class SerializedPropertyExtensions
    {
        private delegate FieldInfo GetFieldInfoAndStaticTypeFromProperty(SerializedProperty property, out Type type);

        private static GetFieldInfoAndStaticTypeFromProperty _getFieldInfoAndStaticTypeFromProperty;

        public static FieldInfo GetFieldInfoAndStaticType(this SerializedProperty prop, out Type type)
        {
            if (_getFieldInfoAndStaticTypeFromProperty == null)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (Type t in assembly.GetTypes())
                    {
                        if (t.Name != "ScriptAttributeUtility")
                            continue;

                        MethodInfo methodInfo = t.GetMethod("GetFieldInfoAndStaticTypeFromProperty",
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                        if (methodInfo == null)
                            throw new Exception("Failed to find GetFieldInfoAndStaticTypeFromProperty method");

                        _getFieldInfoAndStaticTypeFromProperty =
                            (GetFieldInfoAndStaticTypeFromProperty)Delegate.CreateDelegate(
                                typeof(GetFieldInfoAndStaticTypeFromProperty), methodInfo);
                        break;
                    }

                    if (_getFieldInfoAndStaticTypeFromProperty != null) break;
                }

                if (_getFieldInfoAndStaticTypeFromProperty == null)
                {
                    Debug.LogError("GetFieldInfoAndStaticType::Reflection failed!");
                    type = null;
                    return null;
                }
            }

            return _getFieldInfoAndStaticTypeFromProperty(prop, out type);
        }

        public static Type GetArrayElementType(this SerializedProperty property)
        {
            // Nasty trick incoming
            // There is no way to get the type of the element in an array directly from the array serialized property
            // So I add an element to the array, get the type of it and then remove it from the array.
            property.InsertArrayElementAtIndex(0);
            SerializedProperty elementProperty = property.GetArrayElementAtIndex(0);
            elementProperty.GetFieldInfoAndStaticType(out Type element);
            property.DeleteArrayElementAtIndex(0);
            return element;
        }

        public static bool HasPrefabInTheName(this SerializedProperty property)
            => property.name.Contains("prefab", StringComparison.InvariantCultureIgnoreCase);

        public static string GetTargetName(this SerializedProperty property)
        {
            SerializedObject obj = property.serializedObject;
            Type serializedObjectType = obj.targetObject.GetType();
            return $"{obj.targetObject.name} {serializedObjectType.Name} {property.name}";
        }

        public static bool IsArrayElement(this SerializedProperty property)
        {
            return property.propertyPath.Contains(".Array.")
                   && property.propertyPath.Contains("[")
                   && property.propertyPath.Contains("]");
        }
    }
}