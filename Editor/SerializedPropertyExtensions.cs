using System;
using System.Reflection;
using UnityEditor;

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
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly.GetTypes())
                    {
                        if (t.Name == "ScriptAttributeUtility")
                        {
                            MethodInfo mi = t.GetMethod("GetFieldInfoAndStaticTypeFromProperty",
                                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                            _getFieldInfoAndStaticTypeFromProperty =
                                (GetFieldInfoAndStaticTypeFromProperty) Delegate.CreateDelegate(
                                    typeof(GetFieldInfoAndStaticTypeFromProperty), mi);
                            break;
                        }
                    }

                    if (_getFieldInfoAndStaticTypeFromProperty != null) break;
                }

                if (_getFieldInfoAndStaticTypeFromProperty == null)
                {
                    UnityEngine.Debug.LogError("GetFieldInfoAndStaticType::Reflection failed!");
                    type = null;
                    return null;
                }
            }

            return _getFieldInfoAndStaticTypeFromProperty(prop, out type);
        }

        public static T GetCustomAttributeFromProperty<T>(this SerializedProperty prop) where T : System.Attribute
        {
            var info = prop.GetFieldInfoAndStaticType(out _);
            return info.GetCustomAttribute<T>();
        }
    }
}