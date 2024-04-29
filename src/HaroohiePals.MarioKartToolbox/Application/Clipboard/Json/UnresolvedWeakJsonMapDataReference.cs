#nullable enable
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

class UnresolvedWeakJsonMapDataReference<T> : UnresolvedWeakJsonReference<T>
    where T : IReferenceable<T>, IMapDataEntry
{
    public UnresolvedWeakJsonMapDataReference(int index)
        : base(index) { }

    public override Reference<T> ToWeakMapDataReference(T target)
    {
        return new WeakMapDataReference<T>(target);
    }
}