using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj.Common;

namespace HaroohiePals.NitroKart.MapObj.Obstacles;

[MapObj(MkdsMapObjectId.MoveItembox, [typeof(ItemboxShadowRenderPart),
    typeof(ItemboxQuestionRenderPart), typeof(ItemboxBoxRenderPart)], typeof(MoveItemboxLogicPart))]
public class MoveItembox : Itembox
{
    private const double MOVEMENT_SPEED = 3.0;

    private PathwalkerEx _pathwalker;

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

    public void Update(double offsetY)
    {
        if (_pathwalker is not null)
        {
            if (_pathwalker.Update() && _pathwalker.PrevPoit.Unknown2 != 0)
                _pathwalker.Init(0, true);

            Position = _pathwalker.CalcCurrentPointXYZ();

            Position.Y += offsetY;
            Position.Y += 16.0;
        }

        Update();
    }
}
