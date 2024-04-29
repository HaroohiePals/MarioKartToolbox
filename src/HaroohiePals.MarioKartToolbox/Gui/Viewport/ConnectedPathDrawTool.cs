using HaroohiePals.Actions;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Course;
using ImGuiNET;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport
{
    internal class ConnectedPathDrawTool<TPath, TPoint> : MapDataCollectionDrawTool<TPoint>
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, new()
    {
        protected MapDataCollection<TPath> _paths;

        public ConnectedPathDrawTool(IMkdsCourse course, MapDataCollection<TPoint> collection, MapDataCollection<TPath> paths) : base(course, collection)
        {
            _paths = paths;
        }

        private bool TrySplit(TPath path, int splitStartIndex, out List<IAction> actions, out TPath newPath)
        {
            actions = new();
            newPath = null;

            try
            {
                if (splitStartIndex >= path.Points.Count)
                    throw new System.IndexOutOfRangeException();

                // Construct new path
                newPath = new TPath();

                // Insert new path
                actions.Add(new InsertMkdsMapDataCollectionItemsAction<TPath>(_course.MapData, newPath, _paths, _paths.Count));

                // Save reference
                var nextPaths = path.Next.Select(x => x.Target).ToList();
                foreach (var reference in path.Next.ToList())
                    actions.Add(new SetReferenceCollectionItemAction<TPath>(path.Next, reference, null));

                // Replace all references to the current path with the new one
                foreach (var otherPath in _paths.Where(x => x != path))
                    foreach (var otherPathPrev in otherPath.Previous.Where(x => x.Target == path).ToList())
                        actions.Add(new SetReferenceCollectionItemAction<TPath>(otherPath.Previous, otherPathPrev, newPath));

                // Set next to new path
                actions.Add(new SetReferenceCollectionItemAction<TPath>(path.Next, null, newPath));

                // Set new path's previous
                actions.Add(new SetReferenceCollectionItemAction<TPath>(newPath.Previous, null, path));

                // Set new path's nexts
                foreach (var nextPath in nextPaths)
                    actions.Add(new SetReferenceCollectionItemAction<TPath>(newPath.Next, null, nextPath));

                // Move items to new path
                var itemsToMove = path.Points.Skip(splitStartIndex).Take(path.Points.Count - splitStartIndex).Cast<IMapDataEntry>().ToList();
                actions.Add(MoveMkdsMapDataCollectionItemsActionFactory.Create(itemsToMove, path.Points, newPath.Points));

                return true;
            }
            catch
            {
                actions = new();
                return false;
            }
        }

        private bool TryCreatePath(TPath path, TPoint splitPoint, out List<IAction> actions, out TPath newPath)
        {
            actions = new();
            newPath = null;

            try
            {
                // Split paths
                if (TrySplit(path, _collection.IndexOf(splitPoint) + 1, out var splitActions, out var splitPath))
                    actions.AddRange(splitActions);

                // Construct new path
                newPath = new TPath();

                // Insert new path in the collection
                actions.Add(new InsertMkdsMapDataCollectionItemsAction<TPath>(_course.MapData, newPath, _paths, _paths.Count));

                // Link previous with path
                actions.Add(new SetReferenceCollectionItemAction<TPath>(newPath.Previous, null, path));
                actions.Add(new SetReferenceCollectionItemAction<TPath>(path.Next, null, newPath));

                return true;
            }
            catch
            {
                actions = new();
                return false;
            }
        }

        private bool TryConnect(TPath path, TPoint pointFrom, TPoint pointTo, out List<IAction> actions)
        {
            actions = new();

            try
            {
                var connectPath = _paths.FirstOrDefault(x => x.Points.Contains(pointTo));
                var newPath = connectPath;

                int index = connectPath.Points.IndexOf(pointTo);

                //todo: Handle same path splits

                // Different path, split target path only if the target index > 0
                if (index > 0 && TrySplit(connectPath, index, out var splitActions, out newPath))
                    actions.AddRange(splitActions);

                // Link previous
                if (!newPath.Previous.Any(x => x.Target == path))
                    actions.Add(new SetReferenceCollectionItemAction<TPath>(newPath.Previous, null, path));

                // Link next
                if (!path.Next.Any(x => x.Target == newPath))
                    actions.Add(new SetReferenceCollectionItemAction<TPath>(path.Next, null, newPath));

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
            var path = _paths.FirstOrDefault(x => x.Points == _collection);
            var actions = new List<IAction>();

            var lastSelected = context.SceneObjectHolder.GetSelection().LastOrDefault();

            if (context.HoverObject != null)
            {
                if (keyShift && context.HoverObject.Object is TPoint targetPoint && lastSelected is TPoint lastSelectedPoint)
                {
                    if (!TryConnect(path, lastSelectedPoint, targetPoint, out var connectActions))
                        return false;

                    _entry = targetPoint;
                    actions.AddRange(connectActions);
                    _atomicActionBuilder.Do(new BatchAction(actions));

                    return true;
                }
                return false;
            }

            var targetCollection = _collection;

            if (keyShift && lastSelected is TPoint p)
            {
                if (!TryCreatePath(path, p, out var splitActions, out var newPath))
                    return false;

                actions.AddRange(splitActions);

                // The next point must be added in the newly created path
                targetCollection = newPath.Points;
            }

            ConstructEntry();
            UpdateEntry(rayStart, rayDir);
            return AddEntry(context, targetCollection, actions);
        }
    }
}
