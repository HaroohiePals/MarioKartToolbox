using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Extensions;

public static class MkdsMapDataEntryExtensions
{
    public static string GetDisplayName(this IMapDataEntry point) => point switch
    {
        MkdsCamera camera => $"{camera.Type}{(camera.FirstIntroCamera != MkdsCameIntroCamera.No ? $" (First {camera.FirstIntroCamera})" : "")}",
        MkdsArea area => $"{area.Shape} ({area.AreaType})",
        MkdsMapObject mobj => $"{mobj.ObjectId}",
        MkdsStartPoint => "Start Point",
        MkdsRespawnPoint => "Respawn Point",
        _ => "Point"
    };

    public static IMapDataCollection GetInnerCollection(this IMapDataEntry obj)
    {
        switch (obj)
        {
            case MkdsPath path:
                return path.Points;
            case MkdsItemPath itemPath:
                return itemPath.Points;
            case MkdsEnemyPath enemyPath:
                return enemyPath.Points;
            case MkdsMgEnemyPath mgEnemyPath:
                return mgEnemyPath.Points;
            case MkdsCheckPointPath checkpointPath:
                return checkpointPath.Points;
        }

        return null;
    }

    public static string GetSortableIndex(this IMapDataEntry entry, MkdsMapData mapData)
    {
        entry.GetPathPointIndices(mapData, out int pathIndex, out int pointIndex);

        return pathIndex > -1 ? $"{pathIndex:0000}_{pointIndex:0000}" : $"{pointIndex:0000}";
    }

    public static void GetPathPointIndices(this IMapDataEntry entry, MkdsMapData mapData, out int pathIndex, out int pointIndex)
    {
        pathIndex = -1;
        pointIndex = -1;

        switch (entry)
        {
            case MkdsMgEnemyPoint mepo:
                if (mapData.MgEnemyPaths == null)
                    return;
                var parentMepa = mapData.MgEnemyPaths.FirstOrDefault(x => x.Points.Contains(mepo));
                pathIndex = mapData.MgEnemyPaths.IndexOf(parentMepa);
                pointIndex = parentMepa.Points.IndexOf(mepo);
                break;
            case MkdsEnemyPoint epoi:
                if (mapData.EnemyPaths == null)
                    return;
                var parentEpat = mapData.EnemyPaths.FirstOrDefault(x => x.Points.Contains(epoi));
                pathIndex = mapData.EnemyPaths.IndexOf(parentEpat);
                pointIndex = parentEpat.Points.IndexOf(epoi);
                break;
            case MkdsItemPoint ipoi:
                if (mapData.ItemPaths == null)
                    return;
                var parentIpat = mapData.ItemPaths.FirstOrDefault(x => x.Points.Contains(ipoi));
                pathIndex = mapData.ItemPaths.IndexOf(parentIpat);
                pointIndex = parentIpat.Points.IndexOf(ipoi);
                break;
            case MkdsCheckPoint cpoi:
                if (mapData.CheckPointPaths == null)
                    return;
                var parentCpat = mapData.CheckPointPaths.FirstOrDefault(x => x.Points.Contains(cpoi));
                pathIndex = mapData.CheckPointPaths.IndexOf(parentCpat);
                pointIndex = parentCpat.Points.IndexOf(cpoi);
                break;
            case MkdsPathPoint point:
                if (mapData.Paths == null)
                    return;
                var parentPath = mapData.Paths.FirstOrDefault(x => x.Points.Contains(point));
                pathIndex = mapData.Paths.IndexOf(parentPath);
                pointIndex = parentPath.Points.IndexOf(point);
                break;
            default:
                var parentCollection = mapData.GetContainingCollection(entry);
                pointIndex = parentCollection.IndexOf(entry);
                return;
        }
    }

    public static MkdsItemPoint ToItemPoint(this MkdsEnemyPoint enemyPoint)
        => new MkdsItemPoint
        {
            Position = enemyPoint.Position,
            Radius = enemyPoint.Radius
        };

