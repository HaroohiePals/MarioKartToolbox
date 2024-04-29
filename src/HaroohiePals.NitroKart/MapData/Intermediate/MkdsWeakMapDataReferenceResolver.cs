using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Linq;

namespace HaroohiePals.NitroKart.MapData.Intermediate;

public class MkdsWeakMapDataReferenceResolver :
    IReferenceResolver<MkdsPath>,
    IReferenceResolver<MkdsRespawnPoint>,
    IReferenceResolver<MkdsCheckPointPath>,
    IReferenceResolver<MkdsItemPoint>,
    IReferenceResolver<MkdsItemPath>,
    IReferenceResolver<MkdsEnemyPoint>,
    IReferenceResolver<MkdsEnemyPath>,
    IReferenceResolver<MkdsMgEnemyPoint>,
    IReferenceResolver<MkdsCamera>
{
    private readonly MkdsMapData _mapData;

    public MkdsWeakMapDataReferenceResolver(MkdsMapData mapData)
    {
        _mapData = mapData;
    }

    public Reference<MkdsPath> Resolve(Reference<MkdsPath> reference, Reference<MkdsPath>.RemoveReferenceFunc removeFunc)
        => Resolve(reference, removeFunc, _mapData.Paths);

    public Reference<MkdsRespawnPoint> Resolve(Reference<MkdsRespawnPoint> reference,
        Reference<MkdsRespawnPoint>.RemoveReferenceFunc removeFunc)
        => Resolve(reference, removeFunc, _mapData.RespawnPoints);

    public Reference<MkdsCheckPointPath> Resolve(Reference<MkdsCheckPointPath> reference,
        Reference<MkdsCheckPointPath>.RemoveReferenceFunc removeFunc)
        => Resolve(reference, removeFunc, _mapData.CheckPointPaths);

    public Reference<MkdsItemPoint> Resolve(Reference<MkdsItemPoint> reference,
        Reference<MkdsItemPoint>.RemoveReferenceFunc removeFunc)
        => ResolveConnectedPathPoint(reference, removeFunc, _mapData.ItemPaths);

    public Reference<MkdsItemPath> Resolve(Reference<MkdsItemPath> reference,
        Reference<MkdsItemPath>.RemoveReferenceFunc removeFunc)
        => Resolve(reference, removeFunc, _mapData.ItemPaths);

    public Reference<MkdsEnemyPoint> Resolve(Reference<MkdsEnemyPoint> reference,
        Reference<MkdsEnemyPoint>.RemoveReferenceFunc removeFunc)
        => ResolveConnectedPathPoint(reference, removeFunc, _mapData.EnemyPaths);

    public Reference<MkdsEnemyPath> Resolve(Reference<MkdsEnemyPath> reference,
        Reference<MkdsEnemyPath>.RemoveReferenceFunc removeFunc)
        => Resolve(reference, removeFunc, _mapData.EnemyPaths);


    //private static int GetUnresolvedId<T>(Reference<T> reference)
    //    where T : IMapDataEntry, IReferenceable<T>
    //{
    //    if (reference is UnresolvedBinaryMapDataReference<T> binaryReference)
    //        return binaryReference.UnresolvedId;
    //    else if (reference is UnresolvedXmlMapDataReference<T> xmlReference)
    //        return xmlReference.UnresolvedId;
    //    else
    //        throw new ReferenceResolveException();
    //}

    public Reference<MkdsCamera> Resolve(Reference<MkdsCamera> reference,
        Reference<MkdsCamera>.RemoveReferenceFunc removeFunc)
        => Resolve(reference, removeFunc, _mapData.Cameras);


    public Reference<MkdsMgEnemyPoint> Resolve(Reference<MkdsMgEnemyPoint> reference,
        Reference<MkdsMgEnemyPoint>.RemoveReferenceFunc removeFunc)
    {
        if (reference.Target is not null && _mapData.MgEnemyPaths is not null)
        {
            var allPoints = _mapData.MgEnemyPaths.SelectMany(x => x.Points).ToArray();
            if (allPoints.Contains(reference.Target))
                return reference.Target.GetReference(removeFunc);
        }

        return null;
    }

    private static Reference<T> Resolve<T>(Reference<T> reference, Reference<T>.RemoveReferenceFunc removeFunc,
        MapDataCollection<T> collection) where T : IMapDataEntry, IReferenceable<T>
    {
        if (reference.Target is not null && collection is not null && collection.Contains(reference.Target))
            return reference.Target.GetReference(removeFunc);

        return null;
    }

    private static Reference<TPoint> ResolveConnectedPathPoint<TPath, TPoint>(Reference<TPoint> reference,
        Reference<TPoint>.RemoveReferenceFunc removeFunc, MapDataCollection<TPath> collection)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, IReferenceable<TPoint>
    {
        if (reference.Target is not null && collection is not null)
        {
            var allPoints = collection.SelectMany(x => x.Points).ToArray();
            if (allPoints.Contains(reference.Target))
                return reference.Target.GetReference(removeFunc);
        }

        return null;
    }
}