using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj.Common;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HaroohiePals.NitroKart.MapObj.Enemies;

[MapObj(MkdsMapObjectId.Car, [typeof(TrafficRenderPart)], typeof(TrafficLogicPart))]
[MapObj(MkdsMapObjectId.Bus, [typeof(TrafficRenderPart)], typeof(TrafficLogicPart))]
[MapObj(MkdsMapObjectId.Truck, [typeof(TrafficRenderPart)], typeof(TrafficLogicPart))]
public class Traffic : MObjInstance
{
    /// <summary>
    /// Initial Position Y
    /// </summary>
    internal double FieldA0 { get; private set; }
    internal Quaterniond FieldA4 { get; private set; }
    internal Quaterniond FieldB4 { get; private set; }
    internal Quaterniond FieldC4 { get; private set; }
    internal int FieldD4 { get; private set; }
    internal Pathwalker Pathwalker { get; private set; }
    internal short InitialPoint { get; private set; }
    internal short FieldFE { get; private set; }
    internal double Field100 { get; private set; }
    internal double Field104 { get; private set; }
    internal double Field108 { get; private set; }
    internal ushort Field10C { get; private set; }
    internal double Field110 { get; private set; }
    internal double Field114 { get; private set; }
    internal double Field118 { get; private set; }
    internal double Field11C { get; private set; }
    internal double Field120 { get; private set; }
    internal Model Model { get; set; }
    internal ShadowModel ShadowModel { get; set; }
    internal AnimManager NsbtpAnim { get; set; }
    internal ushort NsbtpFrame { get; private set; }
    //internal light_t Light { get; private set; }
    internal Vector3d Field144 { get; private set; }
    internal ushort Field150 { get; private set; }
    internal ushort Field152 { get; private set; }
    internal TrafficParams Params { get; private set; }
    //internal sfx_emitter_ex_params_t* SfxEmitterExParams { get; private set; }
    internal int Field15C { get; private set; }
    internal int Field160 { get; private set; }

    public Traffic(MkdsContext context, RenderPart[] renderParts, LogicPart logicPart)
        : base(context, renderParts, logicPart)
    {
    }

    public override void Init(MkdsMapObject obji, object arg)
    {
        switch (obji.ObjectId)
        {
            case MkdsMapObjectId.Car:
                Params = TrafficParams.Car;
                break;
            case MkdsMapObjectId.Truck:
                Params = TrafficParams.Truck;
                break;
            case MkdsMapObjectId.Bus:
                Params = TrafficParams.Bus;
                break;
        }

        Pathwalker = Pathwalker.FromPath(obji.Path.Target, 3 * obji.Settings.Settings[0] / 100);

        InitialPoint = obji.Settings.Settings[1];
        NsbtpFrame = (ushort)obji.Settings.Settings[2];
        FieldA0 = obji.Path.Target.Points[InitialPoint].Position.Y;

        Field114 = Params.Field0 * Scale.X;
        Field118 = Params.Field4 * Scale.Y;
        Field11C = Params.Field8 * Scale.Z;
        Field120 = Params.FieldC * Scale.Z;
        Field152 = (ushort)(250 * obji.Settings.Settings[0] / 100);
        Field150 = Field152;
        Field100 = Params.Field10;
        Field104 = Params.Field14 / 16.0;
        Field108 = Params.Field18 / 16.0;
        Field110 = Params.Field1C;

        Traf20E1E98();

        Field15C = Params.Field2C;

        //SfxEmitterExParams = mobj_initSoundEmitterExForObjectSfxParam2(ref Mobj);
        //sfx_210B850(SfxEmitterExParams);
        //SoundEmitter.UpdateFunc = sub_210A6EC;

        Field160 = Params.Field30;
    }