    public static MkdsItemPath ToItemPath(this MkdsEnemyPath enemyPath)
    {
        var itemPath = new MkdsItemPath();
        itemPath.Points.AddRange(enemyPath.Points.Select(x => x.ToItemPoint()));
        return itemPath;
    }

    public static IEnumerable<MkdsItemPath> ConvertToItemPaths(this IEnumerable<MkdsEnemyPath> enemyPaths)
    {
        var enemyPathsList = enemyPaths.ToList();
        var itemPaths = enemyPaths.Select(x => x.ToItemPath()).ToList();

        for (int i = 0; i < enemyPathsList.Count; i++)
        {
            var itemPath = itemPaths[i];
            var enemyPath = enemyPathsList[i];

            //Next
            foreach (var nextEnemyPath in enemyPath.Next)
            {
                var nextItemPath = itemPaths[enemyPathsList.IndexOf(nextEnemyPath?.Target)];
                itemPath.Next.Add(new WeakMapDataReference<MkdsItemPath>(nextItemPath));
            }
            //Prev
            foreach (var prevEnemyPath in enemyPath.Previous)
            {
                var prevItemPath = itemPaths[enemyPathsList.IndexOf(prevEnemyPath?.Target)];
                itemPath.Previous.Add(new WeakMapDataReference<MkdsItemPath>(prevItemPath));
            }
        }

        return itemPaths;
    }

    public static MkdsEnemyPoint ToEnemyPoint(this MkdsItemPoint enemyPoint)
        => new MkdsEnemyPoint
        {
            Position = enemyPoint.Position,
            Radius = enemyPoint.Radius
        };

    public static MkdsEnemyPath ToEnemyPath(this MkdsItemPath itemPath)
    {
        var enemyPath = new MkdsEnemyPath();
        enemyPath.Points.AddRange(itemPath.Points.Select(x => x.ToEnemyPoint()));
        return enemyPath;
    }

    public static IEnumerable<MkdsEnemyPath> ConvertToEnemyPaths(this IEnumerable<MkdsItemPath> itemPaths)
    {
        var itemPathsList = itemPaths.ToList();
        var enemyPaths = itemPaths.Select(x => x.ToEnemyPath()).ToList();

        for (int i = 0; i < itemPathsList.Count; i++)
        {
            var enemyPath = enemyPaths[i];
            var itemPath = itemPathsList[i];

            //Next
            foreach (var nextItemPath in itemPath.Next)
            {
                var nextEnemyPath = enemyPaths[itemPathsList.IndexOf(nextItemPath?.Target)];
                enemyPath.Next.Add(new WeakMapDataReference<MkdsEnemyPath>(nextEnemyPath));
            }
            //Prev
            foreach (var prevItemPath in itemPath.Previous)
            {
                var prevEnemyPath = enemyPaths[itemPathsList.IndexOf(prevItemPath?.Target)];
                enemyPath.Previous.Add(new WeakMapDataReference<MkdsEnemyPath>(prevEnemyPath));
            }
        }

        return enemyPaths;
    }

    public static TPoint FindNearest<TPoint>(this IEnumerable<TPoint> sourcePoints, Vector2d point)
        where TPoint : IMapDataEntry, IPoint => FindNearest(sourcePoints, new Vector3d(point.X, 0, point.Y), true);

    public static TPoint FindNearest<TPoint>(this IEnumerable<TPoint> sourcePoints, Vector3d point, bool is2d = false)
        where TPoint : IMapDataEntry, IPoint
    {
        TPoint nearestPoint = default;

        foreach (var sourcePoint in sourcePoints)
        {
            if (nearestPoint == null)
            {
                nearestPoint = sourcePoint;
                continue;
            }

            var sourcePos = sourcePoint.Position;
            var oldNearestPos = nearestPoint.Position;

            if (is2d)
                sourcePos.Y = 0;

            if (Vector3d.Distance(point, sourcePos) < Vector3d.Distance(point, oldNearestPos))
                nearestPoint = sourcePoint;
        }

        return nearestPoint;
    }
}
