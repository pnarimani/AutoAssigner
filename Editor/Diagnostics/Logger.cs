using System.Diagnostics;

namespace AutoAssigner.Diagnostics
{
    internal static class Logger
    {
        [Conditional("AUTO_ASSIGNER_DIAGNOSTICS")]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }
    }
}