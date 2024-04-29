using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;
using System.Reflection;

namespace HaroohiePals.NitroKart.MapObj.Obstacles
{
    [MapObj(MkdsMapObjectId.RotaryBridge, new[] { typeof(RotatingCylinder.RenderPart) }, typeof(RotatingCylinder.LogicPart),
        nameof(RotatingCylinder.RotaryBridgeParams))]
    [MapObj(MkdsMapObjectId.Gear, new[] { typeof(RotatingCylinder.RenderPart) }, typeof(RotatingCylinder.LogicPart),
        nameof(RotatingCylinder.GearParams))]
    [MapObj(MkdsMapObjectId.TestCylinder, new[] { typeof(RotatingCylinder.RenderPart) }, typeof(RotatingCylinder.LogicPart),
        nameof(RotatingCylinder.TestCylinderParams))]
    [MapObj(MkdsMapObjectId.RotaryRoom, new[] { typeof(RotatingCylinder.RenderPart) }, typeof(RotatingCylinder.LogicPart),
        nameof(RotatingCylinder.RotaryRoomParams))]
    public class RotatingCylinder : DColMObjInstance
    {
        private enum RotatingCylinderState
        {
            BeginRotate,
            Rotate,
            EndRotate,
            Idle
        }

        private enum RotatingCylinderType
        {
            TestCylinder,
            GearBlack,
            GearWhite,
            RotaryRoom,
            RotaryBridge
        }

        private StateMachine<RotatingCylinderState> _stateMachine;

        private ushort _startStopDuration;
        private ushort _rotateDuration;
        private ushort _idleDuration;
        private short  _rotYVelocity;
        private double _velocityProgress;
        private double _startStopSpeed;
        private double _field154;

        private RotatingCylinderType _type;

