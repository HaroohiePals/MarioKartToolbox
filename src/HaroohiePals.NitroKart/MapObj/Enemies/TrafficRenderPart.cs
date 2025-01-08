using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.MapData;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.MapObj.Enemies;

class TrafficRenderPart : RenderPart<Traffic>
{
    private Model _carModel;
    private Model _carTireModel;
    private Model _truckModel;
    private Model _busModel;
    private ShadowModel _carShadowModel;
    private ShadowModel _truckShadowModel;
    private ShadowModel _busShadowModel;
    private AnimManager _carNsbtpAnim;
    private AnimManager _truckNsbtpAnim;

    private Vector3d _playerIpatDir = Vector3d.UnitX;
    private bool _updateIpatCulling = false;
    private bool _performIpatCulling = false;

    public TrafficRenderPart(MkdsContext context)
        : base(context, RenderPartType.Normal, false, false)
    {
    }

    protected override void GlobalInit()
    {
        _updateIpatCulling = false; // rconf_getRaceMode() != RACE_MODE_MR && !rstat_getUncontrollable() && (u16)rconf_getPlayerDriverId() < 8;
        _performIpatCulling = false;

        _carModel = new Model(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "car_a.nsbmd"));
        _truckModel = new Model(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "truck_a.nsbmd"));
        _busModel = new Model(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "bus_a.nsbmd"));
        _carTireModel = new Model(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "car_tire.nsbmd"));
        _carShadowModel = new ShadowModel(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "car_a_shadow.nsbmd"), 7);
        _truckShadowModel = new ShadowModel(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "truck_a_shadow.nsbmd"), 7);
        _busShadowModel = new ShadowModel(_context, MObjUtil.GetMapObjFile<Nsbmd>(_context, "bus_a_shadow.nsbmd"), 7);

        _carNsbtpAnim = LoadPatternAnimation(_carModel, "car_a.nsbtp");
        _truckNsbtpAnim = LoadPatternAnimation(_truckModel, "truck_a.nsbtp");

        _carModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
        _carModel.SetPolygonId(7);

        _truckModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
        _truckModel.SetPolygonId(7);

        _busModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
        _busModel.SetPolygonId(7);

        _carTireModel.SetPolyIdLightFlagsEmi(63, 1 << 1, new Rgb555(10, 10, 10));
        _carTireModel.SetPolygonId(7);
        _carTireModel.SetLightEnableFlag(0);

        for (int j = 0; j < _instances.Count; j++)
        {
            var instance = _instances[j];

            switch (instance.ObjectId)
            {
                case MkdsMapObjectId.Car:
                    instance.Model = _carModel;
                    instance.ShadowModel = _carShadowModel;
                    instance.NsbtpAnim = _carNsbtpAnim;
                    break;
                case MkdsMapObjectId.Bus:
                    instance.Model = _busModel;
                    instance.ShadowModel = _busShadowModel;
                    break;
                case MkdsMapObjectId.Truck:
                    instance.Model = _truckModel;
                    instance.ShadowModel = _truckShadowModel;
                    instance.NsbtpAnim = _truckNsbtpAnim;
                    break;
            }
        }
    }

    protected override void Render(Traffic instance, in Matrix4x3d camMtx, ushort alpha)
    {
        bool v7 = false;
        if (!v7 && instance.ShadowModel is not null)
        {
            var renderPos = instance.FieldFE > 15 ? instance.Field144 : instance.Position;
            instance.Mtx.Row3 = renderPos / 16.0;
            MObjUtil.Model2RenderShadowModel(instance.ShadowModel, instance.Mtx, instance.Scale, (byte)alpha);
        }

        instance.Mtx.Row3 = instance.Position / 16.0;

        instance.NsbtpAnim?.SetFrame(instance.NsbtpFrame);

        //light_apply(&instance->light);

        MObjUtil.Model2RenderModel(_context, instance.Model, instance.Mtx, instance.Scale, (byte)alpha);

        if (!v7)
        {
            double sinIdx = instance.Field110 * MObjUtil.SinIdx(instance.Field10C);
            double cosIdx = instance.Field110 * MObjUtil.CosIdx(instance.Field10C);
            var tireMtx = Matrix4x3d.CreateRotationX(Math.Atan2(sinIdx, cosIdx));

            tireMtx.Row0.X = instance.Field100;
            tireMtx.Row3.Y = instance.Field104;
            tireMtx.Row3.Z = instance.Field108;

            // this was PushMatrix
            _context.RenderContext.GeState.StoreMatrix(1);
            _context.RenderContext.GeState.MultMatrix(tireMtx);
            _carTireModel.Render();

            // this was PopMatrix
            _context.RenderContext.GeState.RestoreMatrix(1);

            tireMtx.Row3.Z = -tireMtx.Row3.Z;
            _context.RenderContext.GeState.MultMatrix(tireMtx);
            _carTireModel.Render();
        }
    }

    private AnimManager LoadPatternAnimation(Model model, string fileName)
    {
        return MObjUtil.anim_20EA514(_context, model, MObjUtil.GetMapObjFile<Nsbtp>(_context, fileName));
    }
}
