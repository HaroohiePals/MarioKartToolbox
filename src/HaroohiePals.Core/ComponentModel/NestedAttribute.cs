using System;

namespace HaroohiePals.Core.ComponentModel
{
    public enum NestType
    {
        None,
        Nested,
        Category
    }

    public class NestedAttribute : Attribute
    {
        public NestedAttribute(NestType type)
        {
            NestType = type;
        }

        public NestType NestType { get; set; }
    }
}
