#if !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace AutoAssigner
{
    [CustomEditor(typeof(Object), true)]
    [CanEditMultipleObjects]
    public class AutoAssignerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Auto Assign"))
            {
                foreach (Object o in targets)
                    Assigner.AssignObjectProperties(new SerializedObject(o));
            }

            base.OnInspectorGUI();
        }
    }
}
#endif