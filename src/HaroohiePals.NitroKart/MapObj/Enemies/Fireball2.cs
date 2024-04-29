using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Enemies
{
    [MapObj(MkdsMapObjectId.Fireball2, new[] { typeof(Fireball2.RenderPart) }, typeof(Fireball2.LogicPart))]
    public class Fireball2 : MObjInstance
    {
        private class Fireball
        {
            public Vector3d Position;
            public ushort   ArmRotZ;
            public ushort   BallRotZ;
        }

        private ushort _nrArms;
        private ushort _fireballsPerArm;
        private ushort _armAngleDistance;
        private double _fireballDistance;
        private double _radius;
        private ushort _rotSpeed;
        private ushort _rotation;

        private readonly Fireball    _centerFireball = new();
        private readonly Fireball[,] _fireballs      = new Fireball[20, 20];

        public Fireball2(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            var settings = (Fireball2Settings)ObjiEntry.Settings;

            Flags |= InstanceFlags.Bit9;

            _nrArms          = (ushort)settings.NrArms;
            _fireballsPerArm = (ushort)settings.FireballsPerArm;

            Flags &= ~InstanceFlags.Hidden;
            Flags &= ~InstanceFlags.DisableVisibilityUpdates;
            Flags &= ~InstanceFlags.Bit3;

            Alpha = 31;
        }

        public void Update()
        {
            double r = _radius;
            _centerFireball.Position = Position;
            if (_fireballsPerArm > 1)
                _centerFireball.BallRotZ += 1310;
            for (int i = 0; i < _fireballsPerArm; i++)
            {
                ushort rot = _rotation;
                for (int j = 0; j < _nrArms; j++)
                {
                    rot                      += _armAngleDistance;
                    _fireballs[j, i].ArmRotZ =  rot;
                    _fireballs[j, i].Position = Position +
                                                r * MObjUtil.CosIdx(rot) * Mtx.Row0 +
                                                r * MObjUtil.SinIdx(rot) * Mtx.Row1;
                    _fireballs[j, i].BallRotZ += 1310;
                }

                r -= _fireballDistance;
            }

            _rotation -= _rotSpeed;
        }

        private void RenderFireball(Fireball fireball, in Matrix4x3d camMtx)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetBillboardAtPos(fireball.Position, camMtx));
            if (fireball.BallRotZ != 0)
            {
                _context.RenderContext.GeState.MultMatrix(
                    Matrix3d.CreateRotationZ(MObjUtil.IdxToRad(fireball.BallRotZ)));
            }

            _context.RenderContext.GeState.Scale(Scale);
            var renderPart = (RenderPart)_renderParts[0];
            renderPart.Model.SetAllPolygonIds(_context.MObjState.GetCyclicPolygonId());
            renderPart.Model.Render(this);
        }

        private void Render(in Matrix4x3d camMtx)
        {
            double dot = Vector3d.Dot(Mtx.Row0, camMtx.Row2);
            for (int i = 0; i < _nrArms; i++)
            {
                ushort armRotZ = _fireballs[i, 0].ArmRotZ;
                if ((dot >= 0) ^ (armRotZ >= 0x4000 && armRotZ < 0xC000))
                {
                    for (int j = 0; j < _fireballsPerArm; j++)
                        RenderFireball(_fireballs[i, j], camMtx);
                }
            }

            if (_fireballsPerArm > 1)
                RenderFireball(_centerFireball, camMtx);

            for (int i = 0; i < _nrArms; i++)
            {
                ushort armRotZ = _fireballs[i, 0].ArmRotZ;
                if (!((dot >= 0) ^ (armRotZ >= 0x4000 && armRotZ < 0xC000)))
                {
                    for (int j = _fireballsPerArm - 1; j >= 0; j--)
                        RenderFireball(_fireballs[i, j], camMtx);
                }
            }
        }

        public class RenderPart : RenderPart<Fireball2>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Billboard, false, true) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadBillboardModel(_context, this, "fireball2.nsbmd");
                Model.SetAllPolygonIds(63);
            }

            protected override void GlobalPreRender()
            {
                Model.BbModel.ApplyMaterial();
            }

            protected override void Render(Fireball2 instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx);
        }

        public class LogicPart : LogicPart<Fireball2>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                var rand = _context.MObjState.Random;
                foreach (var instance in _instances)
                {
                    try
                    {
                        var settings = (Fireball2Settings)instance.ObjiEntry.Settings;

                        instance._radius = settings.Radius;
                        instance._rotSpeed = (ushort)(364 * settings.RotationSpeed / 100);
                        instance._armAngleDistance = (ushort)(0x10000 / instance._nrArms);
                        instance._fireballDistance = instance._radius / instance._fireballsPerArm;
                        instance._centerFireball.BallRotZ = (ushort)rand.Next(0x10000);
                        for (int i = 0; i < instance._fireballsPerArm; i++)
                        {
                            for (int j = 0; j < instance._nrArms; j++)
                            {
                                instance._fireballs[j, i] = new Fireball();
                                instance._fireballs[j, i].BallRotZ = (ushort)rand.Next(0x10000);
                            }
                        }
                    }
                    catch { }
                }
            }

            protected override void Update(Fireball2 instance)
                => instance.Update();
        }
    }
}