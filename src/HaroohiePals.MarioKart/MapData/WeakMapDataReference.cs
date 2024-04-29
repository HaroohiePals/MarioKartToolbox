using HaroohiePals.IO.Reference;

namespace HaroohiePals.MarioKart.MapData;

public class WeakMapDataReference<T> : Reference<T>
    where T : IReferenceable<T>, IMapDataEntry
{
    public override bool IsResolved => false;

    public WeakMapDataReference(T target) : base(target, null) { }
}