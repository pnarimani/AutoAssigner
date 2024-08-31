using System;
using System.Diagnostics;
using Unity.Profiling;

namespace AutoAssigner.Diagnostics
{
    internal readonly struct Timer : IDisposable
    {
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private readonly string _name;
        private readonly Stopwatch _watch;
        private readonly ProfilerMarker _marker;
#pragma warning restore CS0414 // Field is assigned but its value is never used

        public Timer(string name)
        {
#if AUTO_ASSIGNER_DIAGNOSTICS
            _name = name;
            _watch = Stopwatch.StartNew();
            _marker = new ProfilerMarker(name);
            _marker.Begin();
#else
            _name = default;
            _watch = default;
            _marker = default;
#endif
        }

        public void Dispose()
        {
#if AUTO_ASSIGNER_DIAGNOSTICS
            _watch.Stop();
            _marker.End();
            UnityEngine.Debug.Log($"{_name} took {_watch.ElapsedMilliseconds}ms");
#endif
        }
    }
}