using System;

namespace HaroohiePals.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ArraySizeAttribute : Attribute
    {
        public int FixedSize { get; }
        public string SizeField { get; }

        public ArraySizeAttribute(int fixedSize)
        {
            FixedSize = fixedSize;
        }

        public ArraySizeAttribute(string sizeField)
        {
            FixedSize = -1;
            SizeField = sizeField;
        }
    }
}
