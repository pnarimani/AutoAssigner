#if ODIN_INSPECTOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AutoAssigner
{
    [ResolverPriority(-12.0)]
    public class OdinInspectorPropertyProcessor<T> : OdinPropertyProcessor<T> where T : Object
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
        {
            if (Property.GetAttribute<NoAutoAssignAttribute>() != null)
                return;

            memberInfos.AddDelegate("InjectedAutoAssign", () =>
            {
                if (!GUILayout.Button("Auto Assign")) return;

                foreach (Object v in Property.ValueEntry.WeakValues)
                {
                    var serialized = new SerializedObject(v);
                    Assigner.AssignObjectProperties(serialized);
                }
            }, -100000f, new OnInspectorGUIAttribute("InjectedAutoAssign"));
        }
    }
}
#endif