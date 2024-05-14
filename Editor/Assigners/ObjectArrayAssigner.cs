using System;
using System.Collections.Generic;
using AutoAssigner.Providers;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AutoAssigner.Assigners
{
    public class ObjectArrayAssigner : ISubAssigner
    {
        public bool TryAssign(SerializedProperty property)
        {
            if(!property.isArray)
                return false;
            
            Type element = property.GetArrayElementType();

            if (!typeof(Object).IsAssignableFrom(element))
                return false;
            
            List<Object> all = ObjectProvider.GetAll(element, property.GetTargetName());
                    
            property.arraySize = all.Count;
            for (var i = 0; i < all.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
            }

            return true;
        }
    }
}