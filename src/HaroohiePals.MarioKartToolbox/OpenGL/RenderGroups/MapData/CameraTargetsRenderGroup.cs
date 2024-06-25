using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData;

class CameraTargetsRenderGroup : RenderGroup, IColoredRenderGroup, IDisposable
{
    private readonly MapDataCollection<MkdsCamera> _cameras;
    private IEnumerable<MkdsCamera> _renderedCameras = new List<MkdsCamera>();
    public Color Color { get; set; }

    private const int TargetASubIndex = 1;
    private const int TargetBSubIndex = 2;

    //todo: I use 2 renderers because I plan using 2 different textures
    protected readonly InstancedPointRenderer _rendererA;
    protected readonly InstancedPointRenderer _rendererB;
    protected readonly LineRenderer _lineRenderer;

    public CameraTargetsRenderGroup(MapDataCollection<MkdsCamera> cameras, Color color, bool render2d, IRendererFactory rendererFactory)
    {
        _cameras = cameras;
        Color = color;

        _lineRenderer = new LineRenderer();
        _lineRenderer.Color = Color;
        _lineRenderer.Thickness = 2;
        _lineRenderer.Loop = true;
        _lineRenderer.Render2d = render2d;

        if (render2d)
        {
            _rendererA = new DotRenderer();
            _rendererB = new DotRenderer();
        }
        else
        {
            _rendererA = rendererFactory.CreateBoxRenderer();
            _rendererB = rendererFactory.CreateBoxRenderer();
        }
    }

    public void Dispose()
    {
        _rendererA.Dispose();
        _rendererB.Dispose();
        _lineRenderer.Dispose();
    }

    private InstancedPoint[] SetupPoints(ViewportContext context, IEnumerable<MkdsCamera> cameras, bool targetB)
    {
        var points = cameras.Select((x, i) =>
        {
            uint pickingId = MktbRendererUtil.GetPickingId(i, PickingGroupId, subIndex: targetB ? TargetBSubIndex : TargetASubIndex);
            bool isSelected = context.IsSelected(x);
            bool isHovered = context.IsHovered(x);

            return new InstancedPoint((Vector3)(targetB ? x.Target2 : x.Target1), new(), new(1), Color, false, x, pickingId, isHovered, isSelected);
        }).ToArray();

        return points;
    }
    public override void Render(ViewportContext context)
    {
        _renderedCameras = _cameras.Where(x =>
            (x.Type == MkdsCameraType.RouteLookAtTargets ||
            x.Type == MkdsCameraType.FixedLookAtTargets) &&
            context.SceneObjectHolder.IsSelected(x)).ToList();

        // Target1
        _rendererA.Points = SetupPoints(context, _renderedCameras, false);
        _rendererA.Render(context);

        // Target2
        _rendererB.Points = SetupPoints(context, _renderedCameras, true);
        _rendererB.Render(context);

        // Line
        foreach (var cam in _renderedCameras)
        {
            var origin = (Vector3)cam.Position;
            var pointA = (Vector3)cam.Target1;
            var pointB = (Vector3)cam.Target2;

            _lineRenderer.Points = new[] { pointA, pointB, origin };
            _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);
        }
    }


    public override object GetObject(int index) => _renderedCameras.ElementAt(index);

    public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
    {
        transform = new Transform(new(0), new(0), new(1));

        if (obj is not MkdsCamera camera)
        {
            return false;
        }

        switch (subIndex)
        {
            default:
            case TargetASubIndex:
                transform.Translation = camera.Target1;
                break;
            case TargetBSubIndex:
                transform.Translation = camera.Target2;
                break;
        }

        return true;
    }

    public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        if (obj is MkdsCamera camera)
        {
            switch (subIndex)
            {
                default:
                case TargetASubIndex:
                    camera.Target1 = transform.Translation;
                    break;
                case TargetBSubIndex:
                    camera.Target2 = transform.Translation;
                    break;
            }

            return true;
        }

        return false;
    }

    public override bool ContainsObject(object obj) => obj is MkdsCamera instance && _renderedCameras.Contains(instance);

}
