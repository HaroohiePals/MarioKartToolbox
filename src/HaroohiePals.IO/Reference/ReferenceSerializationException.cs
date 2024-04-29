using System;

namespace HaroohiePals.IO.Reference;

/// <summary>
/// Exception thrown when reference serialization fails.
/// </summary>
public class ReferenceSerializationException : Exception
{
    public ReferenceSerializationException() { }

    public ReferenceSerializationException(string message)
        : base(message) { }

    public ReferenceSerializationException(string message, Exception inner)
        : base(message, inner) { }
}