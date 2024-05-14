using UnityEditor;

namespace AutoAssigner.Assigners
{
    internal interface ISubAssigner
    {
        bool TryAssign(SerializedProperty property);
    }
}