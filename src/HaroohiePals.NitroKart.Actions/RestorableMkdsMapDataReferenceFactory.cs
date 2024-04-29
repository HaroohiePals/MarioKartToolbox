using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.Actions;

public static class RestorableMkdsMapDataReferenceFactory
{
    private static IRestorableReference[] CollectFromConnectedPath<TPath, TPoint>(MkdsMapData mapData,
        MapDataCollection<TPath> pathCollection, ConnectedPath<TPath, TPoint> connectedPath)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry
    {
        var refs = new List<IRestorableReference>();

        refs.Add(new RestorableMapDataReferenceCollection<TPath>(connectedPath.Next));
        refs.Add(new RestorableMapDataReferenceCollection<TPath>(connectedPath.Previous));

        refs.AddRange(connectedPath.Points.SelectMany(x => CollectAll(mapData, x)));

        //Referrers
        foreach (var path in pathCollection.Where(x => x != connectedPath))
        {
            foreach (var next in path.Next)
            {
                if (next.Target == connectedPath)
                {
                    refs.Add(new RestorableMapDataReferenceCollection<TPath>(path.Next, connectedPath as TPath));
                }
            }

            foreach (var prev in path.Previous)
            {
                if (prev.Target == connectedPath)
                {
                    refs.Add(new RestorableMapDataReferenceCollection<TPath>(path.Previous, connectedPath as TPath));
                }
            }
        }

        return refs.ToArray();
    }

    private static IRestorableReference[] CollectFromMgEnemyPath(MkdsMapData mapData,
        MapDataCollection<MkdsMgEnemyPath> pathCollection, MkdsMgEnemyPath mgEnemyPath)
    {
        var refs = new List<IRestorableReference>();

        refs.Add(new RestorableMapDataReferenceCollection<MkdsMgEnemyPoint>(mgEnemyPath.Next));
        refs.Add(new RestorableMapDataReferenceCollection<MkdsMgEnemyPoint>(mgEnemyPath.Previous));

        refs.AddRange(mgEnemyPath.Points.SelectMany(x => CollectAll(mapData, x)));

        return refs.ToArray();
    }

