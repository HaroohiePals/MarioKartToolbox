using System;

namespace HaroohiePals.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute
    {
        public IgnoreAttribute() { }
    }
}