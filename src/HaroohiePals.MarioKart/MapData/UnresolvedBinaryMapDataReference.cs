using HaroohiePals.IO.Reference;

namespace HaroohiePals.MarioKart.MapData;

public class UnresolvedBinaryMapDataReference<T> : Reference<T>
    where T : IReferenceable<T>, IMapDataEntry
{
    public override bool IsResolved => false;

    public int UnresolvedId { get; }

    public UnresolvedBinaryMapDataReference(int unresolvedId)
    {
        UnresolvedId = unresolvedId;
    }
}