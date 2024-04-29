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

        private bool TrySplit(MkdsMgEnemyPath path, int splitStartIndex, out List<IAction> actions, out MkdsMgEnemyPath newPath)
        {
            actions = new();
            newPath = null;

            try
            {
                if (splitStartIndex >= path.Points.Count)
                    throw new System.IndexOutOfRangeException();

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
                if (newPath.Points.Count > 0)
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(path.Next, null, newPath.Points[0]));

                // Set new path's previous
                if (path.Points.Count > 0)
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(newPath.Previous, null, path.Points[^1]));

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

        private bool TryCreatePath(MkdsMgEnemyPath path, MkdsMgEnemyPoint splitPoint, out List<IAction> actions, out MkdsMgEnemyPath newPath)
        {
            actions = new();
            newPath = null;

            try
            {
                // Split paths
                if (TrySplit(path, _collection.IndexOf(splitPoint) + 1, out var splitActions, out var splitPath))
                    actions.AddRange(splitActions);

                // Construct new path
                newPath = new MkdsMgEnemyPath();

                // Insert new path in the collection
                actions.Add(new InsertMkdsMapDataCollectionItemsAction<MkdsMgEnemyPath>(_course.MapData, newPath, _paths, _paths.Count));

                // Link previous with path
                if (path.Points.Count > 0)
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(newPath.Previous, null, path.Points[^1]));
                actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(path.Next, null, _entry));
                if (newPath.Points.Count > 0)
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(path.Next, null, newPath.Points[0]));

                return true;
            }
            catch
            {
                actions = new();
                return false;
            }
        }

        private bool TryConnect(MkdsMgEnemyPath path, MkdsMgEnemyPoint pointFrom, MkdsMgEnemyPoint pointTo, out List<IAction> actions)
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
                if (!newPath.Previous.Any(x => x.Target == pointFrom))
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(newPath.Previous, null, pointFrom));
                if (path.Points.Count > 0 && !newPath.Previous.Any(x => x.Target == path.Points[^1]))
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(newPath.Previous, null, path.Points[^1]));

                // Link next
                if (newPath.Points.Count > 0 && !path.Next.Any(x => x.Target == newPath.Points[0]))
                    actions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(path.Next, null, newPath.Points[0]));

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
                if (keyShift && context.HoverObject.Object is MkdsMgEnemyPoint targetPoint && lastSelected is MkdsMgEnemyPoint lastSelectedPoint)
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

            ConstructEntry();

            var targetCollection = _collection;

            if (keyShift && lastSelected is MkdsMgEnemyPoint p)
            {
                if (!TryCreatePath(path, p, out var splitActions, out var newPath))
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
