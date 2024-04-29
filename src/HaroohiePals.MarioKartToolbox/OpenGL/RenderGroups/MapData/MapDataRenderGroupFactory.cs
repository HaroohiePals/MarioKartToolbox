using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Drawing;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

class MapDataRenderGroupFactory
{
    private readonly IApplicationSettingsService _applicationSettings;
    private readonly IRendererFactory _rendererFactory;

    public MapDataRenderGroupFactory(IApplicationSettingsService applicationSettings)
    {
        _applicationSettings = applicationSettings;
        //todo: should probably be injected later
        _rendererFactory = new RendererFactory(_applicationSettings);
    }

    public RenderGroup CreatePointRenderGroup<T>(MapDataCollection<T> collection, bool render2d)
        where T : IMapDataEntry
    {
        return collection switch
        {
            MapDataCollection<MkdsMapObject> or
                MapDataCollection<MkdsStartPoint> or
                MapDataCollection<MkdsRespawnPoint> or
                MapDataCollection<MkdsKartPoint2d> or
                MapDataCollection<MkdsCannonPoint> or
                MapDataCollection<MkdsKartPointMission> or
                MapDataCollection<MkdsArea> or
                MapDataCollection<MkdsCamera> => CreateMapDataCollectionRenderGroup(collection, render2d),
            MapDataCollection<MkdsEnemyPath> enemyPathCollection => new IpoiEpoiPointRenderGroup<MkdsEnemyPath, MkdsEnemyPoint>(
                enemyPathCollection, GetMapDataColor<MkdsEnemyPoint>(), render2d, _rendererFactory),
            MapDataCollection<MkdsMgEnemyPath> mgEnemyPathCollection => new MepoPointRenderGroup(
                mgEnemyPathCollection, GetMapDataColor<MkdsMgEnemyPoint>(), render2d, _rendererFactory),
            MapDataCollection<MkdsItemPath> itemPathCollection => new IpoiEpoiPointRenderGroup<MkdsItemPath, MkdsItemPoint>(
                itemPathCollection, GetMapDataColor<MkdsItemPoint>(), render2d, _rendererFactory),
            MapDataCollection<MkdsPath> pathCollection => new PathPointRenderGroup(
                pathCollection, GetMapDataColor<MkdsPathPoint>(), render2d, _rendererFactory),
            MapDataCollection<MkdsCheckPointPath> checkPointPathCollection => new CheckPointRenderGroup(
                checkPointPathCollection, _applicationSettings.Settings.Viewport.Colors.CheckPoint,
                _applicationSettings.Settings.Viewport.Colors.KeyCheckPoint),
            _ => throw new ArgumentException(nameof(collection))
        };
    }

    public RenderGroup CreateLineRenderGroup<T>(MapDataCollection<T> collection, bool render2d)
        where T : IMapDataEntry
    {
        return collection switch
        {
            MapDataCollection<MkdsEnemyPath> enemyPathCollection => new IpoiEpoiLineRenderGroup<MkdsEnemyPath, MkdsEnemyPoint>(
                enemyPathCollection, GetMapDataColor<MkdsEnemyPath>(), render2d),
            MapDataCollection<MkdsMgEnemyPath> mgEnemyPathCollection => new MepoLineRenderGroup(
                mgEnemyPathCollection, GetMapDataColor<MkdsMgEnemyPath>(), render2d),
            MapDataCollection<MkdsItemPath> itemPathCollection => new IpoiEpoiLineRenderGroup<MkdsItemPath, MkdsItemPoint>(
                itemPathCollection, GetMapDataColor<MkdsItemPath>(), render2d),
            MapDataCollection<MkdsPath> pathCollection => new PathRenderGroup(
                pathCollection, GetMapDataColor<MkdsPath>(), render2d),
            MapDataCollection<MkdsCheckPointPath> checkPointPathCollection => new CheckPointPathRenderGroup(
                checkPointPathCollection, GetMapDataColor<MkdsCheckPointPath>()),
            _ => throw new ArgumentException(nameof(collection))
        };
    }

    public RenderGroup CreateRadiusRenderGroup<T>(MapDataCollection<T> collection, bool render2d = false)
        where T : IMapDataEntry
    {
        return collection switch
        {
            MapDataCollection<MkdsEnemyPath> enemyPathCollection => new IpoiEpoiRadiusRenderGroup<MkdsEnemyPath, MkdsEnemyPoint>(
                enemyPathCollection, GetMapDataColor<MkdsEnemyPoint>(), _rendererFactory),
            MapDataCollection<MkdsMgEnemyPath> mgEnemyPathCollection => new MepoRadiusRenderGroup(
                mgEnemyPathCollection, GetMapDataColor<MkdsMgEnemyPoint>(), _rendererFactory),
            MapDataCollection<MkdsItemPath> itemPathCollection => new IpoiEpoiRadiusRenderGroup<MkdsItemPath, MkdsItemPoint>(
                itemPathCollection, GetMapDataColor<MkdsItemPoint>(), _rendererFactory),
            MapDataCollection<MkdsArea> areaCollection => new AreaShapeRenderGroup(
                areaCollection, GetMapDataColor<MkdsArea>(), render2d, _rendererFactory),
            _ => throw new ArgumentException(nameof(collection))
        };
    }

    public RenderGroup CreateCameraTargetsRenderGroup(MapDataCollection<MkdsCamera> collection, bool render2d = false)
        => new CameraTargetsRenderGroup(collection, GetMapDataColor<MkdsCamera>(), render2d, _rendererFactory);

