using HaroohiePals.Graphics3d.OpenGL.Renderers;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.OpenGL.Renderers;
using HaroohiePals.Mathematics;
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
        private const int AREA_SHAPE_SUB_INDEX = 0;

        protected readonly MapDataCollection<MkdsArea> _collection;
        public Color Color { get; set; }

        //private const int ShapeShift = 13;
        //private const int PointIdMask = 0x1FFF;

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

        private InstancedPoint[] SetupPoints(ViewportContext context, MkdsAreaShapeType shape)
        {
            var points = _collection.Where(x => x.Shape == shape).Select(x =>
            {
                uint pickingId = ViewportContext.GetPickingId(PickingGroupId, _collection.IndexOf(x), AREA_SHAPE_SUB_INDEX); // MktbRendererUtil.GetPickingId(i, PickingGroupId, ShapeShift, PointIdMask, (int)shape, 0);
                bool isSelected = context.IsSelected(x, AREA_SHAPE_SUB_INDEX);
                bool isHovered = context.IsHovered(x, AREA_SHAPE_SUB_INDEX);

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
            _boxRenderer.Points = SetupPoints(context, MkdsAreaShapeType.Box);
            _boxRenderer.Render(context);

            //Cylinder
            _cylinderRenderer.Points = SetupPoints(context, MkdsAreaShapeType.Cylinder);
            _cylinderRenderer.Render(context);
        }

        public override object GetObject(int index)
            => _collection[index];

        public override bool ContainsObject(object obj) 
            => obj is MkdsArea instance && _collection.Contains(instance);

        public override bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
        {
            if (subIndex != AREA_SHAPE_SUB_INDEX || obj is not MkdsArea area)
            {
                transform = new Transform(new(0), new(0), new(1));
                return false;
            }

            transform = area.GetTransform();

            return true;
        }

        public override bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
        {
            if (subIndex != AREA_SHAPE_SUB_INDEX || obj is not MkdsArea area)
                return false;

            area.SetTransform(transform);

            return true;
        }

        public override bool TryGetLocalObjectBounds(object obj, int subIndex, out AxisAlignedBoundingBox bounds)
        {
            if (subIndex != AREA_SHAPE_SUB_INDEX || obj is not MkdsArea area)
            {
                bounds = AxisAlignedBoundingBox.Zero;
                return false;
            }

            bounds = area.GetLocalBounds();
            return true;
        }

        public void Dispose()
        {
            _boxRenderer.Dispose();
            _cylinderRenderer.Dispose();
        }
    }
}