    public void Update()
    {
        var v29 = Vector3d.Zero;
        var v3 = Quaterniond.Identity;
        var a2 = Quaterniond.Identity;

        bool v2 = FieldFE > 15;

        var prevPos = Position;
        Pathwalker.Update();
        if (!v2)
        {
            Position = Pathwalker.CalcCurrentPointXZLinearY();
            Velocity = Position - prevPos;
        }
        else
        {
            Field144 = Pathwalker.CalcCurrentPointXZLinearY();
            Position.X = Field144.X;
            Velocity.Y -= 0.35;
            Position.Y += Velocity.Y;
            Position.Z = Field144.Z;
            Velocity.X = Position.X - prevPos.X;
            Velocity.Z = Position.Z - prevPos.Z;
        }
        if (v2 /*|| race_getFrameCounterModulo8() == (FieldD4 & 7)*/)
        {
            v29.X = Position.X;
            v29.Y = Position.Y + (v2 ? 15.0 : 0);
            v29.Z = Position.Z;
            //var position = v29;
            //if (col_collide(&position, 0, 0, FX32_CONST(15), COL_COLLIDE_FLAGS_IS_MAPOBJ, -2, &pushback, &position, 0,
            //                &outColFlags, 0, 0, 0, 0))
            //{
            //    if (outColFlags & 0x1E34EF)
            //    {
            //        qtrn_fromForwardVec(&position, &FieldB4);
            //        light_interpolateToKclColor(
            //            &instance->light, (u16)((outColFlags & COL_FLAGS_LIGHT_MASK) >> COL_FLAGS_LIGHT_SHIFT), 20480);
            //        if (FieldFE && Velocity.Y < 0)
            //        {
            //            if (flags & MOBJ_INST_FLAGS_HIDDEN)
            //            {
            //                mobj_unhide(&Mobj);
            //                Velocity.Y = -FX_TRUNC_MUL(Velocity.Y
            //                                                          , ((Field118 - FX32_CONST(20)) >> 9) +
            //                                                          FX32_CONST(0.45));
            //                FieldC4.X = -FieldC4.X;
            //                FieldC4.Y = -FieldC4.Y;
            //                FieldC4.Z = -FieldC4.Z;
            //                qtrn_slerp(&FieldC4, &FieldB4, &FieldC4, FX32_CONST(0.55));
            //                Field152 = 800;
            //                mobj_emitSfxFromEmitter2(&Mobj, Field160);
            //            }
            //            else if (v2 && instance)
            //            {
            //                FieldFE = 15;
            //                Velocity.Y = 0;
            //                Field152 = 600;
            //            }
            //        }
            //    }
            //    if (outColFlags & 0x214300 && v2)
            //    {
            //        Position.Y += pushback.Y;
            //        Velocity.Y = 0;
            //    }
            //}
        }
        if (v2)
        {
            if (Velocity.Y < 0)
            {
                Field152 = (ushort)(0.003 * -Velocity.Y);
                v3 = FieldB4;
            }
            else
                v3 = FieldC4;
        }
        else if (FieldFE != 0)
        {
            if (--FieldFE != 0)
                v3 = FieldB4;
            else
                Field152 = Field150;
        }
        if (Velocity.X * Velocity.X + Velocity.Z * Velocity.Z < 0.004)
        {
            a2 = FieldB4;
        }
        else
        {
            RotY = MObjUtil.VecToYIdxAngle(Velocity);
            if (FieldFE == 0)
                v3 = FieldB4;

            a2 = MObjUtil.Qtrn20D79B0(v3, RotY);
        }

        FieldA4 = Quaterniond.Slerp(FieldA4, a2, (double)(Field152 / 4096.0));

        var mtx4 = Matrix4d.CreateFromQuaternion(FieldA4);
        Mtx = new Matrix4x3d(mtx4.Row0.Xyz, mtx4.Row1.Xyz, mtx4.Row2.Xyz, Vector3d.Zero);

        Field10C += (ushort)(0.375 * 4096);
        //if (VisibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_WAS_UPDATED)
        //    sub_210B7EC(instance->sfxEmitterExParams,
        //                VisibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_DISTANCE_MASK);
        //mobj_emitSfxFromEmitter(&Mobj, Field15C);
        if (Position.Y < 0)
            Traf20E1E98();
    }

    private void Traf20E1E98()
    {
        Pathwalker.Init(InitialPoint, true);
        Pathwalker.pw_20D8B18_XZ(out Position, out Velocity);

        Position.Y = FieldA0;
        Velocity.Y = 0;

        FieldB4 = Quaterniond.Identity;
        FieldA4 = MObjUtil.Qtrn20D79B0(FieldB4, MObjUtil.VecToYIdxAngle(Velocity));
        Mtx = MObjUtil.EulerAnglesToMtx43(FieldA4.ToEulerAngles());

        FieldFE = 0;
        Field152 = Field150;
        Field10C = 0;

        //light_init(&instance->light, 1);
    }
}