    public static IEnumerable<IRestorableReference> CollectAll(MkdsMapData mapData, object item)
    {
        var refs = new List<IRestorableReference>();

        switch (item)
        {
            case MkdsMapObject mobj:
                // Handle held references
                if (mobj.Path != null)
                    refs.Add(new RestorableReference<MkdsPath>(mobj.Path.Target, mobj, nameof(mobj.Path)));
                break;
            case MkdsArea area:
                // Handle held references
                if (area.EnemyPoint != null)
                    refs.Add(new RestorableReference<MkdsEnemyPoint>(area.EnemyPoint.Target, area, nameof(MkdsEnemyPoint)));
                if (area.MgEnemyPoint != null)
                    refs.Add(new RestorableReference<MkdsMgEnemyPoint>(area.MgEnemyPoint.Target, area, nameof(MkdsMgEnemyPoint)));
                if (area.Camera != null)
                    refs.Add(new RestorableReference<MkdsCamera>(area.Camera.Target, area, nameof(MkdsCamera)));
                break;
            case MkdsCamera camera:
                // Handle held references
                if (camera.NextCamera != null)
                    refs.Add(new RestorableReference<MkdsCamera>(camera.NextCamera.Target, camera, nameof(camera.NextCamera)));
                if (camera.Path != null)
                    refs.Add(new RestorableReference<MkdsPath>(camera.Path.Target, camera, nameof(MkdsPath)));
                // Handle referrers' references
                foreach (var otherCamera in mapData.Cameras)
                {
                    if (otherCamera.NextCamera?.Target == camera)
                        refs.Add(new RestorableReference<MkdsCamera>(camera, otherCamera, nameof(otherCamera.NextCamera)));
                }
                foreach (var area in mapData.Areas)
                {
                    if (area.Camera?.Target == camera)
                        refs.Add(new RestorableReference<MkdsCamera>(camera, area, nameof(area.Camera)));
                }
                break;
            case MkdsCannonPoint cannon:
                // Handle held references
                if (cannon.MgEnemyPoint != null)
                    refs.Add(new RestorableReference<MkdsMgEnemyPoint>(cannon.MgEnemyPoint.Target, cannon, nameof(cannon.MgEnemyPoint)));
                break;
            case MkdsCheckPoint checkpoint:
                // Handle held references
                if (checkpoint.Respawn != null)
                    refs.Add(new RestorableReference<MkdsRespawnPoint>(checkpoint.Respawn.Target, checkpoint, nameof(checkpoint.Respawn)));
                break;
            case MkdsEnemyPoint enemyPoint:
                // Handle referrers' references
                foreach (var area in mapData.Areas)
                {
                    if (area.EnemyPoint?.Target == enemyPoint)
                        refs.Add(new RestorableReference<MkdsEnemyPoint>(enemyPoint, area, nameof(area.EnemyPoint)));
                }
                foreach (var respawnPoint in mapData.RespawnPoints)
                {
                    if (respawnPoint.EnemyPoint?.Target == enemyPoint)
                        refs.Add(new RestorableReference<MkdsEnemyPoint>(enemyPoint, respawnPoint, nameof(respawnPoint.EnemyPoint)));
                }
                break;
            case MkdsItemPoint itemPoint:
                // Handle referrers' references
                foreach (var respawnPoint in mapData.RespawnPoints)
                {
                    if (respawnPoint.ItemPoint?.Target == itemPoint)
                        refs.Add(new RestorableReference<MkdsItemPoint>(itemPoint, respawnPoint, nameof(respawnPoint.ItemPoint)));
                }
                break;
            case MkdsMgEnemyPoint mgEnemyPoint:
                // Handle referrers' references
                foreach (var area in mapData.Areas)
                {
                    if (area.MgEnemyPoint?.Target == mgEnemyPoint)
                        refs.Add(new RestorableReference<MkdsMgEnemyPoint>(mgEnemyPoint, area, nameof(area.MgEnemyPoint)));
                }
                foreach (var cannonPoint in mapData.CannonPoints)
                {
                    if (cannonPoint.MgEnemyPoint?.Target == mgEnemyPoint)
                        refs.Add(new RestorableReference<MkdsMgEnemyPoint>(mgEnemyPoint, cannonPoint, nameof(cannonPoint.MgEnemyPoint)));
                }
                foreach (var respawnPoint in mapData.RespawnPoints)
                {
                    if (respawnPoint.MgEnemyPoint?.Target == mgEnemyPoint)
                        refs.Add(new RestorableReference<MkdsMgEnemyPoint>(mgEnemyPoint, respawnPoint, nameof(respawnPoint.MgEnemyPoint)));
                }
                foreach (var path in mapData.MgEnemyPaths)
                {
                    for (int i = 0; i < path.Next.Count; i++)
                    {
                        if (path.Next[i].Target == mgEnemyPoint)
                            refs.Add(new RestorableMapDataReferenceCollection<MkdsMgEnemyPoint>(path.Next, mgEnemyPoint));
                    }
                    for (int i = 0; i < path.Previous.Count; i++)
                    {
                        if (path.Previous[i].Target == mgEnemyPoint)
                            refs.Add(new RestorableMapDataReferenceCollection<MkdsMgEnemyPoint>(path.Previous, mgEnemyPoint));
                    }
                }
                break;
            case MkdsPath path:
                // Handle referrers' references
                foreach (var camera in mapData.Cameras)
                {
                    if (camera.Path?.Target == path)
                        refs.Add(new RestorableReference<MkdsPath>(path, camera, nameof(camera.Path)));
                }
                foreach (var mapObject in mapData.MapObjects)
                {
                    if (mapObject.Path?.Target == path)
                        refs.Add(new RestorableReference<MkdsPath>(path, mapObject, nameof(mapObject.Path)));
                }
                break;
            case MkdsRespawnPoint respawn:
                // Handle held references
                if (respawn.EnemyPoint != null)
                    refs.Add(new RestorableReference<MkdsEnemyPoint>(respawn.EnemyPoint.Target, respawn, nameof(respawn.EnemyPoint)));
                if (respawn.MgEnemyPoint != null)
                    refs.Add(new RestorableReference<MkdsMgEnemyPoint>(respawn.MgEnemyPoint.Target, respawn, nameof(respawn.MgEnemyPoint)));
                if (respawn.ItemPoint != null)
                    refs.Add(new RestorableReference<MkdsItemPoint>(respawn.ItemPoint.Target, respawn, nameof(respawn.ItemPoint)));

                // Handle referrers' references
                foreach (var checkPoint in mapData.CheckPointPaths.SelectMany(x => x.Points).Where(x => x.Respawn?.Target == respawn))
                    refs.Add(new RestorableReference<MkdsRespawnPoint>(respawn, checkPoint, nameof(checkPoint.Respawn)));
                break;
            case MkdsCheckPointPath checkPointPath:
                refs.AddRange(CollectFromConnectedPath(mapData, mapData.CheckPointPaths, checkPointPath));
                break;
            case MkdsEnemyPath enemyPath:
                refs.AddRange(CollectFromConnectedPath(mapData, mapData.EnemyPaths, enemyPath));
                break;
            case MkdsItemPath itemPath:
                refs.AddRange(CollectFromConnectedPath(mapData, mapData.ItemPaths, itemPath));
                break;
            case MkdsMgEnemyPath mgEnemyPath:
                refs.AddRange(CollectFromMgEnemyPath(mapData, mapData.MgEnemyPaths, mgEnemyPath));
                break;
        }

        return refs;
    }
}
