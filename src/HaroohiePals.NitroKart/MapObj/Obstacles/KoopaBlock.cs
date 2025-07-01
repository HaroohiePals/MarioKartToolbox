using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.MapObj.Obstacles;

[MapObj(MkdsMapObjectId.KoopaBlock, new[] { typeof(KoopaBlockRenderPart) }, typeof(KoopaBlockLogicPart))]
public sealed class KoopaBlock : DColMObjInstance
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

    internal Model Model { get; set; }

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
        _size.Y = (Scale.Y * 100) / 2.0;
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
    }

    private void SpeedUpStateInit()
    {
    }

    private void SpeedUpStateUpdate()
    {
        double sin = MObjUtil.SinIdx((ushort)(_pathwalker.Progress * 262144.0));
        double val = sin * _speed;
        val = Math.Max(0.125, val);
        _pathwalker.Speed = val;
        double val2 = _pathwalker.Path.Parts[_pathwalker.PartIdx].OneDivLength;
        _pathwalker.PartSpeed = val * val2;
        if (_pathwalker.Progress >= 0.0625)
        {
            _stateMachine.NextState = KoopaBlockState.Move;
            _stateMachine.GotoNextState = true;
        }
    }

    private void MoveStateinit()
    {
        double speed = _speed;
        _pathwalker.Speed = speed;
        _pathwalker.PartSpeed = speed * _pathwalker.Path.Parts[_pathwalker.PartIdx].OneDivLength;
    }

    private void MoveStateUpdate()
    {
        if (_pathwalker.CurPoit.Duration == 0 && !_pathwalker.HasEnded)
            return;
        if (_pathwalker.Progress >= 0.9375)
        {
            _stateMachine.NextState = KoopaBlockState.SlowDown;
            _stateMachine.GotoNextState = true;
        }
    }

    private void SlowDownStateInit()
    {
    }

    private void SlowDownStateUpdate()
    {
        double sin = -MObjUtil.SinIdx((ushort)(_pathwalker.Progress * 262144.0));
        double val = sin * _speed;
        val = Math.Max(0.125, val);
        _pathwalker.Speed = val;
        _pathwalker.PartSpeed = val * _pathwalker.Path.Parts[_pathwalker.PartIdx].OneDivLength;
    }

    public void Update()
    {
        if (_waitCounter != 0)
        {
            _waitCounter--;
            Velocity = Vector3d.Zero;
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
            _basePos = Position - _size.Y * Mtx.Row1;
        }
    }
}

