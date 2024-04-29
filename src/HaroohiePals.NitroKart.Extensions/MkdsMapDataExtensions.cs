using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.Extensions;

public static class MkdsMapDataExtensions
{
    public static string GetEntryDisplayName(this MkdsMapData mapData, IMapDataEntry entry)
    {
        entry.GetPathPointIndices(mapData, out int containerIndex, out int entryIndex);

        string entryLabel = "Point";
        string containerLabel = "Path";

        switch (entry)
        {
            case MkdsPath:
            case MkdsEnemyPath:
            case MkdsMgEnemyPath:
            case MkdsCheckPointPath:
            case MkdsItemPath:
                entryLabel = "Path";
                break;
            default:
                entryLabel = entry.GetDisplayName();
                break;
        }

        string name = $"{entryLabel} #{entryIndex}";

        if (containerIndex > -1)
            name = $"{containerLabel} #{containerIndex}: {name}";

        return name;
    }

    public static string GetDisplayName(this IMapDataCollection collection)
    {
        switch (collection)
        {
            case MapDataCollection<MkdsMgEnemyPoint>:
            case MapDataCollection<MkdsEnemyPoint>:
                return "Enemy Path";
            case MapDataCollection<MkdsItemPoint>:
                return "Item Path";
            case MapDataCollection<MkdsCheckPoint>:
                return "Checkpoint Path";
            case MapDataCollection<MkdsPathPoint>:
                return "Path";
            case MapDataCollection<MkdsMapObject> mapObjects:
                return "Map Objects";
            case MapDataCollection<MkdsStartPoint> startPoints:
                return "Start Points";
            case MapDataCollection<MkdsRespawnPoint> respawnPoints:
                return "Respawn Points";
            case MapDataCollection<MkdsKartPoint2d> ktp2d:
                return "Kart Point 2D";
            case MapDataCollection<MkdsCannonPoint> cannonPoints:
                return "Cannon Points";
            case MapDataCollection<MkdsKartPointMission> ktpm:
                return "Mission Points";
            case MapDataCollection<MkdsCamera> cameras:
                return "Cameras";
            case MapDataCollection<MkdsArea> areas:
                return "Areas";
            default:
                return "Collection";
        }
    }

    public static int IndexOf(this IMapDataCollection collection, IMapDataEntry obj)
        => collection.Cast<IMapDataEntry>().ToList().IndexOf(obj);

    public static int Count(this IMapDataCollection collection)
        => collection.Cast<IMapDataEntry>().Count();

    public static IMapDataEntry ConstructEntry(this IMapDataCollection collection)
    {
        switch (collection)
        {
            case MapDataCollection<MkdsMapObject>:
                return new MkdsMapObject();
            case MapDataCollection<MkdsStartPoint>:
                return new MkdsStartPoint();
            case MapDataCollection<MkdsRespawnPoint>:
                return new MkdsRespawnPoint();
            case MapDataCollection<MkdsKartPoint2d>:
                return new MkdsKartPoint2d();
            case MapDataCollection<MkdsCannonPoint>:
                return new MkdsCannonPoint();
            case MapDataCollection<MkdsKartPointMission>:
                return new MkdsKartPointMission();
            case MapDataCollection<MkdsCamera>:
                return new MkdsCamera();
            case MapDataCollection<MkdsArea>:
                return new MkdsArea();

            case MapDataCollection<MkdsPath>:
                return new MkdsPath();
            case MapDataCollection<MkdsPathPoint>:
                return new MkdsPathPoint();

            case MapDataCollection<MkdsItemPath>:
                return new MkdsItemPath();
            case MapDataCollection<MkdsItemPoint>:
                return new MkdsItemPoint();

            case MapDataCollection<MkdsEnemyPath>:
                return new MkdsEnemyPath();
            case MapDataCollection<MkdsEnemyPoint>:
                return new MkdsEnemyPoint();

            case MapDataCollection<MkdsMgEnemyPath>:
                return new MkdsMgEnemyPath();
            case MapDataCollection<MkdsMgEnemyPoint>:
                return new MkdsMgEnemyPoint();

            case MapDataCollection<MkdsCheckPointPath>:
                return new MkdsCheckPointPath();
            case MapDataCollection<MkdsCheckPoint>:
                return new MkdsCheckPoint();
        }

        throw new Exception();
    }

    public static IMapDataCollection GetContainingCollection(this MkdsMapData mapData, IMapDataEntry obj)
    {
        switch (obj)
        {
            case MkdsMapObject:
                return mapData.MapObjects;
            case MkdsStartPoint:
                return mapData.StartPoints;
            case MkdsRespawnPoint:
                return mapData.RespawnPoints;
            case MkdsKartPoint2d:
                return mapData.KartPoint2D;
            case MkdsCannonPoint:
                return mapData.CannonPoints;
            case MkdsKartPointMission:
                return mapData.KartPointMission;
            case MkdsCamera:
                return mapData.Cameras;
            case MkdsArea:
                return mapData.Areas;

            case MkdsPath:
                return mapData.Paths;
            case MkdsPathPoint pathPoint:
                return mapData.Paths.First(path => path.Points.Contains(pathPoint)).Points;

            case MkdsItemPath:
                return mapData.ItemPaths;
            case MkdsItemPoint itemPoint:
                return mapData.ItemPaths.First(path => path.Points.Contains(itemPoint)).Points;

            case MkdsEnemyPath:
                return mapData.EnemyPaths;
            case MkdsEnemyPoint enemyPoint:
                return mapData.EnemyPaths.First(path => path.Points.Contains(enemyPoint)).Points;

            case MkdsMgEnemyPath:
                return mapData.MgEnemyPaths;
            case MkdsMgEnemyPoint mgEnemyPoint:
                return mapData.MgEnemyPaths.First(path => path.Points.Contains(mgEnemyPoint)).Points;

            case MkdsCheckPointPath:
                return mapData.CheckPointPaths;
            case MkdsCheckPoint checkPoint:
                return mapData.CheckPointPaths.First(path => path.Points.Contains(checkPoint)).Points;
        }

        throw new Exception();
    }
}