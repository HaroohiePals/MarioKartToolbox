using HaroohiePals.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.Actions;

public static class DeleteMkdsMapDataCollectionItemsActionFactory
{
    public static IAction Create(MkdsMapData mapData, IEnumerable<IMapDataEntry> entries)
    {
        var actions = new List<IAction>();

        foreach (var group in entries.GroupBy(mapData.GetContainingCollection))
            actions.Add(Create(mapData, group, group.Key));

        if (actions.Count > 0)
            return new BatchAction(actions);

        return null;
    }

    public static IAction Create(MkdsMapData mapData, IMapDataEntry entry, IMapDataCollection sourceCollection) => Create(mapData, new[] { entry }, sourceCollection);

    public static IAction Create(MkdsMapData mapData, IEnumerable<IMapDataEntry> entries, IMapDataCollection sourceCollection)
    {
        switch (sourceCollection)
        {
            case MapDataCollection<MkdsMapObject> mapObjects:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsMapObject>(mapData, entries.Cast<MkdsMapObject>(), mapObjects);
            case MapDataCollection<MkdsStartPoint> startPoints:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsStartPoint>(mapData, entries.Cast<MkdsStartPoint>(), startPoints);
            case MapDataCollection<MkdsRespawnPoint> respawnPoints:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsRespawnPoint>(mapData, entries.Cast<MkdsRespawnPoint>(), respawnPoints);
            case MapDataCollection<MkdsKartPoint2d> ktp2d:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsKartPoint2d>(mapData, entries.Cast<MkdsKartPoint2d>(), ktp2d);
            case MapDataCollection<MkdsCannonPoint> cannonPoints:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsCannonPoint>(mapData, entries.Cast<MkdsCannonPoint>(), cannonPoints);
            case MapDataCollection<MkdsKartPointMission> ktpm:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsKartPointMission>(mapData, entries.Cast<MkdsKartPointMission>(), ktpm);
            case MapDataCollection<MkdsCamera> cameras:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsCamera>(mapData, entries.Cast<MkdsCamera>(), cameras);
            case MapDataCollection<MkdsArea> areas:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsArea>(mapData, entries.Cast<MkdsArea>(), areas);

            case MapDataCollection<MkdsPath> paths:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsPath>(mapData, entries.Cast<MkdsPath>(), paths);
            case MapDataCollection<MkdsPathPoint> path:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsPathPoint>(mapData, entries.Cast<MkdsPathPoint>(), path);

            case MapDataCollection<MkdsItemPath> itemPaths:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsItemPath>(mapData, entries.Cast<MkdsItemPath>(), itemPaths);
            case MapDataCollection<MkdsItemPoint> ItemPath:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsItemPoint>(mapData, entries.Cast<MkdsItemPoint>(), ItemPath);

            case MapDataCollection<MkdsEnemyPath> enemyPaths:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsEnemyPath>(mapData, entries.Cast<MkdsEnemyPath>(), enemyPaths);
            case MapDataCollection<MkdsEnemyPoint> enemyPath:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsEnemyPoint>(mapData, entries.Cast<MkdsEnemyPoint>(), enemyPath);

            case MapDataCollection<MkdsMgEnemyPath> mgEnemyPaths:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsMgEnemyPath>(mapData, entries.Cast<MkdsMgEnemyPath>(), mgEnemyPaths);
            case MapDataCollection<MkdsMgEnemyPoint> mgEnemyPath:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsMgEnemyPoint>(mapData, entries.Cast<MkdsMgEnemyPoint>(), mgEnemyPath);

            case MapDataCollection<MkdsCheckPointPath> checkPointPath:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsCheckPointPath>(mapData, entries.Cast<MkdsCheckPointPath>(), checkPointPath);
            case MapDataCollection<MkdsCheckPoint> checkPointPath:
                return new DeleteMkdsMapDataCollectionItemsAction<MkdsCheckPoint>(mapData, entries.Cast<MkdsCheckPoint>(), checkPointPath);

            default:
                throw new Exception("The supplied Collection isn't supported");
        }
    }
}
