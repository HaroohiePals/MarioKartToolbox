#nullable enable
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

class WeakJsonMapDataReferenceResolver : IReferenceResolverCollection
{
    private readonly IReadOnlyList<IMapDataEntry> _mapDataEntries;

    public WeakJsonMapDataReferenceResolver(IReadOnlyList<IMapDataEntry> mapDataEntries)
    {
        _mapDataEntries = mapDataEntries;
    }

    public Reference<T> Resolve<T>(Reference<T> reference, Reference<T>.RemoveReferenceFunc removeFunc)
        where T : IReferenceable<T>
    {
        if (reference is UnresolvedWeakJsonReference<T> unresolvedWeakJsonReference)
        {
            return unresolvedWeakJsonReference.ToWeakMapDataReference(
                (T)_mapDataEntries[unresolvedWeakJsonReference.Index]);
        }

        return reference;
    }
}