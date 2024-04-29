using System;

namespace HaroohiePals.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ConstantAttribute : Attribute
    {
        public object Value { get; }

        public ConstantAttribute(object value)
        {
            Value = value;
        }
    }
}