        public RotatingCylinder(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            var settings = (RotatingCylinderSettings)obji.Settings;

            var param = (Params)typeof(RotatingCylinder)
                .GetField((string)arg, BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

            _stateMachine = new StateMachine<RotatingCylinderState>(new StateMachineState[]
            {
                new(BeginRotateStateInit, BeginRotateStateUpdate),
                new(null, RotateStateUpdate),
                new(EndRotateStateInit, EndRotateStateUpdate),
                new(null, IdleStateUpdate)
            });
            _lastPosition = Position;
            _lastMtx      = new Matrix3d(Mtx.Row0, Mtx.Row1, Mtx.Row2);
            _baseMtx      = _lastMtx;
            _field124     = Vector3d.Zero;
            uint v17 = 0;
            if (!param.IsXZFloor)
                v17 = 1;
            _field130          = v17;
            _startStopDuration = settings.StartStopDuration;
            _rotateDuration    = settings.RotateDuration;
            _idleDuration      = settings.IdleDuration;
            _rotYVelocity      = (short)((settings.RotYVelocity << 7) / 100);
            if (settings.NegateRotYVelocity)
                _rotYVelocity = (short)-_rotYVelocity;
            RotY = 0;
            if (_startStopDuration != 0)
                _startStopSpeed = 1.0 / _startStopDuration;
            else
                _startStopSpeed = 0;
            _velocityProgress = 0;
            Size.X            = param.SizeX * Scale.X;
            Size.Y            = param.SizeY * Scale.Y;
            if (param.IsXZFloor)
            {
                _isFloorYZ = false;
                _isFloorXZ = true;
            }
            else
            {
                _isFloorYZ = true;
                _isFloorXZ = false;
            }

            _isBoostPanel               =  false;
            _floorThreshold             =  param.DcolFloorThreshold;
            _shape                      =  DColShape.Cylinder;
            _field138                   =  param.DcolField138;
            Flags                       |= InstanceFlags.Hidden;
            _stateMachine.NextState     =  RotatingCylinderState.BeginRotate;
            _stateMachine.GotoNextState =  true;
            _type                       =  param.Type;
            if (_type == RotatingCylinderType.GearBlack && !((GearSettings)obji.Settings).IsBlack)
                _type = RotatingCylinderType.GearWhite;
            Flags &= ~InstanceFlags.DisableVisibilityUpdates;
            Flags &= ~InstanceFlags.Bit3;
            Alpha =  31;
        }

        private void BeginRotateStateInit()
        {
            _velocityProgress = 0;
        }

        private void BeginRotateStateUpdate()
        {
            _velocityProgress += _startStopSpeed;
            RotY              =  (ushort)(RotY + (short)(_velocityProgress * _rotYVelocity));
            if (_stateMachine.Counter > _startStopDuration)
            {
                _stateMachine.NextState     = RotatingCylinderState.Rotate;
                _stateMachine.GotoNextState = true;
            }
        }

        private void RotateStateUpdate()
        {
            RotY = (ushort)(RotY + _rotYVelocity);
            if (_rotateDuration != 0)
            {
                if (_stateMachine.Counter > _rotateDuration)
                {
                    _stateMachine.NextState     = RotatingCylinderState.EndRotate;
                    _stateMachine.GotoNextState = true;
                }
            }
        }

        private void EndRotateStateInit()
        {
            _velocityProgress = 1;
        }

        private void EndRotateStateUpdate()
        {
            _velocityProgress -= _startStopSpeed;
            RotY              =  (ushort)(RotY + (short)(_velocityProgress * _rotYVelocity));
            if (_stateMachine.Counter > _startStopDuration)
            {
                _rotYVelocity               = (short)-_rotYVelocity;
                _stateMachine.NextState     = RotatingCylinderState.Idle;
                _stateMachine.GotoNextState = true;
            }
        }

        private void IdleStateUpdate()
        {
            if (_stateMachine.Counter > (uint)(ushort)_idleDuration)
            {
                _stateMachine.NextState     = RotatingCylinderState.BeginRotate;
                _stateMachine.GotoNextState = true;
            }
        }

        private void Update()
        {
            int oldRotY = RotY;
            _stateMachine.Execute();
            int deltaRotY = RotY - oldRotY;
            if (deltaRotY > 0x8000)
                deltaRotY -= 0x10000;
            if (deltaRotY < -32768)
                deltaRotY += 0x10000;
            _field124 =  Mtx.Row1 * deltaRotY;
            _lastMtx  =  new Matrix3d(Mtx.Row0, Mtx.Row1, Mtx.Row2);
            Mtx       =  Matrix4x3d.CreateRotationY(MObjUtil.IdxToRad(RotY));
            Mtx       *= new Matrix4x3d(_baseMtx.Row0, _baseMtx.Row1, _baseMtx.Row2, Vector3d.Zero);
            _basePos  =  -_size.Y * Mtx.Row1 + Position;
        }

        private void Render(byte alpha)
        {
            Mtx.Row3 = Position / 16.0;
            MObjUtil.Model2RenderModel(_context, _model, Mtx, Scale, alpha);
        }

        private record Params(
            bool IsXZFloor,
            double SizeX,
            double SizeY,
            RotatingCylinderType Type,
            double DcolFloorThreshold,
            uint DcolField138);

        private static readonly Params RotaryRoomParams
            = new(true, 472, 25, RotatingCylinderType.RotaryRoom, 0.5, 1);

        private static readonly Params RotaryBridgeParams
            = new(false, 80, 2150, RotatingCylinderType.RotaryBridge, 0.5, 1);

        private static readonly Params GearParams
            = new(true, 300, 50, RotatingCylinderType.GearBlack, 0.5, 1);

        private static readonly Params TestCylinderParams
            = new(false, 299, 50, RotatingCylinderType.TestCylinder, 357 / 4096.0, 1);

        public class RenderPart : RenderPart<RotatingCylinder>
        {
            public Model TestCylinderModel { get; private set; }
            public Model GearBlackModel    { get; private set; }
            public Model GearWhiteModel    { get; private set; }
            public Model RotaryRoomModel   { get; private set; }
            public Model RotaryBridgeModel { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                var typeLoaded = new bool[5];
                foreach (var instance in _instances)
                {
                    if (!typeLoaded[(int)instance._type])
                    {
                        switch (instance._type)
                        {
                            case RotatingCylinderType.TestCylinder:
                                TestCylinderModel = new Model(_context,
                                    MObjUtil.GetMapObjFile<Nsbmd>(_context, "test_cylinder.nsbmd"));
                                TestCylinderModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
                                break;
                            case RotatingCylinderType.GearBlack:
                                GearBlackModel = new Model(_context,
                                    MObjUtil.GetMapObjFile<Nsbmd>(_context, "gear_black.nsbmd"));
                                GearBlackModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
                                break;
                            case RotatingCylinderType.GearWhite:
                                GearWhiteModel = new Model(_context,
                                    MObjUtil.GetMapObjFile<Nsbmd>(_context, "gear_white.nsbmd"));
                                GearWhiteModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
                                break;
                            case RotatingCylinderType.RotaryRoom:
                                RotaryRoomModel = new Model(_context,
                                    MObjUtil.GetMapObjFile<Nsbmd>(_context, "rotary_room.nsbmd"));
                                RotaryRoomModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
                                break;
                            case RotatingCylinderType.RotaryBridge:
                                RotaryBridgeModel = new Model(_context,
                                    MObjUtil.GetMapObjFile<Nsbmd>(_context, "rotary_bridge.nsbmd"));
                                RotaryBridgeModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
                                break;
                        }

                        typeLoaded[(int)instance._type] = true;
                    }
                }

                foreach (var instance in _instances)
                {
                    switch (instance._type)
                    {
                        case RotatingCylinderType.TestCylinder:
                            instance._model = TestCylinderModel;
                            break;
                        case RotatingCylinderType.GearBlack:
                            instance._model = GearBlackModel;
                            break;
                        case RotatingCylinderType.GearWhite:
                            instance._model = GearWhiteModel;
                            break;
                        case RotatingCylinderType.RotaryRoom:
                            instance._model = RotaryRoomModel;
                            break;
                        case RotatingCylinderType.RotaryBridge:
                            instance._model = RotaryBridgeModel;
                            break;
                    }
                }
            }

            protected override void Render(RotatingCylinder instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render((byte)alpha);
        }

        public class LogicPart : LogicPart<RotatingCylinder>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                int v3 = -1;
                for (int i = 0; i < _instances.Count; i++)
                {
                    try
                    {
                        var instance = _instances[i];
                        if (instance.ObjectId != MkdsMapObjectId.Gear)
                            continue;
                        if (v3 < 0)
                            instance._field138 = 0;
                        else
                        {
                            var ab = _instances[v3].Position - instance.Position;
                            ab.Y = 0;
                            instance._field154 = ab.Length;
                            instance._field154 -= _instances[v3].Size.X;
                        }

                        v3 = i;
                    }
                    catch { }
                }
            }

            protected override void Update(RotatingCylinder instance)
                => instance.Update();
        }
    }
}