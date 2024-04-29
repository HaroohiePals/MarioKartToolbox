using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Scenery
{
    [MapObj(MkdsMapObjectId.Cow, new[] { typeof(Cow.RenderPart) })]
    public class Cow : MObjInstance
    {
        public Cow(MkdsContext context, MapObj.RenderPart[] renderParts, LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        private void Render(in Matrix4x3d camMtx)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetYBillboardAtPos(Position, camMtx));
            _context.RenderContext.GeState.Scale(Scale);
            var renderPart = (RenderPart)_renderParts[0];
            renderPart.Model.SetNsbtpFrame(((CowSettings)ObjiEntry.Settings).NsbtpFrame);
            renderPart.Model.Render(this);
        }

        public class RenderPart : RenderPart<Cow>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Billboard) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadTexAnimBillboardModel(_context, this, "cow.nsbmd", "cow.nsbtp");
                Model.SetAllPolygonIds(63);
            }

            protected override void GlobalPreRender()
            {
                Model.BbModel.ApplyMaterial();
            }

            protected override void Render(Cow instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx);
        }
    }
}