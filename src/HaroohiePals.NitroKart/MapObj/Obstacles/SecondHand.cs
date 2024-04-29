using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Obstacles
{
    [MapObj(MkdsMapObjectId.SecondHand, new[] { typeof(SecondHand.RenderPart) }, typeof(SecondHand.LogicPart))]
    public class SecondHand : MObjInstance
    {
        private enum SecondHandState
        {
            Accelerate,
            Move,
            Decelerate,
            Wait
        }

        private StateMachine<SecondHandState> _stateMachine;

        private Quaterniond _curRotation;
        private Quaterniond _baseRotation;
        private ushort      _startStopFrameCount;
        private ushort      _oneDirFrameCount;
        private ushort      _waitFrameCount;
        private short       _baseVelocity;
        private double      _velocity;
        private double      _acceleration;

        public SecondHand(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            Size.Y = Scale.X * 75;

            _stateMachine = new StateMachine<SecondHandState>(new StateMachineState[]
            {
                new(AccelerateStateInit, AccelerateStateUpdate),
                new(null, MoveStateUpdate),
                new(DecelerateStateInit, DecelerateStateUpdate),
                new(null, WaitStateUpdate)
            });

            _curRotation  = MObjUtil.QtrnFromEulerAngles(obji.Rotation);
            _baseRotation = _curRotation;

            _startStopFrameCount = (ushort)obji.Settings.Settings[2];
            _oneDirFrameCount    = (ushort)obji.Settings.Settings[1];
            _waitFrameCount      = (ushort)obji.Settings.Settings[3];
            _baseVelocity        = (short)((obji.Settings.Settings[0] << 7) / 100);

            if (obji.Settings.Settings[4] != 0)
                _baseVelocity = (short)-_baseVelocity;

            RotY = 0;

            if (_startStopFrameCount != 0)
                _acceleration = 1.0 / _startStopFrameCount;
            else
                _acceleration = 0;

            _velocity = 0;

            _stateMachine.NextState     = SecondHandState.Accelerate;
            _stateMachine.GotoNextState = true;

            Flags |= InstanceFlags.Bit9;
        }

        private void AccelerateStateInit()
        {
            _velocity = 0;
        }

        private void AccelerateStateUpdate()
        {
            _velocity += _acceleration;
            RotY      =  (ushort)(RotY + (short)(_velocity * _baseVelocity));

            if (_stateMachine.Counter > _startStopFrameCount)
            {
                _stateMachine.NextState     = SecondHandState.Move;
                _stateMachine.GotoNextState = true;
            }
        }

        private void MoveStateUpdate()
        {
            RotY = (ushort)(RotY + _baseVelocity);

            if (_oneDirFrameCount != 0 && _stateMachine.Counter > _oneDirFrameCount)
            {
                _stateMachine.NextState     = SecondHandState.Decelerate;
                _stateMachine.GotoNextState = true;
            }
        }

        private void DecelerateStateInit()
        {
            _velocity = 4;
        }

        private void DecelerateStateUpdate()
        {
            _velocity -= _acceleration;
            RotY      =  (ushort)(RotY + (short)(_velocity * _baseVelocity));

            if (_stateMachine.Counter > _startStopFrameCount)
            {
                _baseVelocity = (short)-_baseVelocity;

                _stateMachine.NextState     = SecondHandState.Wait;
                _stateMachine.GotoNextState = true;
            }
        }

        private void WaitStateUpdate()
        {
            if (_stateMachine.Counter > _waitFrameCount)
            {
                _stateMachine.NextState     = SecondHandState.Accelerate;
                _stateMachine.GotoNextState = true;
            }
        }

        private void Update()
        {
            _stateMachine.Execute();
            _curRotation = MObjUtil.QtrnRotY(_baseRotation, RotY);
            var mtx4 = Matrix4d.CreateFromQuaternion(_curRotation);
            Mtx = new Matrix4x3d(mtx4.Row0.Xyz, mtx4.Row1.Xyz, mtx4.Row2.Xyz, Vector3d.Zero);
        }

        private void Render(byte alpha)
        {
            Mtx.Row3 = Position / 16.0;
            var renderPart = (RenderPart)_renderParts[0];
            MObjUtil.Model2RenderModel(_context, renderPart.Model, Mtx, Scale, alpha);
        }

        public class RenderPart : RenderPart<SecondHand>
        {
            public Model Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                var nsbmd = MObjUtil.GetMapObjFile<Nsbmd>(_context, "second_hand.nsbmd");
                Model = new Model(_context, nsbmd);
                Model.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
            }

            protected override void Render(SecondHand instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render((byte)alpha);
        }

        public class LogicPart : LogicPart<SecondHand>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void Update(SecondHand instance)
                => instance.Update();
        }
    }
}