using HaroohiePals.NitroKart.MapData;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Water
{
    [MapObj(MkdsMapObjectId.Puddle, new[] { typeof(Puddle.RenderPart) })]
    public class Puddle : MObjInstance
    {
        public Puddle(MkdsContext context, MapObj.RenderPart[] renderParts, LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        private void Render()
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.Translate((Position + (0, 1.8, 0)) / 16.0);
            _context.RenderContext.GeState.Scale(Scale);
            _context.RenderContext.GeState.MultMatrix(new Matrix3d(Mtx.Row0, Mtx.Row1, Mtx.Row2));
            var renderPart = (RenderPart)_renderParts[0];
            renderPart.Model.Render(this);
        }

        public class RenderPart : RenderPart<Puddle>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Billboard) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadBillboardModel(_context, this, "puddle.nsbmd");
            }

            protected override void GlobalPreRender()
                => Model.BbModel.ApplyMaterial();

            protected override void Render(Puddle instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render();
        }
    }
}