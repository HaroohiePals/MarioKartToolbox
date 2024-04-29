using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Enemies
{
    [MapObj(MkdsMapObjectId.NsKiller2, new[] { typeof(NsKiller2.RenderPart) }, typeof(NsKiller2.LogicPart))]
    public class NsKiller2 : MObjInstance
    {
        private enum NsKiller2State
        {
            State0,
            State1
        }

        private int            _counter;
        private NsKiller2State _state;

        public NsKiller2(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            Flags |= InstanceFlags.Bit9;
            SetInvisible();
        }

        private void Update()
        {
            // if (mobj.clipAreaMask & driver_getClipAreaMask())
            //     mobj_disableVisibilityUpdates(&mobj);
            // else
            Flags &= ~InstanceFlags.DisableVisibilityUpdates;

            switch (_state)
            {
                case NsKiller2State.State0:
                    if (_counter-- > 0)
                    {
                        // if (!(mobj.flags & MOBJ_INST_FLAGS_DISABLE_VISIBILITY_UPDATES) &&
                        //     mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_WAS_UPDATED)
                        //     sub_210B7EC(sfxEmitterExParams,
                        //         mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_DISTANCE_MASK);
                        // else
                        //     sub_210B7EC(sfxEmitterExParams, mobj_calcXZCamDist(&mobj.position));
                        Position += Velocity;
                        // if (counter < 590)
                        //     mobj_emitSfxFromEmitter(&mobj, SEL_KILLER_FLY_SU);
                    }
                    else
                        SetInvisible();

                    break;
                case NsKiller2State.State1:
                    if (_counter-- > 0)
                    {
                        Velocity.Y -= 0.25;
                        Position   += Velocity;
                    }
                    else
                        SetInvisible();

                    break;
            }
        }

        private void Render()
        {
            if (_counter <= 0)
                return;

            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.Translate(Position / 16.0);
            _context.RenderContext.GeState.MultMatrix(Matrix4x3d.CreateRotationY(MObjUtil.IdxToRad(RotY)));
            _context.RenderContext.GeState.Scale(Scale);

            var renderPart = (RenderPart)_renderParts[0];
            renderPart.ShadowModel.Render(this);
            renderPart.Model.Render(this);
        }

        public static void Spawn(MkdsContext context, in Vector3d position, in Vector3d direction, ushort rotY)
        {
            var logicPart = (LogicPart)context.MObjState.GetLogicPartForObjectId(MkdsMapObjectId.NsKiller2);

            var instance = logicPart?.AcquireFreeObject();
            if (instance == null)
                return;

            instance._counter   = 600;
            instance.Position   = position + (0, 42, 0);
            instance.Mtx[2, 0]  = direction.X;
            instance.Mtx[2, 1]  = 0;
            instance.Mtx[2, 2]  = direction.Z;
            instance.Velocity.X = 6 * direction.X;
            instance.Velocity.Y = 0;
            instance.Velocity.Z = 6 * direction.Z;
            instance.RotY       = rotY;
            instance._state     = NsKiller2State.State0;
        }

        public class RenderPart : RenderPart<NsKiller2>
        {
            public MObjModel Model       { get; private set; }
            public MObjModel ShadowModel { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                Model       = MObjUtil.LoadModel(_context, this, "NsKiller2.nsbmd");
                ShadowModel = MObjUtil.LoadShadowModel(_context, this, "NsKiller2_s.nsbmd");
                ShadowModel.SetAllPolygonIds(63);
            }

            protected override void Render(NsKiller2 instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render();
        }

        public class LogicPart : LogicPart<NsKiller2>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void Update(NsKiller2 instance)
                => instance.Update();
        }
    }
}