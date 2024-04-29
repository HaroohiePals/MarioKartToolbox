using HaroohiePals.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.Actions;

public static class InsertMkdsMapDataCollectionItemsActionFactory
{
    public static IAction Create(MkdsMapData mapData, IMapDataEntry entry, IMapDataCollection targetCollection, int targetIndex = 0) => Create(mapData, new[] { entry }, targetCollection, targetIndex);

    public static IAction Create(MkdsMapData mapData, IEnumerable<IMapDataEntry> entries, IMapDataCollection targetCollection, int targetIndex = 0)
    {
        switch (targetCollection)
        {
            case MapDataCollection<MkdsMapObject> mapObjects:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsMapObject>(mapData, entries.Cast<MkdsMapObject>(), mapObjects, targetIndex);
            case MapDataCollection<MkdsStartPoint> startPoints:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsStartPoint>(mapData, entries.Cast<MkdsStartPoint>(), startPoints, targetIndex);
            case MapDataCollection<MkdsRespawnPoint> respawnPoints:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsRespawnPoint>(mapData, entries.Cast<MkdsRespawnPoint>(), respawnPoints, targetIndex);
            case MapDataCollection<MkdsKartPoint2d> ktp2d:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsKartPoint2d>(mapData, entries.Cast<MkdsKartPoint2d>(), ktp2d, targetIndex);
            case MapDataCollection<MkdsCannonPoint> cannonPoints:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsCannonPoint>(mapData, entries.Cast<MkdsCannonPoint>(), cannonPoints, targetIndex);
            case MapDataCollection<MkdsKartPointMission> ktpm:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsKartPointMission>(mapData, entries.Cast<MkdsKartPointMission>(), ktpm, targetIndex);
            case MapDataCollection<MkdsCamera> cameras:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsCamera>(mapData, entries.Cast<MkdsCamera>(), cameras, targetIndex);
            case MapDataCollection<MkdsArea> areas:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsArea>(mapData, entries.Cast<MkdsArea>(), areas, targetIndex);

            case MapDataCollection<MkdsPath> paths:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsPath>(mapData, entries.Cast<MkdsPath>(), paths, targetIndex);
            case MapDataCollection<MkdsPathPoint> path:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsPathPoint>(mapData, entries.Cast<MkdsPathPoint>(), path, targetIndex);

            case MapDataCollection<MkdsItemPath> itemPaths:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsItemPath>(mapData, entries.Cast<MkdsItemPath>(), itemPaths, targetIndex);
            case MapDataCollection<MkdsItemPoint> ItemPath:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsItemPoint>(mapData, entries.Cast<MkdsItemPoint>(), ItemPath, targetIndex);

            case MapDataCollection<MkdsEnemyPath> enemyPaths:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsEnemyPath>(mapData, entries.Cast<MkdsEnemyPath>(), enemyPaths, targetIndex);
            case MapDataCollection<MkdsEnemyPoint> enemyPath:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsEnemyPoint>(mapData, entries.Cast<MkdsEnemyPoint>(), enemyPath, targetIndex);

            case MapDataCollection<MkdsMgEnemyPath> mgEnemyPaths:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsMgEnemyPath>(mapData, entries.Cast<MkdsMgEnemyPath>(), mgEnemyPaths, targetIndex);
            case MapDataCollection<MkdsMgEnemyPoint> mgEnemyPath:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsMgEnemyPoint>(mapData, entries.Cast<MkdsMgEnemyPoint>(), mgEnemyPath, targetIndex);

            case MapDataCollection<MkdsCheckPointPath> checkPointPaths:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsCheckPointPath>(mapData, entries.Cast<MkdsCheckPointPath>(), checkPointPaths, targetIndex);
            case MapDataCollection<MkdsCheckPoint> checkPointPath:
                return new InsertMkdsMapDataCollectionItemsAction<MkdsCheckPoint>(mapData, entries.Cast<MkdsCheckPoint>(), checkPointPath, targetIndex);

            default:
                throw new Exception("The supplied Collection isn't supported");
        }
    }
}
