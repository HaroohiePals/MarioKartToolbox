using HaroohiePals.IO.Reference;

namespace HaroohiePals.MarioKart.MapData;

public interface IMapDataEntry
{
    void ResolveReferences(IReferenceResolverCollection resolverCollection);
    void ReleaseReferences();
    IMapDataEntry Clone();
}