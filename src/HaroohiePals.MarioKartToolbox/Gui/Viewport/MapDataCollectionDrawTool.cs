using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Extensions;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport
{
    public class MapDataCollectionDrawTool<T> : DrawTool where T : IMapDataEntry, new()
    {
        protected readonly MapDataCollection<T> _collection;
        protected readonly IMkdsCourse _course;

        protected T _entry;

        public MapDataCollectionDrawTool(IMkdsCourse course, MapDataCollection<T> collection)
        {
            _collection = collection;
            _course = course;
        }

        protected bool AddEntry(ViewportContext context, MapDataCollection<T> targetCollection, List<IAction> previousActions = null)
        {
            if (previousActions == null)
                previousActions = new();

            var lastSelected = context.SceneObjectHolder.GetSelection().LastOrDefault();

            previousActions.Add(new InsertMkdsMapDataCollectionItemsAction<T>(_course.MapData, _entry, targetCollection,
                lastSelected != null && lastSelected is T l ? targetCollection.IndexOf(l) + 1 : targetCollection.Count));

            _atomicActionBuilder.Do(new BatchAction(previousActions));

            return true;
        }

        protected virtual void ConstructEntry() => _entry = new T();

        protected virtual void UpdateEntry(Vector3d rayStart, Vector3d rayDir)
        {
            if (_entry is not IPoint point)
                return;

            if (_course.Collision != null && _course.Collision.FindIntersection(rayStart, rayDir, out var intersection))
                point.Position = intersection;
            else
                point.Position = rayStart + rayDir * 100f;
        }

        protected virtual void PerformFinalizationActions() { }

        protected override bool MouseDown(ViewportContext context, Vector3d rayStart, Vector3d rayDir)
        {
            if (context.HoverObject != null)
                return false;

            ConstructEntry();
            UpdateEntry(rayStart, rayDir);
            return AddEntry(context, _collection);
        }

        protected override bool MouseDrag(ViewportContext context, Vector3d rayStart, Vector3d rayDir)
        {
            if (_entry == null)
                return false;

            UpdateEntry(rayStart, rayDir);
            return true;
        }

        protected override bool MouseUp(ViewportContext context, Vector3d rayStart, Vector3d rayDir)
        {
            if (_entry == null)
                return false;

            PerformFinalizationActions();

            context.SceneObjectHolder.SetSelection(_entry);

            return true;
        }

        public override string ToString() => _collection.GetDisplayName();
    }

    public static class MapDataCollectionDrawTool
    {
        public static DrawTool CreateTool(IMkdsCourse course, IMapDataEntry entry)
        {
            try
            {
                switch (entry)
                {
                    default:
                        return null;

                    case MkdsMapObject:
                        return new MapDataCollectionDrawTool<MkdsMapObject>(course, course.MapData.MapObjects);
                    case MkdsStartPoint:
                        return new MapDataCollectionDrawTool<MkdsStartPoint>(course, course.MapData.StartPoints);
                    case MkdsRespawnPoint:
                        return new MapDataCollectionDrawTool<MkdsRespawnPoint>(course, course.MapData.RespawnPoints);
                    case MkdsKartPoint2d:
                        return new MapDataCollectionDrawTool<MkdsKartPoint2d>(course, course.MapData.KartPoint2D);
                    case MkdsCannonPoint:
                        return new MapDataCollectionDrawTool<MkdsCannonPoint>(course, course.MapData.CannonPoints);
                    case MkdsKartPointMission:
                        return new MapDataCollectionDrawTool<MkdsKartPointMission>(course, course.MapData.KartPointMission);
                    case MkdsCamera:
                        return new MapDataCollectionDrawTool<MkdsCamera>(course, course.MapData.Cameras);
                    case MkdsArea:
                        return new MapDataCollectionDrawTool<MkdsArea>(course, course.MapData.Areas);

                    case MkdsPath path:
                        return new MapDataCollectionDrawTool<MkdsPathPoint>(course, path.Points);
                    case MkdsPathPoint:
                        return new MapDataCollectionDrawTool<MkdsPathPoint>(course, course.MapData.Paths.FirstOrDefault(x => x.Points.Contains(entry))?.Points);

                    case MkdsEnemyPath path:
                        return new ConnectedPathDrawTool<MkdsEnemyPath, MkdsEnemyPoint>(course, path.Points, course.MapData.EnemyPaths);
                    case MkdsEnemyPoint:
                        return new ConnectedPathDrawTool<MkdsEnemyPath, MkdsEnemyPoint>(course, course.MapData.EnemyPaths.FirstOrDefault(x => x.Points.Contains(entry))?.Points, course.MapData.EnemyPaths);

                    case MkdsMgEnemyPath path:
                        return new MgEnemyPathDrawTool(course, path.Points, course.MapData.MgEnemyPaths);
                    case MkdsMgEnemyPoint:
                        return new MgEnemyPathDrawTool(course, course.MapData.MgEnemyPaths.FirstOrDefault(x => x.Points.Contains(entry))?.Points, course.MapData.MgEnemyPaths);

                    case MkdsItemPath path:
                        return new ConnectedPathDrawTool<MkdsItemPath, MkdsItemPoint>(course, path?.Points, course.MapData.ItemPaths);
                    case MkdsItemPoint:
                        return new ConnectedPathDrawTool<MkdsItemPath, MkdsItemPoint>(course, course.MapData.ItemPaths.FirstOrDefault(x => x.Points.Contains(entry))?.Points, course.MapData.ItemPaths);

                    case MkdsCheckPointPath path:
                        return new CheckPointPathDrawTool(course, path?.Points);
                    case MkdsCheckPoint:
                        return new CheckPointPathDrawTool(course, course.MapData.CheckPointPaths.FirstOrDefault(x => x.Points.Contains(entry))?.Points);
                }
            }
            catch
            {

            }

            return null;
        }

        public static DrawTool CreateTool(IMkdsCourse course, IMapDataCollection collection)
        {
            try
            {
                switch (collection)
                {
                    default:
                        return null;

                    case MapDataCollection<MkdsMapObject> mapObjects:
                        return new MapDataCollectionDrawTool<MkdsMapObject>(course, mapObjects);
                    case MapDataCollection<MkdsStartPoint> startPoints:
                        return new MapDataCollectionDrawTool<MkdsStartPoint>(course, startPoints);
                    case MapDataCollection<MkdsRespawnPoint> respawnPoints:
                        return new MapDataCollectionDrawTool<MkdsRespawnPoint>(course, respawnPoints);
                    case MapDataCollection<MkdsKartPoint2d> ktp2d:
                        return new MapDataCollectionDrawTool<MkdsKartPoint2d>(course, ktp2d);
                    case MapDataCollection<MkdsCannonPoint> cannonPoints:
                        return new MapDataCollectionDrawTool<MkdsCannonPoint>(course, cannonPoints);
                    case MapDataCollection<MkdsKartPointMission> ktpm:
                        return new MapDataCollectionDrawTool<MkdsKartPointMission>(course, ktpm);
                    case MapDataCollection<MkdsCamera> cameras:
                        return new MapDataCollectionDrawTool<MkdsCamera>(course, cameras);
                    case MapDataCollection<MkdsArea> areas:
                        return new MapDataCollectionDrawTool<MkdsArea>(course, areas);
                }
            }
            catch
            {

            }

            return null;
        }
    }
}
