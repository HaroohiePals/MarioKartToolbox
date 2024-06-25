using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.MapData
{
    internal class AreaShapeRenderGroup : RenderGroup, IColoredRenderGroup, IDisposable
    {
        protected readonly MapDataCollection<MkdsArea> _collection;
        public Color Color { get; set; }

        private const int ShapeShift = 13;
        private const int PointIdMask = 0x1FFF;

        private MeshRenderer _boxRenderer;
        private MeshRenderer _cylinderRenderer;

        public bool ShowAll = false;

        public AreaShapeRenderGroup(MapDataCollection<MkdsArea> collection, Color color, bool render2d, IRendererFactory rendererFactory)
        {
            Color = color;
            _collection = collection;
            _cylinderRenderer = rendererFactory.CreateCylinderAreaRenderer(render2d);
            _boxRenderer = rendererFactory.CreateBoxAreaRenderer(render2d);
        }

        private InstancedPoint[] SetupPoints(ViewportContext context, IEnumerable<MkdsArea> areas, MkdsAreaShapeType shape)
        {
            var points = areas.Where(x => x.Shape == shape).Select((x, i) =>
            {
                uint pickingId = MktbRendererUtil.GetPickingId(i, PickingGroupId, ShapeShift, PointIdMask, (int)shape);
                bool isSelected = context.IsSelected(x);
                bool isHovered = context.IsHovered(x);

                return new InstancedPoint(x.GetTransformMatrix(), Color, false, x, pickingId, isHovered, isSelected);
            }).ToArray();

            return points;
        }

        public override void Render(ViewportContext context)
        {
            if (!context.TranslucentPass)
                return;

            var areas = ShowAll ? _collection : context.SceneObjectHolder.GetSelection().OfType<MkdsArea>();

            //Box
            _boxRenderer.Points = SetupPoints(context, areas, MkdsAreaShapeType.Box);
            _boxRenderer.Render(context);

            //Cylinder
            _cylinderRenderer.Points = SetupPoints(context, areas, MkdsAreaShapeType.Cylinder);
            _cylinderRenderer.Render(context);
        }

        public override object GetObject(int index)
        {
            var shape = (MkdsAreaShapeType)(index >> ShapeShift);

            var areasWithShape = _collection.Where(x => x.Shape == shape).ToArray();

            return areasWithShape[index & PointIdMask];
        }

        public override bool ContainsObject(object obj) => obj is MkdsArea instance && _collection.Contains(instance);

        public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
        {
            if (subIndex != -1 || obj is not MkdsArea area)
            {
                transform = new Transform(new(0), new(0), new(1));
                return false;
            }

            transform = new Transform(area.Position, area.GetRotation(), area.LengthVector);

            return true;
        }

        public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
        {
            if (subIndex != -1 || obj is not MkdsArea area)
                return false;

            area.Position = transform.Translation;
            area.SetRotation(transform.Rotation);
            area.LengthVector = transform.Scale;

            return true;
        }

        public void Dispose()
        {
            _boxRenderer.Dispose();
            _cylinderRenderer.Dispose();
        }
    }
}
