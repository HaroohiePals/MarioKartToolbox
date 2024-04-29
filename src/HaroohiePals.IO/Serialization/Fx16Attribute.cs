using System;

namespace HaroohiePals.IO.Serialization
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class Fx16Attribute : Attribute
    {
        public Fx16Attribute() { }
    }
}