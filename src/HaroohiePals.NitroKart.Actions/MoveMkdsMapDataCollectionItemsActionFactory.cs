using HaroohiePals.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.Actions;

public static class MoveMkdsMapDataCollectionItemsActionFactory
{
    public static IAction Create(IEnumerable<IMapDataEntry> entries, IMapDataCollection sourceCollection, IMapDataCollection targetCollection, int targetIndex = 0)
    {
        if (sourceCollection.GetType() != targetCollection.GetType())
            throw new Exception("Source and Target Collection types don't match!");

        switch (sourceCollection)
        {
            case MapDataCollection<MkdsMapObject> mapObjects:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsMapObject>(entries.Cast<MkdsMapObject>(), mapObjects, targetIndex);
            case MapDataCollection<MkdsStartPoint> startPoints:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsStartPoint>(entries.Cast<MkdsStartPoint>(), startPoints, targetIndex);
            case MapDataCollection<MkdsRespawnPoint> respawnPoints:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsRespawnPoint>(entries.Cast<MkdsRespawnPoint>(), respawnPoints, targetIndex);
            case MapDataCollection<MkdsKartPoint2d> ktp2d:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsKartPoint2d>(entries.Cast<MkdsKartPoint2d>(), ktp2d, targetIndex);
            case MapDataCollection<MkdsCannonPoint> cannonPoints:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsCannonPoint>(entries.Cast<MkdsCannonPoint>(), cannonPoints, targetIndex);
            case MapDataCollection<MkdsKartPointMission> ktpm:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsKartPointMission>(entries.Cast<MkdsKartPointMission>(), ktpm, targetIndex);
            case MapDataCollection<MkdsCamera> cameras:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsCamera>(entries.Cast<MkdsCamera>(), cameras, targetIndex);
            case MapDataCollection<MkdsArea> areas:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsArea>(entries.Cast<MkdsArea>(), areas, targetIndex);

            case MapDataCollection<MkdsPath> paths:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsPath>(entries.Cast<MkdsPath>(), paths, targetIndex);
            case MapDataCollection<MkdsPathPoint> path:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsPathPoint>(entries.Cast<MkdsPathPoint>(), path, (MapDataCollection<MkdsPathPoint>)targetCollection, targetIndex);

            case MapDataCollection<MkdsItemPath> itemPaths:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsItemPath>(entries.Cast<MkdsItemPath>(), itemPaths, targetIndex);
            case MapDataCollection<MkdsItemPoint> ItemPath:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsItemPoint>(entries.Cast<MkdsItemPoint>(), ItemPath, (MapDataCollection<MkdsItemPoint>)targetCollection, targetIndex);

            case MapDataCollection<MkdsEnemyPath> enemyPaths:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsEnemyPath>(entries.Cast<MkdsEnemyPath>(), enemyPaths, targetIndex);
            case MapDataCollection<MkdsEnemyPoint> enemyPath:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsEnemyPoint>(entries.Cast<MkdsEnemyPoint>(), enemyPath, (MapDataCollection<MkdsEnemyPoint>)targetCollection, targetIndex);

            case MapDataCollection<MkdsMgEnemyPath> mgEnemyPaths:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsMgEnemyPath>(entries.Cast<MkdsMgEnemyPath>(), mgEnemyPaths, targetIndex);
            case MapDataCollection<MkdsMgEnemyPoint> mgEnemyPath:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsMgEnemyPoint>(entries.Cast<MkdsMgEnemyPoint>(), mgEnemyPath, (MapDataCollection<MkdsMgEnemyPoint>)targetCollection, targetIndex);

            case MapDataCollection<MkdsCheckPointPath> checkPointPaths:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsCheckPointPath>(entries.Cast<MkdsCheckPointPath>(), checkPointPaths, targetIndex);
            case MapDataCollection<MkdsCheckPoint> checkPointPath:
                return new MoveMkdsMapDataCollectionItemsAction<MkdsCheckPoint>(entries.Cast<MkdsCheckPoint>(), checkPointPath, (MapDataCollection<MkdsCheckPoint>)targetCollection, targetIndex);

            default:
                throw new Exception("The supplied Collection isn't supported");
        }
    }
}
