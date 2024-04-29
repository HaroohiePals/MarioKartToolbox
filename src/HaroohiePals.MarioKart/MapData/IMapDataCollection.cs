using HaroohiePals.IO.Reference;
using System.Collections;

namespace HaroohiePals.MarioKart.MapData;

public interface IMapDataCollection : IEnumerable
{
    void ResolveReferences(IReferenceResolverCollection resolverCollection);
}