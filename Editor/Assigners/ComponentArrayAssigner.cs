using System;
using System.Collections.Generic;
using System.Linq;
using AutoAssigner.Providers;
using UnityEditor;
using UnityEngine;

namespace AutoAssigner.Assigners
{
    public class ComponentArrayAssigner : ISubAssigner
    {
        public bool TryAssign(SerializedProperty property)
        {
            if (!property.isArray)
                return false;

            Type element = property.GetArrayElementType();

            if (!typeof(Component).IsAssignableFrom(element))
                return false;

            if (property.arraySize != 0)
            {
                // Do nothing to arrays that already have value
                return true;
            }

            List<Component> all;

            var isComponent = property.serializedObject.targetObject is Component;
            if (!property.HasPrefabInTheName() && isComponent)
            {
                var root = (Component)property.serializedObject.targetObject;
                all = root.GetComponentsInChildren(element, true).ToList();

                if (all.Count == 0)
                    all = PrefabProvider.GetAll(element, property.GetTargetName());
            }
            else
            {
                all = PrefabProvider.GetAll(element, property.GetTargetName());
            }

            property.arraySize = all.Count;

            for (var i = 0; i < all.Count; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
            }

            return true;
        }
    }
}