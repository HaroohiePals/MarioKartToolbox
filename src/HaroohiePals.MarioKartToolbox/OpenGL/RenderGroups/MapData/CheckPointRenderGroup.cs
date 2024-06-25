using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    public class CheckPointRenderGroup : RenderGroup, IDisposable
    {
        private readonly MapDataCollection<MkdsCheckPointPath> _paths;
        private readonly LineRenderer _lineRenderer;
        private readonly DotRenderer _dotRenderer;
        private readonly ArrowRenderer _arrowRenderer;

        public enum CheckPointPickingSubIndex : int
        {
            Point1 = -1,
            Point2,
        }

        private const int PathIdShift = 13;
        private const int PointIdMask = 0x1FFF;

        public Color RegularColor { get; set; }
        public Color KeyColor { get; set; }

        public CheckPointRenderGroup(MapDataCollection<MkdsCheckPointPath> paths, Color regularColor, Color keyColor)
        {
            RegularColor = regularColor;
            KeyColor = keyColor;
            _paths = paths;
            _lineRenderer = new();
            _lineRenderer.Offset2dY = -0.25f;
            _dotRenderer = new();
            _arrowRenderer = new();
        }

        public override void Render(ViewportContext context)
        {
            if (context.TranslucentPass)
            {
                for (int i = 0; i < _paths.Count; i++)
                {
                    var path = _paths[i];

                    if (path.Points.Count == 0)
                        continue;

                    _dotRenderer.Points = path.Points.Select((pathPoint, pointIndex) =>
                    {
                        int subIndex = (int)CheckPointPickingSubIndex.Point1;
                        uint pickingId = MktbRendererUtil.GetPickingId(pointIndex, PickingGroupId, PathIdShift, PointIdMask, i, subIndex);
                        bool isSelected = context.IsSelected(pathPoint, subIndex);
                        bool isHovered = context.IsHovered(pathPoint, subIndex);

                        return new InstancedPoint(
                            new Vector3((float)pathPoint.Point1.X, 0, (float)pathPoint.Point1.Y), new(), new(1),
                            GetCheckPointColor(pathPoint), false, pathPoint, pickingId, isHovered, isSelected);

                    }).ToArray();
                    _dotRenderer.Render(context);

                    _dotRenderer.Points = path.Points.Select((pathPoint, pointIndex) =>
                    {
                        int subIndex = (int)CheckPointPickingSubIndex.Point2;
                        uint pickingId = MktbRendererUtil.GetPickingId(pointIndex, PickingGroupId, PathIdShift, PointIdMask, i, subIndex);
                        bool isSelected = context.IsSelected(pathPoint, subIndex);
                        bool isHovered = context.IsHovered(pathPoint, subIndex);

                        return new InstancedPoint(
                            new Vector3((float)pathPoint.Point2.X, 0, (float)pathPoint.Point2.Y), new(), new(1),
                            GetCheckPointColor(pathPoint), false, pathPoint, pickingId, isHovered, isSelected);
                    }).ToArray();
                    _dotRenderer.Render(context);

                    var arrows = new List<InstancedPoint>();

                    _arrowRenderer.Points = path.Points.Select((point, pointIndex) =>
                    {
                        int subIndex = (int)CheckPointPickingSubIndex.Point1;
                        uint pickingId = MktbRendererUtil.GetPickingId(pointIndex, PickingGroupId, PathIdShift, PointIdMask, i, subIndex);
                        bool isSelected = context.IsSelected(point, subIndex);
                        bool isHovered = context.IsHovered(point, subIndex);

                        var dirVec = (point.Point1 - point.Point2).Normalized();
                        float angle = (float)Math.Atan2(dirVec.Y, dirVec.X);

                        return new InstancedPoint(
                            new Vector3((float)(point.Point1.X + point.Point2.X) / 2, 0,
                                (float)(point.Point1.Y + point.Point2.Y) / 2), new(), new(1),
                            GetCheckPointColor(point), true, point, pickingId, isHovered, isSelected, angle);
                    }).ToArray();

                    _arrowRenderer.Render(context);
                }

                return;
            }

            _lineRenderer.Thickness = 3;
            _lineRenderer.Loop = false;
            _lineRenderer.Render2d = true;

            for (int i = 0; i < _paths.Count; i++)
            {
                var path = _paths[i];

                if (path.Points.Count == 0)
                    continue;

                for (int j = 0; j < path.Points.Count; j++)
                {
                    var pathPoint = path.Points[j];
                    var points = new Vector3[2];
                    points[0] = new Vector3((float)pathPoint.Point1.X, 0, (float)pathPoint.Point1.Y);
                    points[1] = new Vector3((float)pathPoint.Point2.X, 0, (float)pathPoint.Point2.Y);

                    _lineRenderer.Color = GetCheckPointColor(pathPoint);

                    if (context.SceneObjectHolder.IsSelected(pathPoint))
                    {
                        var hsl = Color4.ToHsl(_lineRenderer.Color);
                        hsl.Z += 0.2f;
                        _lineRenderer.Color = Color4.FromHsl(hsl);
                    }


                    _lineRenderer.Points = points;
                    _lineRenderer.Render(context.ViewMatrix, context.ProjectionMatrix, context.TranslucentPass, context.ViewportSize);
                }
            }
        }

        private Color GetCheckPointColor(MkdsCheckPoint pathPoint) => pathPoint.KeyPointId != -1 ? KeyColor : RegularColor;

        public override object GetObject(int index) => _paths[index >> PathIdShift].Points[index & PointIdMask];

        public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
        {
            transform = new Transform(new(0), new(0), new(1));

            if (obj is not MkdsCheckPoint checkpoint)
            {
                return false;
            }

            switch ((CheckPointPickingSubIndex)subIndex)
            {
                default:
                    return false;
                case CheckPointPickingSubIndex.Point1:
                    transform.Translation = new Vector3d(checkpoint.Point1.X, 0, checkpoint.Point1.Y);
                    break;
                case CheckPointPickingSubIndex.Point2:
                    transform.Translation = new Vector3d(checkpoint.Point2.X, 0, checkpoint.Point2.Y);
                    break;
            }

            return true;
        }

        public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
        {
            if (obj is MkdsCheckPoint checkpoint)
            {
                switch ((CheckPointPickingSubIndex)subIndex)
                {
                    default:
                        return false;
                    case CheckPointPickingSubIndex.Point1:
                        checkpoint.Point1 = new Vector2d(transform.Translation.X, transform.Translation.Z);
                        break;
                    case CheckPointPickingSubIndex.Point2:
                        checkpoint.Point2 = new Vector2d(transform.Translation.X, transform.Translation.Z);
                        break;
                }

                return true;
            }

            return false;
        }

        public override bool ContainsObject(object obj) => obj is MkdsCheckPoint instance && _paths.Any(x => x.Points.Contains(instance));

        public void Dispose()
        {
            _lineRenderer.Dispose();
        }
    }
}