using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate;

class MkdsMapDataReferenceSerializer :
    IReferenceSerializer<MkdsPath, int>,
    IReferenceSerializer<MkdsRespawnPoint, int>,
    IReferenceSerializer<MkdsCheckPointPath, int>,
    IReferenceSerializer<MkdsItemPoint, int>,
    IReferenceSerializer<MkdsItemPath, int>,
    IReferenceSerializer<MkdsEnemyPoint, int>,
    IReferenceSerializer<MkdsEnemyPath, int>,
    IReferenceSerializer<MkdsMgEnemyPoint, int>,
    IReferenceSerializer<MkdsCamera, int>
{
    private const string CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE = "Cannot serialize unresolved reference";

    private readonly MkdsMapData _mapData;

    public MkdsMapDataReferenceSerializer(MkdsMapData mapData)
    {
        _mapData = mapData;
    }

    public int Serialize(Reference<MkdsPath> reference)
        => Serialize(reference, _mapData.Paths);

    public int Serialize(Reference<MkdsRespawnPoint> reference)
        => Serialize(reference, _mapData.RespawnPoints);

    public int Serialize(Reference<MkdsCheckPointPath> reference)
        => Serialize(reference, _mapData.CheckPointPaths);

    public int Serialize(Reference<MkdsItemPoint> reference)
        => SerializeConnectedPathPoint(reference, _mapData.ItemPaths);

    public int Serialize(Reference<MkdsItemPath> reference)
        => Serialize(reference, _mapData.ItemPaths);

    public int Serialize(Reference<MkdsEnemyPoint> reference)
        => SerializeConnectedPathPoint(reference, _mapData.EnemyPaths);

    public int Serialize(Reference<MkdsEnemyPath> reference)
        => Serialize(reference, _mapData.EnemyPaths);

    public int Serialize(Reference<MkdsMgEnemyPoint> reference)
    {
        if (reference == null)
            throw new NullReferenceException(nameof(reference));
        if (!reference.IsResolved)
            throw new ArgumentException(CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE, nameof(reference));
        if (_mapData.MgEnemyPaths is null)
            throw new SerializationException();

        int index = _mapData.MgEnemyPaths.SelectMany(x => x.Points).ToList().IndexOf(reference.Target);
        if (index < 0)
            throw new SerializationException();

        return index;
    }

    public int Serialize(Reference<MkdsCamera> reference)
        => Serialize(reference, _mapData.Cameras);

    private static int Serialize<T>(Reference<T> reference, MapDataCollection<T> collection)
        where T : IMapDataEntry, IReferenceable<T>
    {
        if (reference == null)
            throw new NullReferenceException(nameof(reference));
        if (!reference.IsResolved)
            throw new ArgumentException(CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE, nameof(reference));
        if (collection is null)
            throw new SerializationException();
        int index = collection.IndexOf(reference.Target);
        if (index < 0)
            throw new SerializationException();

        return index;
    }

    private static int SerializeConnectedPathPoint<TPath, TPoint>(Reference<TPoint> reference, MapDataCollection<TPath> collection)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, IReferenceable<TPoint>
    {
        if (reference == null)
            throw new NullReferenceException(nameof(reference));
        if (!reference.IsResolved)
            throw new ArgumentException(CANNOT_SERIALIZE_UNRESOLVED_EXCEPTION_MESSAGE, nameof(reference));
        if (collection is null)
            throw new SerializationException();

        int index = collection.SelectMany(x => x.Points).ToList().IndexOf(reference.Target);
        if (index < 0)
            throw new SerializationException();

        return index;
    }
}