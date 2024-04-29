using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Enemies
{
    [MapObj(MkdsMapObjectId.IronBall, new[] { typeof(IronBall.RenderPart) }, typeof(IronBall.LogicPart))]
    [MapObj(MkdsMapObjectId.IronBallNocol, new[] { typeof(IronBall.RenderPart) })]
    public class IronBall : MObjInstance
    {
        private const double GrpConfSizeX = 15;

        private enum IballState
        {
            ActivateWait,
            Wait,
            Roll
        }

        private int _rotZ;
        private int _waitCounter;
        private int _simpleActivateWaitCounter;
        private int _lapTarget;
        private double _speed;
        private double _fastSpeed;
        private double _slowSpeed;
        private double _elevation;
        private double _elevationVelocity;

        private Pathwalker _pathwalker;

        // objshadow_t     shadow;
        private Vector3d _routePos;

        // int        clipAreaMask;
        private int _field12C;
        private IballState _state;

        public IronBall(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            Scale = new(4);
            Size.X = GrpConfSizeX * Scale.X;
        }

        private void Update()
        {
            var settings = (IronBallSettings)ObjiEntry.Settings;

            if (_simpleActivateWaitCounter > 0)
                _simpleActivateWaitCounter--;
            switch (_state)
            {
                case IballState.ActivateWait:
                    if ((Flags & InstanceFlags.UseSimpleHitResponse) != 0)
                    {
                        if (_simpleActivateWaitCounter-- <= 0)
                        {
                            _simpleActivateWaitCounter = settings.MinTimeUntilRespawn;
                            _waitCounter = settings.SpawnDelay;
                            _pathwalker.Init(0, true);
                            _state = IballState.Wait;
                        }
                    }
                    else if (_lapTarget <= 3) //rstat_signedHasAnyDriverReachedLap(fieldAC))
                    {
                        _lapTarget++;
                        _waitCounter = settings.SpawnDelay;
                        _pathwalker.Init(0, true);
                        _state = IballState.Wait;
                    }

                    break;
                case IballState.Wait:
                    if (--_waitCounter <= 0)
                    {
                        Flags &= ~InstanceFlags.Hidden;
                        Flags &= ~InstanceFlags.DisableVisibilityUpdates;
                        Flags &= ~InstanceFlags.Bit3;
                        Alpha = 31;
                        _state = IballState.Roll;
                    }

                    break;
                case IballState.Roll:
                    // if ((clipAreaMask & driver_getClipAreaMask()) == 0
                    //     && !(mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_INAUDIBLE))
                    //     sub_210B874(mobj.soundEmitter, fieldB0);
                    _pathwalker.SetSpeed(_speed);
                    if (_pathwalker.Update())
                    {
                        var prevPoit = _pathwalker.PrevPoit;
                        short nextPoint = (short)(prevPoit.Unknown2 & 0xFFFF);
                        if (!_pathwalker.IsForwards)
                        {
                            if (nextPoint >= 0)
                            {
                                _pathwalker.Init(nextPoint, true);
                                prevPoit = _pathwalker.PrevPoit;
                            }
                            else if (settings.MinTimeUntilRespawn != 0)
                            {
                                _pathwalker.Init(0, true);
                                Flags |= InstanceFlags.Hidden;
                                Flags |= InstanceFlags.DisableVisibilityUpdates;
                                Flags |= InstanceFlags.Bit3;
                                _state = IballState.ActivateWait;
                                return;
                            }
                            else
                            {
                                SetInvisible();
                                return;
                            }
                        }
                        else if (nextPoint >= 0 && _context.MObjState.Random.Next(5) != 0)
                        {
                            _pathwalker.Init(nextPoint, true);
                            prevPoit = _pathwalker.PrevPoit;
                        }

                        if (prevPoit.Duration == 0 || prevPoit.Duration == 1)
                        {
                            _speed = _fastSpeed;
                            _field12C = 0;
                        }
                        else if (prevPoit.Duration == 2 || prevPoit.Duration == 3)
                        {
                            _speed = _slowSpeed;
                            _field12C = 2;
                        }
                        else if (prevPoit.Duration == 10)
                        {
                            _speed = 8;
                            _field12C = 2;
                            _elevation = _routePos.Y;
                            _pathwalker.Init(_pathwalker.ResPath.Points.IndexOf(prevPoit) + 1, true);
                            _pathwalker.CalcCurrentPointLinearXYZSpecial(out _routePos, out Velocity);
                            _routePos.Y += 1;
                            _elevation -= _routePos.Y;
                            _elevationVelocity = 10;
                        }
                        else if (prevPoit.Duration == 11)
                        {
                            _speed = 5;
                            _field12C = 2;
                            _elevation = _routePos.Y;
                            _pathwalker.Init(_pathwalker.ResPath.Points.IndexOf(prevPoit) + 1, true);
                            _pathwalker.CalcCurrentPointLinearXYZSpecial(out _routePos, out Velocity);
                            _routePos.Y += 1;
                            _elevation -= _routePos.Y;
                            _elevationVelocity = -3;
                        }
                        else if (prevPoit.Duration >= 100)
                        {
                            _routePos = prevPoit.Position;
                            Velocity = Vector3d.Zero;
                            if (prevPoit.Duration < 200)
                            {
                                // bnd_handleBallHit(prevPoit->duration - 100);
                                _speed = 15;
                                _field12C = 1;
                            }
                            else if (prevPoit.Duration < 300)
                            {
                                int tmp = prevPoit.Duration - 200;
                                if (prevPoit.Duration != _pathwalker.CurPoit.Duration)
                                    _speed = 30;
                                // else
                                // flip_handleBallHit(tmp);
                                _field12C = 1;
                            }
                        }
                    }

                    // if (pathwalker.PrevPoit.Unknown1 != 0 && pathwalker.CurPoit.Unknown1 != 0)
                    //     mobj_setMapIconTranslucent(&mobj);
                    // else
                    //     mobj_resetMapIconTranslucent(&mobj);
                    if (_elevation > 0 || _elevationVelocity != 0)
                    {
                        _elevationVelocity -= 1;
                        _elevation += _elevationVelocity;
                        if (_elevation <= 0)
                        {
                            _elevationVelocity *= -0.5;
                            if (_elevationVelocity < 1)
                                _elevationVelocity = 0;
                            // else if ((clipAreaMask & driver_getClipAreaMask()) == 0
                            //          && !(mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_INAUDIBLE))
                            //     sub_210B8BC((driver_t*)mobj.soundEmitter, fieldC0);
                            _elevation = _elevationVelocity;
                        }
                    }

                    if (_pathwalker.PrevPoit.Duration >= 100)
                    {
                        _pathwalker.CalcCurrentPointLinearXYZSpecial(out _routePos, out Velocity);
                        _routePos.Y += 1;
                        _speed = System.Math.Max(_speed - 0.1, 10);
                    }
                    else if (_pathwalker.PrevPoit.Duration == 10 || _pathwalker.PrevPoit.Duration == 11)
                    {
                        _pathwalker.CalcCurrentPointLinearXYZSpecial(out _routePos, out Velocity);
                        _routePos.Y += 1;
                    }
                    else if (_pathwalker.PrevPoit.Duration == 0 || _pathwalker.PrevPoit.Duration == 2)
                    {
                        _pathwalker.pw_20D8BF8_XYZ(out _routePos, out Velocity);
                        _routePos.Y += 1;
                    }
                    else
                    {
                        _pathwalker.CalcCurrentPointLinearXYZSpecial(out _routePos, out Velocity);
                        _routePos.Y += 1;
                    }

                    Position = _routePos + Mtx.Row1 * Size.X;
                    Position.Y += _elevation;
                    int newRotZ;
                    if (_rotZ < 0)
                    {
                        newRotZ = -1;
                        if (_rotZ == -1)
                            newRotZ = 910;
                    }
                    else if (_rotZ != 1)
                        newRotZ = 1;
                    else
                        newRotZ = -910;

                    _rotZ = newRotZ;
                    if (_field12C == 2)
                    {
                        // oshd_setParams(&shadow, &routePos, 2 * mobj.size.x, 28);
                        _routePos.Y += 1;
                        _field12C = 1;
                    }

                    if (_field12C != 0)
                    {
                        // if ((int)(mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_DISTANCE_MASK) < 1000000)
                        {
                            int v20 = (int)(_elevation / 4) - 7;
                            int shadowAlpha;
                            if (v20 < 0)
                                shadowAlpha = 28;
                            else if (v20 < 28)
                                shadowAlpha = 28 - v20;
                            else
                                shadowAlpha = 0;
                            // shadow.alpha = shadowAlpha;
                            // oshd_setPosition(&shadow, &routePos);
                        }

                        Flags &= ~InstanceFlags.Hidden;
                    }
                    else
                        Flags |= InstanceFlags.Hidden;

                    break;
            }
        }

        private void Render(in Matrix4x3d camMtx)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetBillboardAtPos(Position, camMtx));
            if (_rotZ != 0)
                _context.RenderContext.GeState.MultMatrix(Matrix4x3d.CreateRotationZ(MObjUtil.IdxToRad((ushort)_rotZ)));
            _context.RenderContext.GeState.Scale(Scale);
            ((RenderPart)_renderParts[0]).Model.Render(this);
        }

        public class RenderPart : RenderPart<IronBall>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Billboard) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadBillboardModel(_context, this, "IronBall.nsbmd");
                Model.SetAllPolygonIds(63);
            }

            protected override void GlobalPreRender()
            {
                Model.BbModel.ApplyMaterial();
            }

            protected override void Render(IronBall instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx);
        }

        public class LogicPart : LogicPart<IronBall>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                foreach (var instance in _instances)
                {
                    try
                    {
                        var settings = (IronBallSettings)instance.ObjiEntry.Settings;
                        instance.Flags |= InstanceFlags.Hidden;
                        instance.Flags |= InstanceFlags.DisableVisibilityUpdates;
                        instance.Flags |= InstanceFlags.Bit3;
                        instance._fastSpeed = 10 * settings.Speed / 100.0;
                        instance._slowSpeed = 5 * settings.Speed / 100.0;
                        instance._speed = instance._fastSpeed;
                        instance.Position = instance._routePos = instance.ObjiEntry.Position;
                        instance._routePos.Y += 1;
                        // oshd_setParams(&instance->shadow, &instance->routePos, 2 * instance->mobj.size.x, 28);
                        // mobj_setDirectionFromFloor(&instance->mobj);
                        instance._pathwalker = Pathwalker.FromPath(instance.ObjiEntry.Path.Target, 0);
                        instance._pathwalker.CalcCurrentPointLinearXYZSpecial(out instance._routePos,
                            out instance.Velocity);
                        instance._routePos.Y += 1;
                        instance.Position = instance._routePos + instance.Mtx.Row1 * instance.Size.X;
                        if ((instance.Flags & InstanceFlags.UseSimpleHitResponse) != 0)
                        {
                            instance._simpleActivateWaitCounter = settings.InitialActivationDelay;
                            instance._state = IballState.ActivateWait;
                        }
                        else
                        {
                            instance._lapTarget = (settings.InitialActivationDelay + 5999) / 6000 + 1;
                            instance._state = IballState.ActivateWait;
                        }
                    }
                    catch { }
                }
            }

            protected override void Update(IronBall instance)
                => instance.Update();
        }
    }
}