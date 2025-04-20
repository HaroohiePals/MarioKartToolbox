using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj.Common;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Obstacles;

[MapObj(MkdsMapObjectId.MoveItembox, [typeof(ItemboxShadowRenderPart),
    typeof(ItemboxQuestionRenderPart), typeof(ItemboxBoxRenderPart)], typeof(ItemboxLogicPart))]
public class MoveItembox : Itembox
{
    private const double MOVEMENT_SPEED = 3.0;

    private PathwalkerEx _pathwalker;

    public ushort OscillationAngle { get; private set; } = 0;
    public double OffsetY { get; private set; } = 0.0;
    public double Shadow2dScale { get; private set; } = 1.0;
    public Vector3d Shadow3dScale { get; private set; } = Vector3d.One;

    public MoveItembox(MkdsContext context, RenderPart[] renderParts, LogicPart logicPart) 
        : base(context, renderParts, logicPart)
    {
    }

    public override void Init(MkdsMapObject obji, object arg)
    {
        base.Init(obji, arg);

        if (IsRuntime)
        {
            //_pathwalker.Path = null;
        }
        else
        {
            _pathwalker = new PathwalkerEx(obji.Path.Target, MOVEMENT_SPEED, obji.Settings.Settings[0]);
            _pathwalker.Init(obji.Settings.Settings[4], true);

            Position = _pathwalker.CalcCurrentPointXYZ();
        }
    }

    public override void Update()
    {
        // This was in MoveItemboxLogicPart GlobalPreUpdate
        double sin = MObjUtil.SinIdx(OscillationAngle);
        OffsetY = sin * 4.0;
        OscillationAngle += MObjUtil.DegToIdx(4.0);

        Shadow2dScale = 0.9375 - (sin / 16.0);
        Shadow3dScale = new Vector3d(1.4);

        if (_pathwalker is not null)
        {
            if (_pathwalker.Update() && _pathwalker.PrevPoit.Unknown2 != 0)
                _pathwalker.Init(0, true);

            Position = _pathwalker.CalcCurrentPointXYZ();

            Position.Y += OffsetY;
            Position.Y += 16.0;
        }

        if (ShadowType == ItemboxShadowType.Shadow2d)
            ObjectShadow.SetPositionXZ(Position);

        base.Update();
    }
}
