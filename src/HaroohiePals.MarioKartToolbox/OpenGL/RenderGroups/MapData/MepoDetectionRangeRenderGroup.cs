using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
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
        
        var visiblePoints = ShowAll
            ? paths.SelectMany(path => path.Points)
            : context.SceneObjectHolder.GetSelection()
                .OfType<MkdsMgEnemyPoint>()
                .Where(x => paths.Any(path => path.Points.Contains(x)));

        _renderer.Points = visiblePoints
            .Select(x => new InstancedPoint((Vector3)x.Position, new(), 
                new(MEPO_DETECTION_RANGE),
                Color, false, x, ViewportContext.InvalidPickingId, false, false))
            .ToArray();
        _renderer.Render(context);
    }

    public void Dispose()
    {
        _renderer.Dispose();
    }
}
