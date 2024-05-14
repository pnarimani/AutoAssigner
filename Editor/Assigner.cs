using AutoAssigner.Assigners;
using AutoAssigner.Diagnostics;
using UnityEditor;

namespace AutoAssigner
{
    public static class Assigner
    {
        private static readonly ISubAssigner[] _assigners =
        {
            new ComponentAssigner(),
            new GameObjectAssigner(),
            new ObjectAssigner(),
            new ComponentArrayAssigner(),
            new ObjectArrayAssigner(),
        };

        public static void AssignObjectProperties(SerializedObject obj)
        {
            SerializedProperty property = obj.GetIterator();
            if (property.NextVisible(true))
            {
                while (true)
                {
                    Logger.Log($"Assigning property: {property.propertyPath}");

                    foreach (ISubAssigner assigner in _assigners)
                    {
                        using (new Timer($"Assigner `{assigner.GetType().Name}` for `{property.propertyPath}`"))
                        {
                            if (assigner.TryAssign(property))
                            {
                                Logger.Log($"Property `{property.propertyPath}` assigned by {assigner.GetType().Name}");
                                break;
                            }
                        }
                    }

                    if (!property.NextVisible(true))
                        break;
                }
            }

            obj.ApplyModifiedProperties();
        }
    }
}