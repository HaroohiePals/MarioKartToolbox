using System;

namespace HaroohiePals.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class Fx32Attribute : Attribute
    {
        public Fx32Attribute() { }
    }
}