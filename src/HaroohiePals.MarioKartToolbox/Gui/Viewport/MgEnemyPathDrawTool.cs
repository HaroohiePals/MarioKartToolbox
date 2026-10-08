using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using ImGuiNET;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport
{
    internal class MgEnemyPathDrawTool : MapDataCollectionDrawTool<MkdsMgEnemyPoint>
    {
        private MapDataCollection<MkdsMgEnemyPath> _paths;

        public MgEnemyPathDrawTool(IMkdsCourse course, MapDataCollection<MkdsMgEnemyPoint> collection, MapDataCollection<MkdsMgEnemyPath> paths)
            : base(course, collection)
        {
            _paths = paths;
        }

        private static MapDataReferenceCollection<MkdsMgEnemyPoint> GetEndpointLinks(MkdsMgEnemyPath path, int index)
            => index == 0 && path.Points.Count > 1 ? path.Previous : path.Next;

        private static void AddLink(List<IAction> actions, MapDataReferenceCollection<MkdsMgEnemyPoint> links, MkdsMgEnemyPoint target)
        {
            if (!links.Any(x => x.Target == target))
                actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(links, null, target));
        }

        private bool TrySplit(MkdsMgEnemyPath path, int splitStartIndex, out List<IAction> actions, out MkdsMgEnemyPath newPath)
        {
            actions = new();
            newPath = null;

            try
            {
                if (splitStartIndex <= 0 || splitStartIndex >= path.Points.Count)
                    throw new System.IndexOutOfRangeException();

                var lastKeptPoint = path.Points[splitStartIndex - 1];
                var firstMovedPoint = path.Points[splitStartIndex];

                // Construct new path
                newPath = new MkdsMgEnemyPath();

                // Move items to new path
                var itemsToMove = path.Points.Skip(splitStartIndex).Take(path.Points.Count - splitStartIndex).ToList();
                actions.Add(MoveMkdsMapDataCollectionItemsActionFactory.Create(itemsToMove, path.Points, newPath.Points));

                // Insert new path
                actions.Add(new InsertMkdsMapDataCollectionItemsAction<MkdsMgEnemyPath>(_course.MapData, newPath, _paths, _paths.Count));

                // Save reference
                var nextPoints = path.Next.Select(x => x.Target).ToList();
                foreach (var reference in path.Next.ToList())
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(path.Next, reference, null));

                // Set next to new path
                actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(path.Next, null, firstMovedPoint));

                // Set new path's previous
                actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(newPath.Previous, null, lastKeptPoint));

                // Set new path's nexts
                foreach (var nextPoint in nextPoints)
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(newPath.Next, null, nextPoint));

                return true;
            }
            catch
            {
                actions = new();
                return false;
            }
        }

        private bool TryCreatePath(MkdsMgEnemyPoint splitPoint, out List<IAction> actions, out MkdsMgEnemyPath newPath)
        {
            actions = new();
            newPath = null;

            try
            {
                var path = _paths.First(x => x.Points.Contains(splitPoint));
                int index = path.Points.IndexOf(splitPoint);

                // Split paths if branching from a middle point, so that it becomes the last point
                if (index > 0 && index < path.Points.Count - 1)
                {
                    if (!TrySplit(path, index + 1, out var splitActions, out _))
                        return false;
                    actions.AddRange(splitActions);
                }

                // Construct new path
                newPath = new MkdsMgEnemyPath();

                // Insert new path in the collection
                actions.Add(new InsertMkdsMapDataCollectionItemsAction<MkdsMgEnemyPath>(_course.MapData, newPath, _paths, _paths.Count));

                // Link the split point with the new path's first point (the entry about to be added)
                AddLink(actions, newPath.Previous, splitPoint);
                AddLink(actions, GetEndpointLinks(path, index), _entry);

                return true;
            }
            catch
            {
                actions = new();
                return false;
            }
        }

        private bool TryConnect(MkdsMgEnemyPoint pointFrom, MkdsMgEnemyPoint pointTo, out List<IAction> actions)
        {
            actions = new();

            try
            {
                var fromPath = _paths.First(x => x.Points.Contains(pointFrom));
                var toPath = _paths.First(x => x.Points.Contains(pointTo));

                int fromIndex = fromPath.Points.IndexOf(pointFrom);
                int toIndex = toPath.Points.IndexOf(pointTo);

                bool fromIsEndpoint = fromIndex == 0 || fromIndex == fromPath.Points.Count - 1;
                bool toIsEndpoint = toIndex == 0 || toIndex == toPath.Points.Count - 1;

                MapDataReferenceCollection<MkdsMgEnemyPoint> fromLinks;
                MapDataReferenceCollection<MkdsMgEnemyPoint> toLinks;

                if (fromPath == toPath)
                {
                    // Same path, only allow linking its endpoints together (loop)
                    if (pointFrom == pointTo || !fromIsEndpoint || !toIsEndpoint)
                        return false;

                    fromLinks = GetEndpointLinks(fromPath, fromIndex);
                    toLinks = GetEndpointLinks(toPath, toIndex);
                }
                else
                {
                    // Links can only start from endpoints, split middle points so that
                    // the source becomes a last point and the target becomes a start point
                    if (fromIsEndpoint)
                        fromLinks = GetEndpointLinks(fromPath, fromIndex);
                    else
                    {
                        if (!TrySplit(fromPath, fromIndex + 1, out var splitActions, out _))
                            return false;
                        actions.AddRange(splitActions);
                        fromLinks = fromPath.Next;
                    }

                    if (toIsEndpoint)
                        toLinks = GetEndpointLinks(toPath, toIndex);
                    else
                    {
                        if (!TrySplit(toPath, toIndex, out var splitActions, out var newPath))
                            return false;
                        actions.AddRange(splitActions);
                        toLinks = newPath.Previous;
                    }
                }

                AddLink(actions, fromLinks, pointTo);
                AddLink(actions, toLinks, pointFrom);

                return true;
            }
            catch
            {
                actions = new();
                return false;
            }
        }

        protected override bool MouseDown(ViewportContext context, Vector3d rayStart, Vector3d rayDir)
        {
            bool keyShift = ImGui.GetIO().KeyShift;
            var actions = new List<IAction>();

            var lastSelected = context.SceneObjectHolder.GetSelection().LastOrDefault();

            if (context.HoverObject != null)
            {
                if (keyShift && context.HoverObject.Object is MkdsMgEnemyPoint targetPoint && lastSelected is MkdsMgEnemyPoint lastSelectedPoint)
                {
                    if (!TryConnect(lastSelectedPoint, targetPoint, out var connectActions))
                        return false;

                    _entry = targetPoint;
                    actions.AddRange(connectActions);
                    _atomicActionBuilder.Do(new BatchAction(actions));

                    return true;
                }
                return false;
            }

            ConstructEntry();

            var targetCollection = _collection;

            if (keyShift && lastSelected is MkdsMgEnemyPoint p)
            {
                if (!TryCreatePath(p, out var splitActions, out var newPath))
                    return false;

                actions.AddRange(splitActions);

                // The next point must be added in the newly created path
                targetCollection = newPath.Points;
            }

            UpdateEntry(rayStart, rayDir);
            return AddEntry(context, targetCollection, actions);
        }
    }
}
