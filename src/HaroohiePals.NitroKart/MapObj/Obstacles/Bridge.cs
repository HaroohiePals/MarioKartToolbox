using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Obstacles
{
    [MapObj(MkdsMapObjectId.Bridge, new[] { typeof(Bridge.RenderPart) }, typeof(Bridge.LogicPart))]
    public class Bridge : DColMObjInstance
    {
        private ushort _field144;
        private ushort _rotSpeed;
        private ushort _angle;
        private ushort _field14A;
        private ushort _openWaitFrames;
        private ushort _closedWaitFrames;
        private ushort _waitCounter;
        private double _animProgress;

        public Bridge(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            _lastPosition     = Position;
            _lastMtx          = new Matrix3d(Mtx.Row0, Mtx.Row1, Mtx.Row2);
            _baseMtx          = _lastMtx;
            _field124         = Vector3d.Zero;
            _field130         = 1;
            _field144         = (ushort)((obji.Settings.Settings[0] << 16) / 360 >> 1);
            _rotSpeed         = (ushort)(obji.Settings.Settings[1] != 0 ? (0x10000 / obji.Settings.Settings[1] >> 1) : 0);
            _openWaitFrames   = (ushort)obji.Settings.Settings[2];
            _closedWaitFrames = (ushort)obji.Settings.Settings[3];
            _waitCounter      = 0;
            _angle            = 0;
            // if (rconf_getRaceMode() == RACE_MODE_NT)
            // {
            //     rotSpeed = 0;
            //     angle    = (obji->settings[4] << 16) / 360 >> 1;
            // }

            _size.X         =  Scale.X * 90;
            _size.Y         =  Scale.Y * 30 / 2;
            _size.Z         =  Scale.Z * 350;
            _sizeZ2         =  Scale.Z * 50;
            _isFloorYZ      =  false;
            _isFloorXZ      =  true;
            _isFloorXY      =  false;
            _isBoostPanel   =  true;
            _floorThreshold =  0.5;
            _shape          =  DColShape.Box;
            _field138       =  0;
            Flags           |= InstanceFlags.Hidden;
            Flags           &= ~InstanceFlags.DisableVisibilityUpdates;
            Flags           &= ~InstanceFlags.Bit3;
            Alpha           =  31;
            // if (obji.Rotation.Y < 0)
            // mobj_initSoundEmitterForObjectSfxParam4(&mobj);
        }

        private void Update()
        {
            ushort v3 = (ushort)(0x10000 - (ushort)(MObjUtil.SinIdx(_angle) * _field144 + _field144));
            int    v4 = v3 - _field14A;
            if (v4 > 0x8000)
                v4 -= 0x10000;
            if (v4 < -0x8000)
                v4 += 0x10000;
            _field124 =  Mtx.Row0 * v4;
            _field14A =  v3;
            _lastMtx  =  new Matrix3d(Mtx.Row0, Mtx.Row1, Mtx.Row2);
            Mtx       =  Matrix4x3d.CreateRotationX(MObjUtil.IdxToRad(v3));
            Mtx       *= new Matrix4x3d(_baseMtx.Row0, _baseMtx.Row1, _baseMtx.Row2, Vector3d.Zero);
            _basePos  =  -_size.Y * Mtx.Row1 + Position;
            if (_waitCounter != 0)
            {
                _waitCounter--;
                return;
            }

            // if (mobj.soundEmitter)
            //     mobj_emitSfxFromEmitter(&mobj, SET_BRIDGE_SU);
            _angle += _rotSpeed;
            if (_angle >= 0x4000 && _angle < _rotSpeed + 0x4000)
                _waitCounter = _openWaitFrames;
            else if (_angle >= MObjUtil.DegToIdx(270) && _angle < _rotSpeed + MObjUtil.DegToIdx(270))
                _waitCounter = _closedWaitFrames;
            _animProgress = (2 * ((0x1680000L * (ushort)(0x10000 - _field14A) + 0x80000) >> 20)) / 4096.0;
            var renderPart = (RenderPart)_renderParts[0];
            if (_animProgress > renderPart.NsbcaAnim.GetAnimLength())
                _animProgress = renderPart.NsbcaAnim.GetAnimLength();
            if (_animProgress < 0)
                _animProgress = 0;
        }

        private void Render(byte alpha)
        {
            Mtx.Row3 = Position / 16.0;
            var renderPart = (RenderPart)_renderParts[0];
            renderPart.NsbcaAnim.SetFrame(_animProgress);
            MObjUtil.Model2RenderModel(_context, _model, Mtx, Scale, alpha);
        }

        public class RenderPart : RenderPart<Bridge>
        {
            public Model       Model     { get; private set; }
            public AnimManager NsbcaAnim { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                var nsbmd = MObjUtil.GetMapObjFile<Nsbmd>(_context, "bridge.nsbmd");
                Model     = new Model(_context, nsbmd);
                NsbcaAnim = new AnimManager(AnimManager.AnimKind.Jnt, Model, 1);
                NsbcaAnim.RegisterAllAnims(MObjUtil.GetMapObjFile<Nsbca>(_context, "bridge.nsbca"));
                NsbcaAnim.SetAnim(0);
                Model.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
                foreach (var instance in _instances)
                    instance._model = Model;
            }

            protected override void Render(Bridge instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render((byte)alpha);
        }

        public class LogicPart : LogicPart<Bridge>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void Update(Bridge instance)
                => instance.Update();
        }
    }
}