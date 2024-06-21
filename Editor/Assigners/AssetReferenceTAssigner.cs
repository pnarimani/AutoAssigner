#if UNITY_ADDRESSABLES
using System;
using AutoAssigner.Providers;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace AutoAssigner.Assigners
{
    public class AssetReferenceTAssigner : ISubAssigner
    {
        public bool TryAssign(SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.Generic)
                return false;

            if (property.isArray || property.IsArrayElement())
                return false;

            property.GetFieldInfoAndStaticType(out Type fieldType);
            
            if (!IsAssignableToGenericType(fieldType, typeof(AssetReferenceT<>)))
                return false;

            var obj = property.GetObject();
            if (obj is not AssetReference assetRef)
                return false;

            if (assetRef.editorAsset != null)
                return true;

            var genericType = GetGenericType(fieldType);
            if (genericType == null)
                return false;
            
            var targetName = $"{genericType.Name} {property.name}";
            var targetObj = ObjectProvider.GetOne(genericType, targetName);
            if (targetObj == null)
                targetObj = PrefabProvider.GetOne(genericType, targetName);

            assetRef.SetEditorAsset(targetObj);
            assetRef.SetEditorSubObject(targetObj);
            
            //We must Update SerializedObject to read new values
            property.serializedObject.Update();
            
            return true;
        }

        //It's tricky to detect both AssetReferenceT<Sprite> and AssetReferenceSprite
        private bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            if (givenType == null || genericType == null)
                return false;

            if (givenType.IsGenericType && 
                genericType.IsAssignableFrom(givenType.GetGenericTypeDefinition()))
                return true;

            var baseType = givenType.BaseType;
            return IsAssignableToGenericType(baseType, genericType);
        }

        //Return Sprite for both AssetReferenceT<Sprite> and AssetReferenceSprite
        private Type GetGenericType(Type givenType)
        {
            if (givenType == null)
                return null;

            if (givenType.IsGenericType)
            {
                var genericArguments = givenType.GetGenericArguments();
                return genericArguments.Length == 0 ? null : genericArguments[0];
            }
            
            var baseType = givenType.BaseType;
            return GetGenericType(baseType);
        }
    }
}
#endif