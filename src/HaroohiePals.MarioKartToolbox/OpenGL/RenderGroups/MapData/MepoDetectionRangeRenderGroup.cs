using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

sealed class MepoDetectionRangeRenderGroup(
    MapDataCollection<MkdsMgEnemyPath> paths,
    Color color,
    IRendererFactory rendererFactory)
    : RenderGroup, IColoredRenderGroup, IDisposable
{
    private const float MEPO_DETECTION_RANGE = 200f;

    public Color Color { get; set; } = color;
    public bool ShowAll { get; set; }

    private readonly MeshRenderer _renderer = rendererFactory.CreateSphereRenderer(false);

    public override void Render(ViewportContext context)
    {
        if (!context.TranslucentPass)
            return;
        
        _renderer.Points = GetVisiblePoints(context)
            .Select(x => new InstancedPoint((Vector3)x.Position, new(), 
                new(MEPO_DETECTION_RANGE),
                Color, false, x, ViewportContext.InvalidPickingId, false, false))
            .ToArray();
        _renderer.Render(context);
    }

    private IEnumerable<MkdsMgEnemyPoint> GetVisiblePoints(ViewportContext context)
    {
        if (ShowAll)
            return paths.SelectMany(path => path.Points);

        var selection = context.SceneObjectHolder.GetSelection().ToList();
        var points = new HashSet<MkdsMgEnemyPoint>();

        points.UnionWith(selection.OfType<MkdsMgEnemyPath>().SelectMany(path => path.Points));
        points.UnionWith(selection.OfType<MkdsMgEnemyPoint>());

        return points;
    }

    public void Dispose()
    {
        _renderer.Dispose();
    }
}
