using HaroohiePals.NitroKart.MapData;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Scenery
{
    [MapObj(MkdsMapObjectId.Of6TreeNocol, new[] { typeof(Of6Tree1RenderPart) })]
    [MapObj(MkdsMapObjectId.OlaTree1C, new[] { typeof(OlaTree1cRenderPart) })]
    [MapObj(MkdsMapObjectId.DeTree1, new[] { typeof(DeTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.OlgMush1, new[] { typeof(OlgMush1RenderPart) })]
    [MapObj(MkdsMapObjectId.OlgPipe1, new[] { typeof(OlgPipe1RenderPart) })]
    [MapObj(MkdsMapObjectId.BeachTree1, new[] { typeof(BeachTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.NsCannon1, new[] { typeof(NsCannon1RenderPart) })]
    [MapObj(MkdsMapObjectId.CrossTree1, new[] { typeof(CrossTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.BankTree1, new[] { typeof(BankTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.Of6Yoshi1, new[] { typeof(Of6Yoshi1RenderPart) })]
    [MapObj(MkdsMapObjectId.GardenTree1Nocol, new[] { typeof(GardenTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.GardenTree1, new[] { typeof(GardenTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.TownTree1Nocol, new[] { typeof(TownTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.TownTree1, new[] { typeof(TownTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.MarioTree3, new[] { typeof(MarioTree3RenderPart) })]
    [MapObj(MkdsMapObjectId.SnowTree1Nocol, new[] { typeof(SnowTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.OpaTree1, new[] { typeof(OpaTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.SnowTree1, new[] { typeof(SnowTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.DeTree1Nocol, new[] { typeof(DeTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.OsaTree1C, new[] { typeof(OsaTree1cRenderPart) })]
    [MapObj(MkdsMapObjectId.KinoHouse1, new[] { typeof(KinoHouse1RenderPart) })]
    [MapObj(MkdsMapObjectId.Of6Tree, new[] { typeof(Of6Tree1RenderPart) })]
    [MapObj(MkdsMapObjectId.KinoHouse2, new[] { typeof(KinoHouse2RenderPart) })]
    [MapObj(MkdsMapObjectId.KinoMount1, new[] { typeof(KinoMount1RenderPart) })]
    [MapObj(MkdsMapObjectId.KinoMount2, new[] { typeof(KinoMount2RenderPart) })]
    [MapObj(MkdsMapObjectId.Om6Tree1, new[] { typeof(Om6Tree1RenderPart) })]
    [MapObj(MkdsMapObjectId.BeachTree1Nocol, new[] { typeof(BeachTree1RenderPart) })]
    [MapObj(MkdsMapObjectId.BankEgg1, new[] { typeof(BankEgg1RenderPart) })]
    [MapObj(MkdsMapObjectId.MiniDokan, new[] { typeof(MiniDokanRenderPart) })]
    public class BillboardObject : MObjInstance
    {
        public BillboardObject(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        private void Render(in Matrix4x3d camMtx)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetYBillboardAtPos(Position, camMtx));
            _context.RenderContext.GeState.Scale(Scale);
            var renderPart = (RenderPart)_renderParts[0];
            renderPart.Model.Render(this);
        }

        public abstract class RenderPart : RenderPart<BillboardObject>
        {
            public MObjModel Model { get; private set; }

            private readonly string _modelName;

            protected RenderPart(MkdsContext context, string modelName)
                : base(context, RenderPartType.Billboard)
            {
                _modelName = modelName;
            }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadBillboardModel(_context, this, _modelName);
                Model.SetAllPolygonIds(63);
            }

            protected override void GlobalPreRender()
            {
                Model.BbModel.ApplyMaterial();
            }

            protected override void Render(BillboardObject instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx);
        }

        public class BeachTree1RenderPart : RenderPart
        {
            public BeachTree1RenderPart(MkdsContext context)
                : base(context, "BeachTree1.nsbmd") { }
        }

        public class OpaTree1RenderPart : RenderPart
        {
            public OpaTree1RenderPart(MkdsContext context)
                : base(context, "opa_tree1.nsbmd") { }
        }

        public class OlgMush1RenderPart : RenderPart
        {
            public OlgMush1RenderPart(MkdsContext context)
                : base(context, "OlgMush1.nsbmd") { }
        }

        public class OlgPipe1RenderPart : RenderPart
        {
            public OlgPipe1RenderPart(MkdsContext context)
                : base(context, "OlgPipe1.nsbmd") { }
        }

        public class Of6Yoshi1RenderPart : RenderPart
        {
            public Of6Yoshi1RenderPart(MkdsContext context)
                : base(context, "of6yoshi1.nsbmd") { }
        }

        public class NsCannon1RenderPart : RenderPart
        {
            public NsCannon1RenderPart(MkdsContext context)
                : base(context, "NsCannon1.nsbmd") { }
        }

        public class CrossTree1RenderPart : RenderPart
        {
            public CrossTree1RenderPart(MkdsContext context)
                : base(context, "CrossTree1.nsbmd") { }
        }

        public class BankTree1RenderPart : RenderPart
        {
            public BankTree1RenderPart(MkdsContext context)
                : base(context, "Bank_Tree1.nsbmd") { }
        }

        public class GardenTree1RenderPart : RenderPart
        {
            public GardenTree1RenderPart(MkdsContext context)
                : base(context, "GardenTree1.nsbmd") { }
        }

        public class TownTree1RenderPart : RenderPart
        {
            public TownTree1RenderPart(MkdsContext context)
                : base(context, "TownTree1.nsbmd") { }
        }

        public class MarioTree3RenderPart : RenderPart
        {
            public MarioTree3RenderPart(MkdsContext context)
                : base(context, "MarioTree3.nsbmd") { }
        }

        public class SnowTree1RenderPart : RenderPart
        {
            public SnowTree1RenderPart(MkdsContext context)
                : base(context, "Snow_Tree1.nsbmd") { }
        }

        public class DeTree1RenderPart : RenderPart
        {
            public DeTree1RenderPart(MkdsContext context)
                : base(context, "DeTree1.nsbmd") { }
        }

        public class OlaTree1cRenderPart : RenderPart
        {
            public OlaTree1cRenderPart(MkdsContext context)
                : base(context, "olaTree1c.nsbmd") { }
        }

        public class OsaTree1cRenderPart : RenderPart
        {
            public OsaTree1cRenderPart(MkdsContext context)
                : base(context, "osaTree1c.nsbmd") { }
        }

        public class KinoHouse1RenderPart : RenderPart
        {
            public KinoHouse1RenderPart(MkdsContext context)
                : base(context, "KinoHouse1.nsbmd") { }
        }

        public class KinoHouse2RenderPart : RenderPart
        {
            public KinoHouse2RenderPart(MkdsContext context)
                : base(context, "KinoHouse2.nsbmd") { }
        }

        public class KinoMount1RenderPart : RenderPart
        {
            public KinoMount1RenderPart(MkdsContext context)
                : base(context, "KinoMount1.nsbmd") { }
        }

        public class KinoMount2RenderPart : RenderPart
        {
            public KinoMount2RenderPart(MkdsContext context)
                : base(context, "KinoMount2.nsbmd") { }
        }

        public class Om6Tree1RenderPart : RenderPart
        {
            public Om6Tree1RenderPart(MkdsContext context)
                : base(context, "om6Tree1.nsbmd") { }
        }

        public class Of6Tree1RenderPart : RenderPart
        {
            public Of6Tree1RenderPart(MkdsContext context)
                : base(context, "Of6Tree1.nsbmd") { }
        }

        public class BankEgg1RenderPart : RenderPart
        {
            public BankEgg1RenderPart(MkdsContext context)
                : base(context, "BankEgg1.nsbmd") { }
        }

        public class MiniDokanRenderPart : RenderPart
        {
            public MiniDokanRenderPart(MkdsContext context)
                : base(context, "mini_dokan.nsbmd") { }
        }
    }
}