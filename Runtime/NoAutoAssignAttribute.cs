using System;

namespace AutoAssigner
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public class NoAutoAssignAttribute : Attribute
    {
    }
}
