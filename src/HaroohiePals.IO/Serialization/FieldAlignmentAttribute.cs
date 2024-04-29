using System;

namespace HaroohiePals.IO.Serialization
{
    public enum FieldAlignment
    {
        Packed,
        FieldSize
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true, AllowMultiple = false)]
    public sealed class FieldAlignmentAttribute : Attribute
    {
        public FieldAlignment Alignment { get; }

        public FieldAlignmentAttribute(FieldAlignment alignment)
        {
            Alignment = alignment;
        }
    }
}
