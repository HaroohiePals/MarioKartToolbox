using System;

namespace HaroohiePals.IO.Reference;

/// <summary>
/// Exception thrown when reference resolving fails.
/// </summary>
public class ReferenceResolveException : Exception
{
    public ReferenceResolveException() { }

    public ReferenceResolveException(string message)
        : base(message) { }

    public ReferenceResolveException(string message, Exception inner)
        : base(message, inner) { }
}