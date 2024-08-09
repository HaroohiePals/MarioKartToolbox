using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Obstacles;

class MoveItemboxLogicPart : LogicPart<MoveItembox>
{
    private ushort _oscillationAngle;
    private double _offsetY;
    private double _shadow2dScale;
    private Vector3d _shadow3dScale;

    public MoveItemboxLogicPart(MkdsContext context)
        : base(context, LogicPartType.Type0)
    {
    }

    protected override void GlobalInit()
    {
        _oscillationAngle = 0;
        _offsetY = 0;

        _shadow2dScale = 1.0;
        _shadow3dScale = Vector3d.One;
    }

    protected override void GlobalPreUpdate()
    {
        double sin = MObjUtil.SinIdx(_oscillationAngle);
        _offsetY = sin * 4.0;
        _oscillationAngle += MObjUtil.DegToIdx(4.0);

        _shadow2dScale = 0.9375 - sin; //FX32_CONST(0.9375) - (sin >> 4)
        _shadow3dScale = new Vector3d(1.4);
    }

    protected override void Update(MoveItembox instance) 
        => instance.Update(_offsetY);
}
