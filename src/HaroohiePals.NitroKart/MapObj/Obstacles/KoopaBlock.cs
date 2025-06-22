using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.MapObj.Obstacles
{
    [MapObj(MkdsMapObjectId.KoopaBlock, new[] { typeof(KoopaBlock.RenderPart) }, typeof(KoopaBlock.LogicPart))]
    public class KoopaBlock : DColMObjInstance
    {
        private enum KoopaBlockState
        {
            SpeedUp,
            Move,
            SlowDown
        }

        private StateMachine<KoopaBlockState> _stateMachine;

        private Pathwalker _pathwalker;
        private double _speed;
        private ushort _waitCounter;

        public KoopaBlock(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            _stateMachine = new StateMachine<KoopaBlockState>(new StateMachineState[]
            {
                new(SpeedUpStateInit, SpeedUpStateUpdate),
                new(MoveStateinit, MoveStateUpdate),
                new(SlowDownStateInit, SlowDownStateUpdate)
            });
            _speed = ((KoopaBlockSettings)obji.Settings).PathSpeed / 100.0;
            _pathwalker = Pathwalker.FromPath(obji.Path.Target, _speed);
            _waitCounter = 0;
            _lastMtx = new Matrix3d(Mtx.Row0, Mtx.Row1, Mtx.Row2);
            _field124 = Vector3d.Zero;
            _field130 = 0;
            _size.X = Scale.X * 400;
            _size.Y = Scale.Y * 100 / 2;
            _size.Z = Scale.Z * 175;
            _sizeZ2 = Scale.Z * 175;
            _isFloorYZ = false;
            _isFloorXZ = true;
            _isFloorXY = false;
            _isBoostPanel = false;
            _floorThreshold = 0.5;
            _pathwalker.Init(0, true);
            _pathwalker.CalcCurrentPointLinearXYZSpecial(out Position, out Velocity);
            _lastPosition = Position;
            _shape = DColShape.Box;
            _field138 = 0;
            Flags |= InstanceFlags.Hidden;
            Flags &= ~InstanceFlags.DisableVisibilityUpdates;
            Flags &= ~InstanceFlags.Bit3;
            Alpha = 31;
            _stateMachine.NextState = KoopaBlockState.SpeedUp;
            _stateMachine.GotoNextState = true;
            //instance->dcolMObj.mobj.sfxMaxDistanceSquare = gSfxParams[3].maxDistanceSquare;
        }

        private void SpeedUpStateInit()
        {
        }

        private void SpeedUpStateUpdate()
        {
            double sin;
            double val;
            double val2;

            sin = MObjUtil.SinIdx((ushort)(_pathwalker.Progress * 64.0));
            val = sin * _speed;
            _pathwalker.Speed = val > 0.125 ? val : 0.125;
            val2 = _pathwalker.Path.Parts[_pathwalker.PartIdx].OneDivLength;
            if (val <= 0.125)
                val = 0.125;
            _pathwalker.PartSpeed = val * val2;
            if (_pathwalker.Progress < 0.0625)
                return;
            _stateMachine.NextState = KoopaBlockState.Move;
            _stateMachine.GotoNextState = true;
        }

        private void MoveStateinit()
        {
            _pathwalker.SetSpeed(_speed);
        }

        private void MoveStateUpdate()
        {
            if (_pathwalker.CurPoit.Duration == 0 && _pathwalker.HasEnded == false)
                return;
            if (_pathwalker.Progress < 0.9375)
                return;
            _stateMachine.NextState = KoopaBlockState.SlowDown;
            _stateMachine.GotoNextState = true;
        }

        private void SlowDownStateInit()
        {
        }

        private void SlowDownStateUpdate()
        {
            double sin;
            double val;
            double val2;

            sin = -MObjUtil.SinIdx((ushort)(_pathwalker.Progress * 64.0));
            val = sin * _speed;
            _pathwalker.Speed = val > 0.125 ? val : 0.125;
            val2 = _pathwalker.Path.Parts[_pathwalker.PartIdx].OneDivLength;
            if (val <= 0.125)
                val = 0.125;
            _pathwalker.PartSpeed = val * val2;
        }

        private void Update()
        {
            if (_waitCounter > 0)
            {
                _waitCounter--;
                Velocity = Vector3d.Zero;
                /*
                if (!(_waitCounter > 0))
                {
                    mobj_emitSfxIfAudible(&instance->dcolMObj.mobj, SET_KBLOCK_BEGIN_MOVE_SU);
                }
                */
            }
            else
            {
                if (_pathwalker.Update())
                {
                    if (_stateMachine.CurState == KoopaBlockState.SlowDown)
                    {
                        _stateMachine.NextState = KoopaBlockState.SpeedUp;
                        _stateMachine.GotoNextState = true;
                    }
                    _waitCounter = (ushort)_pathwalker.PrevPoit.Duration;
                }
                _lastPosition = Position;
                Position = _pathwalker.CalcCurrentPointLinearXYZ();
                _stateMachine.Execute();
                Velocity = Position - _lastPosition;
                _basePos = -_size.Y * Mtx.Row1 + Position;
            }
        }

        private void Render(byte alpha)
        {
            Mtx.Row3 = Position / 16.0;
            MObjUtil.Model2RenderModel(_context, _model, Mtx, Scale, alpha);
        }

        public class RenderPart : RenderPart<KoopaBlock>
        {
            public Model Model { get; private set; }
            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                var nsbmd = MObjUtil.GetMapObjFile<Nsbmd>(_context, "koopa_block.nsbmd");
                Model = new Model(_context, nsbmd);
                Model.SetPolyIdLightFlagsEmi(63, 1 << 0, new Rgb555(10, 10, 10));
                foreach (var instance in _instances)
                    instance._model = Model;
            }

            protected override void Render(KoopaBlock instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render((byte)alpha);
        }

        public class LogicPart : LogicPart<KoopaBlock>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void Update(KoopaBlock instance)
                => instance.Update();
        }
    }
}