    public RenderGroup CreateKartPointRenderGroup<T>(MkdsMapData mapData,
        MapDataCollection<T> collection, bool render2d = false)
        where T : IMapDataEntry, IRotatedPoint
    {
        return collection switch
        {
            MapDataCollection<MkdsStartPoint> => new StartPointRenderGroup(
                mapData, GetMapDataColor<T>(), render2d, _rendererFactory),
            _ => new KartPointRenderGroup<T>(collection, GetMapDataColor<T>(), render2d, _rendererFactory)
        };
    }

    public void UpdateRenderGroupColor(RenderGroup renderGroup)
    {
        if (renderGroup is IColoredRenderGroup coloredRenderGroup)
        {
            coloredRenderGroup.Color = renderGroup switch
            {
                MapDataCollectionRenderGroup<MkdsMapObject> => GetMapDataColor<MkdsMapObject>(),
                MapDataCollectionRenderGroup<MkdsStartPoint> => GetMapDataColor<MkdsStartPoint>(),
                KartPointRenderGroup<MkdsStartPoint> => GetMapDataColor<MkdsStartPoint>(),
                MapDataCollectionRenderGroup<MkdsRespawnPoint> => GetMapDataColor<MkdsRespawnPoint>(),
                MapDataCollectionRenderGroup<MkdsKartPoint2d> => GetMapDataColor<MkdsKartPoint2d>(),
                MapDataCollectionRenderGroup<MkdsCannonPoint> => GetMapDataColor<MkdsCannonPoint>(),
                MapDataCollectionRenderGroup<MkdsKartPointMission> => GetMapDataColor<MkdsKartPointMission>(),
                MapDataCollectionRenderGroup<MkdsArea> => GetMapDataColor<MkdsArea>(),
                AreaShapeRenderGroup => GetMapDataColor<MkdsArea>(),
                MapDataCollectionRenderGroup<MkdsCamera> => GetMapDataColor<MkdsCamera>(),
                CameraTargetsRenderGroup => GetMapDataColor<MkdsCamera>(),
                IpoiEpoiPointRenderGroup<MkdsEnemyPath, MkdsEnemyPoint> => GetMapDataColor<MkdsEnemyPoint>(),
                IpoiEpoiLineRenderGroup<MkdsEnemyPath, MkdsEnemyPoint> => GetMapDataColor<MkdsEnemyPath>(),
                MepoPointRenderGroup => GetMapDataColor<MkdsMgEnemyPoint>(),
                MepoLineRenderGroup => GetMapDataColor<MkdsMgEnemyPath>(),
                IpoiEpoiPointRenderGroup<MkdsItemPath, MkdsItemPoint> => GetMapDataColor<MkdsItemPoint>(),
                IpoiEpoiLineRenderGroup<MkdsItemPath, MkdsItemPoint> => GetMapDataColor<MkdsItemPath>(),
                PathPointRenderGroup => GetMapDataColor<MkdsPathPoint>(),
                PathRenderGroup => GetMapDataColor<MkdsPath>(),
                CheckPointPathRenderGroup => GetMapDataColor<MkdsCheckPointPath>(),
                _ => coloredRenderGroup.Color
            };
        }
        else if (renderGroup is CheckPointRenderGroup checkPointRenderGroup)
        {
            checkPointRenderGroup.RegularColor = _applicationSettings.Settings.Viewport.Colors.CheckPoint;
            checkPointRenderGroup.KeyColor = _applicationSettings.Settings.Viewport.Colors.KeyCheckPoint;
        }
    }

    private Color GetMapDataColor<T>() where T : IMapDataEntry
        => GetMapDataColor(typeof(T));

    private Color GetMapDataColor(Type type)
    {
        ref readonly var colorSettings = ref _applicationSettings.Settings.Viewport.Colors;
        if (type == typeof(MkdsMapObject))
            return colorSettings.MapObjects;
        else if (type == typeof(MkdsPath))
            return colorSettings.Paths;
        else if (type == typeof(MkdsPathPoint))
            return colorSettings.Paths;
        else if (type == typeof(MkdsStartPoint))
            return colorSettings.StartPoints;
        else if (type == typeof(MkdsRespawnPoint))
            return colorSettings.RespawnPoints;
        else if (type == typeof(MkdsKartPoint2d))
            return colorSettings.KartPoint2D;
        else if (type == typeof(MkdsCannonPoint))
            return colorSettings.CannonPoints;
        else if (type == typeof(MkdsKartPointMission))
            return colorSettings.KartPointMission;
        else if (type == typeof(MkdsEnemyPath))
            return colorSettings.EnemyPaths;
        else if (type == typeof(MkdsEnemyPoint))
            return colorSettings.EnemyPaths;
        else if (type == typeof(MkdsMgEnemyPath))
            return colorSettings.MgEnemyPaths;
        else if (type == typeof(MkdsMgEnemyPoint))
            return colorSettings.MgEnemyPaths;
        else if (type == typeof(MkdsItemPath))
            return colorSettings.ItemPaths;
        else if (type == typeof(MkdsItemPoint))
            return colorSettings.ItemPaths;
        else if (type == typeof(MkdsArea))
            return colorSettings.Areas;
        else if (type == typeof(MkdsCamera))
            return colorSettings.Cameras;
        else if (type == typeof(MkdsCheckPointPath))
            return colorSettings.CheckPoint;

        throw new ArgumentException(nameof(type));
    }

    private MapDataCollectionRenderGroup<T> CreateMapDataCollectionRenderGroup<T>(
        MapDataCollection<T> collection, bool render2d) where T : IMapDataEntry
    {
        var color = GetMapDataColor<T>();
        return new MapDataCollectionRenderGroup<T>(collection, color, render2d, _rendererFactory);
    }
}