using System;

namespace HaroohiePals.IO.Serialization
{
    public enum FieldType
    {
        U8,
        S8,
        U16,
        S16,
        U32,
        S32,
        U64,
        S64,
        Fx16,
        Fx32,
        Float,
        Double
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class TypeAttribute : Attribute
    {
        public FieldType Type { get; }

        public TypeAttribute(FieldType type)
        {
            Type = type;
        }
    }
}