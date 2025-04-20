using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.Runtime.CompilerServices;

namespace HaroohiePals.NitroKart.MapObj.Common;

[MapObj(MkdsMapObjectId.Itembox, [typeof(ItemboxShadowRenderPart), 
    typeof(ItemboxQuestionRenderPart), typeof(ItemboxBoxRenderPart)], typeof(ItemboxLogicPart))]
public class Itembox : MObjInstance
{
    private const double POSITION_Y_OFFSET = 12;
    private static readonly Vector3d DefaultScale = Vector3d.One; //ibox_sItemBoxScale

    public int BoxFrameCounter { get; private set; }
    public int QuestionFrameCounter { get; private set; }
    public int RespawnCounter { get; private set; }
    public bool IsMoving { get; private set; }
    public bool Setting6 { get; private set; }
    public ItemboxShadowType ShadowType { get; internal set; }
    public ObjectShadow ObjectShadow { get; private set; }
    public int BoxAlpha { get; set; }
    public int QuestionAlpha { get; set; }
    public uint PolygonId { get; internal set; }
    public int PlayerItemSlotListId { get; private set; }
    public int EnemyItemSlotListId { get; private set; }
    public bool IsRuntime { get; private set; }
    public Vector3d RenderPos => Position;
    public Func<uint, Matrix4x3d> BoxAnimFunc { get; private set; }
    public uint BoxAnimFuncArg { get; private set; }

    public Itembox(MkdsContext context, RenderPart[] renderParts, LogicPart logicPart) 
        : base(context, renderParts, logicPart)
    {
        ObjectShadow = new ObjectShadow(context);
    }

    public override void Init(MkdsMapObject obji, object arg)
    {
        Position.Y += POSITION_Y_OFFSET;

        BoxFrameCounter = _context.MObjState.Random.Next(300);
        QuestionFrameCounter = _context.MObjState.Random.Next(130);

        RespawnCounter = 128;

        if (obji is not null)
        {
            IsMoving = ObjectId == MkdsMapObjectId.MoveItembox && obji.Path is not null;
            Setting6 = obji.Settings.Settings[6] != 0;
            IsRuntime = false;
        }
        else
        {
            IsMoving = true;
            Setting6 = false;
            IsRuntime = true;

            Scale = DefaultScale;
        }

        if (IsRuntime)
        {
            PlayerItemSlotListId = 0;
            EnemyItemSlotListId = 0;
        }
        else
        {
            PlayerItemSlotListId = obji.Settings.Settings[1];
            EnemyItemSlotListId = obji.Settings.Settings[2];
        }

        if (IsRuntime)
        {
            ShadowType = ItemboxShadowType.Shadow3d;
        }
        else
        {
            ShadowType = (ItemboxShadowType)obji.Settings.Settings[3];
            if (ShadowType == ItemboxShadowType.Shadow2d)
            {
                var shadowPos = obji.Position;

                if (IsMoving)
                    shadowPos.Y += 0.5;

                if (!ObjectShadow.SetParams(shadowPos, 20.0, 13))
                    ShadowType = ItemboxShadowType.None;
            }
        }

        //if (obji is not null && oshd_isPointOnShadowFloor(&obji->position))
        //    mobj_setMapIconTranslucent(&_mobj);

        //mobj_setVisibilityFlagsBit4(&_mobj);

        // found in renderShadow
        BoxAlpha = 26;
        QuestionAlpha = 20;
    }

    public virtual void Update()
    {
        if (++BoxFrameCounter >= 300)
            BoxFrameCounter = 0;
        if (++QuestionFrameCounter >= 130)
            QuestionFrameCounter = 0;

        int respawnCounter = RespawnCounter;
        if (RespawnCounter < 128)
        {
            RespawnCounter = respawnCounter + 1;
            if (respawnCounter > 96)
                Flags &= ~InstanceFlags.Hidden;
        }
    }
}
