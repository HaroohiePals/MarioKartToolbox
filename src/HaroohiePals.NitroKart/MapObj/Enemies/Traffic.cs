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
        //Mobj.SoundEmitter.UpdateFunc = sub_210A6EC;

        Field160 = Params.Field30;
    }

    private void Traf20E1E98()
    {
        Pathwalker.Init(InitialPoint, true);
        Pathwalker.pw_20D8B18_XZ(out Position, out Velocity);

        Position.Y = FieldA0;
        Velocity.Y = 0;

        FieldB4 = new Quaterniond(0.0, 0.0, 0.0, 1.0);
        FieldA4 = MObjUtil.Qtrn20D79B0(FieldB4, MObjUtil.VecToYIdxAngle(Velocity));
        Mtx = MObjUtil.EulerAnglesToMtx43(FieldA4.ToEulerAngles());

        FieldFE = 0;
        Field152 = Field150;
        Field10C = 0;

        //light_init(&instance->light, 1);
    }
}
