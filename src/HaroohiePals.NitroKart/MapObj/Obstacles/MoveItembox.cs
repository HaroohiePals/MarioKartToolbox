using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj.Common;

namespace HaroohiePals.NitroKart.MapObj.Obstacles;

[MapObj(MkdsMapObjectId.MoveItembox, [typeof(ItemboxShadowRenderPart),
    typeof(ItemboxQuestionRenderPart), typeof(ItemboxBoxRenderPart)], typeof(MoveItemboxLogicPart))]
public class MoveItembox : Itembox
{
    private Pathwalker _pathwalker;

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
            //pwex_initFromObject(&instance->pathwalker.pathwalker, obji, dword_2158908, obji->settings[0]);
            //pwex_init(&instance->pathwalker, (u16)obji->settings[4], TRUE);

            _pathwalker = Pathwalker.FromPath(obji.Path.Target, 3.0);
            _pathwalker.Init(obji.Settings.Settings[4], true);

            Position = _pathwalker.CalcCurrentPointXYZ();
        }
    }

    public void Update(double offsetY)
    {
        if (_pathwalker is not null)
        {
            //if (pwex_update(&instance->pathwalker) && instance->pathwalker.pathwalker.prevPoit->unknown2_s16)
            //    pwex_init(&instance->pathwalker, 0, 1);
            _pathwalker.Update();

            Position = _pathwalker.CalcCurrentPointXYZ();

            //if (instance->pathwalker.pathwalker.prevPoit->unknown1 && instance->pathwalker.pathwalker.curPoit->unknown1)
            //    mobj_setMapIconTranslucent(&instance->itembox.mobj);
            //else
            //    mobj_resetMapIconTranslucent(&instance->itembox.mobj);

            Position.Y += offsetY;
            Position.Y += 16.0;
        }

        //if (instance->itembox.shadowType == IBOX_SHADOW_TYPE_2D)
        //    oshd_setPositionXZ(&instance->itembox.objShadow, &instance->itembox.mobj.position);

        Update();
    }
}
