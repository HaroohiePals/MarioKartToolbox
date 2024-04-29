using HaroohiePals.Actions;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.Actions;

public class InsertMkdsMapDataCollectionItemsAction<T> : IAction where T : IMapDataEntry
{
    private readonly MkdsMapData _mapData;
    private readonly IReadOnlyList<T> _entries;
    private readonly MapDataCollection<T> _target;
    private readonly int _targetIndex;

    public bool IsCreateDelete => true;

    public InsertMkdsMapDataCollectionItemsAction(MkdsMapData mapData, T entry, MapDataCollection<T> target, int targetIndex = 0) : this(mapData, new[] { entry }, target, targetIndex) { }

    public InsertMkdsMapDataCollectionItemsAction(MkdsMapData mapData, IEnumerable<T> entries, MapDataCollection<T> target, int targetIndex = 0)
    {
        _mapData = mapData;
        _entries = entries.ToArray();
        _target = target;
        _targetIndex = targetIndex;
    }

    public void Do()
    {
        int insertIndex = _targetIndex;

        foreach (var entry in _entries)
            _target.Insert(insertIndex++, entry);

        ResolveWeakReferences();
    }

    private void ResolveWeakReferences()
    {
        //todo: Move this logic somewhere else later

        var resolverCollection = new ReferenceResolverCollection();
        var referenceResolver = new MkdsWeakMapDataReferenceResolver(_mapData);
        resolverCollection.RegisterResolver<MkdsPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsRespawnPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsCheckPointPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsItemPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsItemPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsEnemyPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsEnemyPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsMgEnemyPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsCamera>(referenceResolver);

        _mapData.MapObjects?.ResolveReferences(resolverCollection);
        _mapData.Paths?.ResolveReferences(resolverCollection);
        _mapData.RespawnPoints?.ResolveReferences(resolverCollection);
        _mapData.CannonPoints?.ResolveReferences(resolverCollection);
        _mapData.CheckPointPaths?.ResolveReferences(resolverCollection);
        _mapData.ItemPaths?.ResolveReferences(resolverCollection);
        _mapData.EnemyPaths?.ResolveReferences(resolverCollection);
        _mapData.MgEnemyPaths?.ResolveReferences(resolverCollection);
        _mapData.Areas?.ResolveReferences(resolverCollection);
        _mapData.Cameras?.ResolveReferences(resolverCollection);
    }

    public void Undo()
    {
        foreach (var entry in _entries)
            _target.Remove(entry);
    }
}