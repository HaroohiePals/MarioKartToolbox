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

sealed class MepoRangeRenderGroup(
    MapDataCollection<MkdsMgEnemyPath> paths,
    Color color,
    IRendererFactory rendererFactory)
    : RenderGroup, IColoredRenderGroup, IDisposable
{
    public const float Range = 200f;

    public Color Color { get; set; } = color;

    private readonly MeshRenderer _renderer = rendererFactory.CreateSphereRenderer();

    public override void Render(ViewportContext context)
    {
        if (!context.TranslucentPass)
            return;

        _renderer.Points = context.SceneObjectHolder.GetSelection()
            .OfType<MkdsMgEnemyPoint>()
            .Where(x => paths.Any(path => path.Points.Contains(x)))
            .Select(x => new InstancedPoint((Vector3)x.Position, new(), new(Range / 10f),
                Color, false, x, ViewportContext.InvalidPickingId, false, false))
            .ToArray();
        _renderer.Render(context);
    }

    public void Dispose()
    {
        _renderer.Dispose();
    }
}
