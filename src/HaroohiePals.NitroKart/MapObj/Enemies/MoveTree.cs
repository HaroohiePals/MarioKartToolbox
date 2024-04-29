using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Enemies
{
    [MapObj(MkdsMapObjectId.MoveTree, new[] { typeof(MoveTree.RenderPart) }, typeof(MoveTree.LogicPart))]
    public class MoveTree : MObjInstance
    {
        private enum MTreeState
        {
            Wait,
            Walk
        }

        private int        _pointDuration;
        private int        _counter;
        private double     _speed;
        private ushort     _nsbcaFrame;
        private int        _nsbcaFrameDelta;
        private Pathwalker _pathwalker;

        private MTreeState _state;
        // objshadow_t     shadow;

        public MoveTree(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            _pathwalker = Pathwalker.FromPath(obji.Path.Target, 0);
        }

        public void Update()
        {
            var model = ((RenderPart)_renderParts[0]).Model;
            switch (_state)
            {
                case MTreeState.Wait:
                    _pathwalker.SetSpeed(0);
                    if (_pointDuration-- <= 0)
                        _state = MTreeState.Walk;
                    // if ((s32)(mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_DISTANCE_MASK) >= 810000)
                    //     break;
                    if (_counter-- > 0)
                    {
                        _nsbcaFrameDelta = 2;
                        _nsbcaFrame      = (ushort)(_nsbcaFrame + _nsbcaFrameDelta);
                        if (_nsbcaFrame >= model.NsbcaAnim.GetAnimLength())
                            _nsbcaFrame = 0;
                    }
                    else if (_nsbcaFrame != 0)
                    {
                        _nsbcaFrame = (ushort)(_nsbcaFrame + _nsbcaFrameDelta);
                        if (_nsbcaFrame >= model.NsbcaAnim.GetAnimLength())
                            _nsbcaFrame = 0;
                        // if (nsbcaFrame == 0 || nsbcaFrame == 30)
                        //     mobj_emitSfxIfAudible(&mobj, SET_WALKINGTREE_SU);
                    }

                    break;
                case MTreeState.Walk:
                    _pathwalker.SetSpeed(_speed);
                    if (_pathwalker.Update())
                    {
                        _pointDuration = (ushort)_pathwalker.PrevPoit.Duration;
                        _state         = MTreeState.Wait;
                    }

                    var pos = _pathwalker.CalcCurrentPointLinearXZ();
                    Position.X = pos.X;
                    Position.Z = pos.Z;
                    // if ((s32)(mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_DISTANCE_MASK) >= 810000)
                    //     break;
                    if (_counter-- > 0)
                        _nsbcaFrameDelta = 2;
                    else
                        _nsbcaFrameDelta = 1;
                    _nsbcaFrame = (ushort)(_nsbcaFrame + _nsbcaFrameDelta);
                    if (_nsbcaFrame >= model.NsbcaAnim.GetAnimLength())
                        _nsbcaFrame = 0;
                    // if (nsbcaFrame == 0 || nsbcaFrame == 30)
                    //     mobj_emitSfxIfAudible(&mobj, SET_WALKINGTREE_SU);
                    break;
            }
        }

        public void Render(in Matrix4x3d camMtx, byte alpha)
        {
            var model = ((RenderPart)_renderParts[0]).Model;
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetYBillboardAtPos(Position, camMtx));
            _context.RenderContext.GeState.Scale(Scale);
            model.NsbcaAnim.SetFrame(_nsbcaFrame);
            model.Render(this);
            //oshd
        }

        public class RenderPart : RenderPart<MoveTree>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadModel(_context, this, "move_tree.nsbmd");
                MObjUtil.LoadNsbcaAnim(_context, Model, "move_tree.nsbca");
                Model.SetAllPolygonIds(63);
            }

            protected override void Render(MoveTree instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx, (byte)alpha);
        }

        public class LogicPart : LogicPart<MoveTree>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                foreach (var instance in _instances)
                {
                    instance._speed = ((MoveTreeSettings)instance.ObjiEntry.Settings).PathSpeed / 100.0;
                    //oshd_setParams
                }
            }

            protected override void Update(MoveTree instance)
                => instance.Update();
        }
    }
}