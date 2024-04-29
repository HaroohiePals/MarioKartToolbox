using HaroohiePals.Actions;
using HaroohiePals.MarioKart.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class MapDataGeneratorViewModel
{
    private readonly ICourseEditorContext _courseEditorContext;
    private MkdsMapData _mapData => _courseEditorContext.Course.MapData;

    public MapDataGeneratorSettings Settings;

    public readonly bool CanGenerate;
    public readonly bool ValidStartPoint;
    public bool ValidSourceType { get; private set; }

    private IEnumerable<MkdsRespawnPoint> _currentRespawnPoints;
    private IEnumerable<MkdsEnemyPoint> _currentEnemyPoints;
    private IEnumerable<MkdsItemPoint> _currentItemPoints;
    private IEnumerable<MkdsCheckPoint> _currentCheckPoints;

    public MapDataGeneratorViewModel(ICourseEditorContext courseEditorContext)
    {
        _courseEditorContext = courseEditorContext;

        CanGenerate = !_mapData.IsMgStage;
        ValidStartPoint = _mapData.StartPoints?.Count > 0;

        Settings.CheckPointWidth = 250;
        Settings.Flags = MapDataGeneratorFlags.UpdateCheckPointReferences | MapDataGeneratorFlags.UpdateRespawnReferences;

        Settings.RespawnPointSkip = 8;
        Settings.RespawnPointKeepPathBoundaries = false;

        Settings.CheckPointPointSkip = 2;
        Settings.CheckPointKeepPathBoundaries = true;
    }

    public bool PerformGenerate()
    {
        if (!CanGenerate || !ValidStartPoint || !ValidSourceType)
            return false;

        try
        {
            var actions = new List<IAction>();

            _currentItemPoints = _mapData.ItemPaths.SelectMany(x => x.Points).ToList();
            _currentEnemyPoints = _mapData.EnemyPaths.SelectMany(x => x.Points).ToList();
            _currentCheckPoints = _mapData.CheckPointPaths.SelectMany(x => x.Points).ToList();
            _currentRespawnPoints = _mapData.RespawnPoints.ToList();

            if (Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateItemPath))
                actions.AddRange(GenerateItemPath());

            if (Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateEnemyPath))
                actions.AddRange(GenerateEnemyPath());

            if (Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateRespawn))
            {
                if (Settings.SourceType == MapDataGeneratorSourceType.EnemyPath)
                    actions.AddRange(GenerateRespawn(_mapData.EnemyPaths));
                if (Settings.SourceType == MapDataGeneratorSourceType.ItemPath)
                    actions.AddRange(GenerateRespawn(_mapData.ItemPaths));
            }

            if (Settings.Flags.HasFlag(MapDataGeneratorFlags.GenerateCheckPoint))
            {
                if (Settings.SourceType == MapDataGeneratorSourceType.EnemyPath)
                    actions.AddRange(GenerateCheckPointPaths(_mapData.EnemyPaths));
                if (Settings.SourceType == MapDataGeneratorSourceType.ItemPath)
                    actions.AddRange(GenerateCheckPointPaths(_mapData.ItemPaths));
            }

            if (Settings.Flags.HasFlag(MapDataGeneratorFlags.UpdateRespawnReferences))
                actions.AddRange(UpdateRespawnReferences());

            if (Settings.Flags.HasFlag(MapDataGeneratorFlags.UpdateCheckPointReferences))
                actions.AddRange(UpdateCheckPointReferences());

            _courseEditorContext.ActionStack.Add(new BatchAction(actions));

            return true;
        }
        catch
        {
            return false;
        }
    }

    private IEnumerable<IAction> GenerateItemPath()
    {
        var actions = new List<IAction>();

        if (Settings.SourceType != MapDataGeneratorSourceType.EnemyPath)
            return actions;

        //Clear paths first
        if (_mapData.ItemPaths != null && _mapData.ItemPaths.Count > 0)
        {
            foreach (var path in _mapData.ItemPaths)
                actions.Add(new DeleteMkdsMapDataCollectionItemsAction<MkdsItemPath>(_mapData, path, _mapData.ItemPaths));
        }

        var newPaths = _mapData.EnemyPaths.ConvertToItemPaths();

        _currentItemPoints = newPaths.SelectMany(x => x.Points).ToList();

        actions.Add(InsertMkdsMapDataCollectionItemsActionFactory.Create(_mapData, newPaths, _mapData.ItemPaths));

        return actions;
    }

    private IEnumerable<IAction> GenerateEnemyPath()
    {
        var actions = new List<IAction>();

        if (Settings.SourceType != MapDataGeneratorSourceType.ItemPath)
            return actions;

        //Clear paths first
        if (_mapData.EnemyPaths != null && _mapData.EnemyPaths.Count > 0)
        {
            foreach (var path in _mapData.EnemyPaths)
                actions.Add(new DeleteMkdsMapDataCollectionItemsAction<MkdsEnemyPath>(_mapData, path, _mapData.EnemyPaths));
        }

        var newPaths = _mapData.ItemPaths.ConvertToEnemyPaths();
        _currentEnemyPoints = newPaths.SelectMany(x => x.Points).ToList();

        actions.Add(InsertMkdsMapDataCollectionItemsActionFactory.Create(_mapData, newPaths, _mapData.EnemyPaths));

        return actions;
    }

    private IReadOnlyList<TPoint> PreparePathPoints<TPath, TPoint>(ConnectedPath<TPath, TPoint> sourcePath, int skip, bool keepBoundaries)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, IRoutePoint
    {
        var result = new List<TPoint>();

        for (int i = 0; i < sourcePath.Points.Count; i++)
        {
            if (i % skip != 0)
                continue;
            result.Add(sourcePath.Points[i]);
        }

        if (keepBoundaries && !result.Contains(sourcePath.Points[^1]))
            result.Add(sourcePath.Points[^1]);

        return result;
    }

    private IEnumerable<IAction> GenerateRespawn<TPath, TPoint>(IEnumerable<ConnectedPath<TPath, TPoint>> sourcePaths)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, IRoutePoint
    {
        var actions = new List<IAction>();

        //Clear Respawn points first
        if (_mapData.RespawnPoints != null && _mapData.RespawnPoints.Count > 0)
        {
            foreach (var point in _mapData.RespawnPoints)
                actions.Add(new DeleteMkdsMapDataCollectionItemsAction<MkdsRespawnPoint>(_mapData, point, _mapData.RespawnPoints));
        }

        var newRespawnPoints = new List<MkdsRespawnPoint>();

        foreach (var sourcePath in sourcePaths)
        {
            var points = PreparePathPoints(sourcePath, Settings.RespawnPointSkip, Settings.RespawnPointKeepPathBoundaries);

            for (int i = 0; i < points.Count; i++)
            {
                Vector3d p1 = points[i].Position;
                Vector3d p2 = Vector3d.Zero;

                if (i + 1 >= points.Count && sourcePath.Next?.Count > 0)
                {
                    var nextPoints = sourcePath.Next.Where(x => x.Target?.Points.Count > 0)
                        .Select(x => x.Target?.Points[0].Position);

                    foreach (Vector3d nextPoint in nextPoints)
                        p2 += nextPoint;

                    p2 /= nextPoints.Count();
                }
                else
                {
                    // Instead of getting the next position from prepared points,
                    // I use the original path (for more accuracy)
                    var nextPoint = sourcePath.Points[sourcePath.Points.IndexOf(points[i]) + 1];
                    p2 = nextPoint.Position;
                }

                var direction = (p2 - p1).Normalized().Xz;
                double angle = -MathHelper.RadiansToDegrees(Math.Atan2(direction.Y, direction.X)) + 90;

                newRespawnPoints.Add(new MkdsRespawnPoint
                {
                    Position = p1,
                    Rotation = new Vector3d(0, angle, 0)
                });
            }
        }

        _currentRespawnPoints = newRespawnPoints;

        actions.Add(InsertMkdsMapDataCollectionItemsActionFactory.Create(_mapData, newRespawnPoints, _mapData.RespawnPoints));

        return actions;
    }

    private MkdsCheckPoint GenerateCheckPoint(Vector3d sourcePoint, Vector2d direction, double width)
    {
        var cpoi = new MkdsCheckPoint
        {
            Point1 = new Vector2d(sourcePoint.X + direction.X * width, sourcePoint.Z + direction.Y * width),
            Point2 = new Vector2d(sourcePoint.X - direction.X * width, sourcePoint.Z - direction.Y * width)
        };

        return cpoi;
    }

    private MkdsCheckPointPath GenerateCheckPointPath<TPath, TPoint>(ConnectedPath<TPath, TPoint> sourcePath)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, IRoutePoint
    {
        var path = new MkdsCheckPointPath();

        Vector2d? prevDir = null;

        var points = PreparePathPoints(sourcePath, Settings.CheckPointPointSkip, Settings.CheckPointKeepPathBoundaries);

        for (int i = 0; i < points.Count; i++)
        {
            Vector3d p1 = points[i].Position;
            Vector3d p2 = Vector3d.Zero;

            if (i + 1 >= points.Count && sourcePath.Next?.Count > 0)
            {
                var nextPoints = sourcePath.Next.Where(x => x.Target?.Points.Count > 0)
                    .Select(x => x.Target?.Points[0].Position);

                foreach (Vector3d nextPoint in nextPoints)
                    p2 += nextPoint;

                p2 /= nextPoints.Count();
            }
            else
            {
                // Instead of getting the next position from prepared points,
                // I use the original path (for more accuracy)
                var nextPoint = sourcePath.Points[sourcePath.Points.IndexOf(points[i]) + 1];
                p2 = nextPoint.Position;
            }

            var direction = (p2 - p1).Normalized().Xz;
            direction = direction.PerpendicularRight;

            if (prevDir is not null)
                direction = Vector2d.Lerp(prevDir.Value, direction, 0.5d);

            prevDir = direction;

            path.Points.Add(GenerateCheckPoint(p1, direction, Settings.CheckPointWidth));
        }

        //Fix sections
        if (path.Points.Count > 0)
        {
            path.Points[0].StartSection = 0;
            path.Points[^1].GotoSection = 0;
        }

        return path;
    }

    private IEnumerable<IAction> GenerateCheckPointPaths<TPath, TPoint>(IEnumerable<ConnectedPath<TPath, TPoint>> sourcePaths)
        where TPath : ConnectedPath<TPath, TPoint>, new()
        where TPoint : IMapDataEntry, IRoutePoint
    {
        var actions = new List<IAction>();

        //Clear paths first
        if (_mapData.CheckPointPaths != null && _mapData.CheckPointPaths.Count > 0)
        {
            foreach (var path in _mapData.CheckPointPaths)
                actions.Add(new DeleteMkdsMapDataCollectionItemsAction<MkdsCheckPointPath>(_mapData, path, _mapData.CheckPointPaths));
        }

        //Generate paths
        var pathsList = sourcePaths.ToList();
        var checkPointPaths = sourcePaths.Select(x => GenerateCheckPointPath(x)).ToList();

        if (checkPointPaths.Count == 0)
            return actions;

        for (int i = 0; i < pathsList.Count; i++)
        {
            var checkPointPath = checkPointPaths[i];
            var sourcePath = pathsList[i];

            //Next
            foreach (var next in sourcePath.Next)
            {
                var nextPath = checkPointPaths[pathsList.IndexOf(next?.Target)];
                checkPointPath.Next.Add(new WeakMapDataReference<MkdsCheckPointPath>(nextPath));
            }
            //Prev
            foreach (var prev in sourcePath.Previous)
            {
                var prevPath = checkPointPaths[pathsList.IndexOf(prev?.Target)];
                checkPointPath.Previous.Add(new WeakMapDataReference<MkdsCheckPointPath>(prevPath));
            }
        }

        int keyCheckpointIndex = 0;

        //Apply key checkpoints
        void applyKeyCheckpoints(MkdsCheckPointPath cpat)
        {
            //Don't count the first one
            int checkpointCounted = -1;
            int keyCheckpointModulo = 5;

            foreach (var point in cpat.Points)
            {
                if (checkpointCounted++ % keyCheckpointModulo == 0)
                    point.KeyPointId = (short)keyCheckpointIndex++;
            }
        }

        //Todo: handle split paths properly by assigning weights maybe
        applyKeyCheckpoints(checkPointPaths[0]);

        _currentCheckPoints = checkPointPaths.SelectMany(x => x.Points).ToList();

        actions.Add(InsertMkdsMapDataCollectionItemsActionFactory.Create(_mapData, checkPointPaths, _mapData.CheckPointPaths));

        return actions;
    }

    private IEnumerable<IAction> UpdateRespawnReferences()
    {
        var actions = new List<IAction>();

        if (_currentRespawnPoints?.Count() == 0)
            return actions;

        //Find nearest enemy and item points
        foreach (var ktpj in _currentRespawnPoints)
        {
            var nearestItem = _currentItemPoints.FindNearest(ktpj.Position);
            var nearestEnemy = _currentEnemyPoints.FindNearest(ktpj.Position);

            if (nearestItem is not null)
                actions.Add(new SetMapDataEntryReferenceAction<MkdsItemPoint>(ktpj, nameof(MkdsRespawnPoint.ItemPoint), null, nearestItem));
            if (nearestEnemy is not null)
                actions.Add(new SetMapDataEntryReferenceAction<MkdsEnemyPoint>(ktpj, nameof(MkdsRespawnPoint.EnemyPoint), null, nearestEnemy));
        }

        return actions;
    }

    private IEnumerable<IAction> UpdateCheckPointReferences()
    {
        var actions = new List<IAction>();

        if (_currentRespawnPoints?.Count() == 0 || _currentCheckPoints?.Count() == 0)
            return actions;

        //Find nearest respawn point
        foreach (var cpoi in _currentCheckPoints)
        {
            var entryAvgPos = (cpoi.Point1 + cpoi.Point2) / 2;
            var nearestRespawn = _currentRespawnPoints.FindNearest(entryAvgPos);

            if (nearestRespawn is not null)
                actions.Add(new SetMapDataEntryReferenceAction<MkdsRespawnPoint>(cpoi, nameof(MkdsCheckPoint.Respawn), null, nearestRespawn));
        }

        return actions;
    }

    public void Update()
    {
        if (Settings.SourceType == MapDataGeneratorSourceType.EnemyPath)
            ValidSourceType = _mapData.EnemyPaths?.Count > 0;

        if (Settings.SourceType == MapDataGeneratorSourceType.ItemPath)
            ValidSourceType = _mapData.ItemPaths?.Count > 0;
    }
}