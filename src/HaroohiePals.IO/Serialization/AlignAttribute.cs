using System;

namespace HaroohiePals.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class AlignAttribute : Attribute
    {
        public int Alignment { get; }

        public AlignAttribute(int alignment)
        {
            Alignment = alignment;
        }
    }
}