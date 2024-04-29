using System;

namespace HaroohiePals.IO.Serialization
{
    public enum ReferenceType
    {
        Absolute,
        ChunkRelative,
        FieldRelative
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ReferenceAttribute : Attribute
    {
        public ReferenceType Type             { get; }
        public FieldType     PointerFieldType { get; }
        public int           Offset           { get; }

        public ReferenceAttribute(ReferenceType type, FieldType pointerFieldType = FieldType.U32, int offset = 0)
        {
            Type             = type;
            PointerFieldType = pointerFieldType;
            Offset           = offset;
        }
    }
}