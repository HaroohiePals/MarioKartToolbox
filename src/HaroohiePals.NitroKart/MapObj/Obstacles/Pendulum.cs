using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Obstacles
{
    [MapObj(MkdsMapObjectId.Pendulum, new[] { typeof(Pendulum.RenderPart) }, typeof(Pendulum.LogicPart))]
    public class Pendulum : MObjInstance
    {
        private Quaterniond _rotation;
        private Vector3d    _prevPosition;
        private Vector3d    _renderPos;
        private Matrix4x3d  _shadowMtx;
        private double      _offsetY;
        private ushort      _swingRange;
        private ushort      _swingVelocity;
        private ushort      _angle;

        public Pendulum(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        public override void Init(MkdsMapObject obji, object arg)
        {
            _rotation = MObjUtil.QtrnFromEulerAngles(obji.Rotation);

            _shadowMtx = Mtx;

            _offsetY = Scale.Y * 500;
            _angle   = 0;

            _renderPos = obji.Position;

            Position = -_offsetY * Mtx.Row1 + _renderPos;

            _prevPosition = Position;

            _shadowMtx.Row3 = Position / 16.0;

            _swingVelocity = (ushort)(6553600 / (uint)obji.Settings.Settings[0] / 240);
            _swingRange    = (ushort)(MObjUtil.DegToIdx(24) * (uint)obji.Settings.Settings[1] / 100);

            Size.X = Scale.X * 100;
            Size.Y = Scale.Z * 50;
            Size.Z = 0;
        }

        private void Update()
        {
            _angle += _swingVelocity;
            ushort angle2 = (ushort)(_swingRange * MObjUtil.SinIdx(_angle));

            var mtx4 = Matrix4d.CreateFromQuaternion(MObjUtil.QtrnRotZ(_rotation, angle2));
            Mtx = new Matrix4x3d(mtx4.Row0.Xyz, mtx4.Row1.Xyz, mtx4.Row2.Xyz, Vector3d.Zero);

            Position = -_offsetY * Mtx.Row1 + _renderPos;

            _shadowMtx.Row3 = Position / 16.0;

            Velocity = Position - _prevPosition;

            _prevPosition = Position;
        }

        private void Render(byte alpha)
        {
            Mtx.Row3 = _renderPos / 16.0;
            var renderPart = (RenderPart)_renderParts[0];
            MObjUtil.Model2RenderModel(_context, renderPart.Model, Mtx, Scale, alpha);
            MObjUtil.Model2RenderShadowModel(renderPart.ShadowModel, _shadowMtx, Scale, alpha);
        }

        public class RenderPart : RenderPart<Pendulum>
        {
            public Model       Model       { get; private set; }
            public ShadowModel ShadowModel { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                var nsbmd = MObjUtil.GetMapObjFile<Nsbmd>(_context, "pendulum.nsbmd");
                Model = new Model(_context, nsbmd);
                var shadowNsbmd = MObjUtil.GetMapObjFile<Nsbmd>(_context, "pendulum_shadow.nsbmd");
                ShadowModel = new ShadowModel(_context, shadowNsbmd, 63);
                Model.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
            }

            protected override void Render(Pendulum instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render((byte)alpha);
        }

        public class LogicPart : LogicPart<Pendulum>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void Update(Pendulum instance)
                => instance.Update();
        }
    }
}