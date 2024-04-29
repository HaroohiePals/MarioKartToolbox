using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Enemies
{
    [MapObj(MkdsMapObjectId.NsKiller1, new[] { typeof(NsKiller1.RenderPart) }, typeof(NsKiller1.LogicPart))]
    public class NsKiller1 : MObjInstance
    {
        private enum NsKiller1State
        {
            Wait,
            Move,
            Shoot,
            Emit
        }

        private int            _counter;
        private NsKiller1State _state;
        private Pathwalker     _pathwalker;

        public NsKiller1(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            _pathwalker = Pathwalker.FromPath(obji.Path.Target, 0);
            _pathwalker.MakeLengthLinear();
            _context.MObjState.CreateSubObjects(MkdsMapObjectId.NsKiller2, 20, this);
        }

        private void Update()
        {
            switch (_state)
            {
                case NsKiller1State.Wait:
                    if (_counter-- <= 0)
                    {
                        _pathwalker.SetSpeed(2);
                        _state = NsKiller1State.Move;
                    }

                    break;
                case NsKiller1State.Move:
                {
                    if (_pathwalker.Update())
                    {
                        _counter = 45;
                        _state   = NsKiller1State.Shoot;
                    }

                    var pos = _pathwalker.CalcCurrentPointLinearXZ();
                    Position.X = pos.X;
                    Position.Z = pos.Z;
                    break;
                }
                case NsKiller1State.Shoot:
                    if (_counter == 45)
                    {
                        //this is most likely an inline, possibly with 2 others in it
                        // if (!(mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_INAUDIBLE))
                        // {
                        //     NNS_SndPlayerStopSeq(sub_2108E6C(SND_HANDLE_8), 0);
                        //     snd_unkstruct_field0_t unk =
                        //     {
                        //         SET_KILLER_SHOOT_SU,
                        //         &mobj.position,
                        //         9,
                        //         mobj_calcXZCamDist(&mobj.position)
                        //     };
                        //     sub_210B708(&unk, sub_2108E6C(SND_HANDLE_8));
                        // }
                    }
                    else if (_counter <= 0)
                    {
                        _counter = 0;
                        _state   = NsKiller1State.Emit;
                    }

                    _counter--;
                    break;
                case NsKiller1State.Emit:
                    NsKiller2.Spawn(_context, Position, Mtx.Row2, RotY);
                    // if (!(mobj.flags & (MOBJ_INST_FLAGS_DISABLE_VISIBILITY_UPDATES | MOBJ_INST_FLAGS_CLIPPED)))
                    // {
                    //     ptcm_createEmitter(PTCL_RACE_EFFECT_NS_KILLER1_A, &mobj.position);
                    //     ptcm_createEmitter(PTCL_RACE_EFFECT_NS_KILLER1_B, &mobj.position);
                    //     ptcm_createEmitter(PTCL_RACE_EFFECT_NS_KILLER1_C, &mobj.position);
                    // }
                    _counter = 75;
                    _state   = NsKiller1State.Wait;
                    break;
            }
        }

        private void Render()
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.Scale(Scale);
            Mtx.Row3 = Position / 16.0;
            _context.RenderContext.GeState.MultMatrix(Mtx);
            ((RenderPart)_renderParts[0]).Model.Render(this);
        }

        public class RenderPart : RenderPart<NsKiller1>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadModel(_context, this, "NsKiller1.nsbmd");
            }

            protected override void Render(NsKiller1 instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render();
        }

        public class LogicPart : LogicPart<NsKiller1>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                foreach (var instance in _instances)
                    instance._counter = 0;
            }

            protected override void Update(NsKiller1 instance)
                => instance.Update();
        }
    }
}