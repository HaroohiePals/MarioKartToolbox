#nullable enable
using HaroohiePals.IO.Reference;
using System;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

abstract class UnresolvedWeakJsonReference<T> : Reference<T>
    where T : IReferenceable<T>
{
    public override bool IsResolved => false;

    public int Index { get; }

    public UnresolvedWeakJsonReference(int index)
    {
        Index = index;
    }

    public virtual Reference<T> ToWeakMapDataReference(T target)
    {
        throw new NotSupportedException();
    }
}