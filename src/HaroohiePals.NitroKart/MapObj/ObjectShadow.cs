using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;
using HaroohiePals.NitroKart.Resources;
using OpenTK.Mathematics;
using System.IO;
using System.Runtime.ConstrainedExecution;
using static HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model.ImdPrimitiveElement;

namespace HaroohiePals.NitroKart.MapObj;

public class ObjectShadow
{
    private const double OFFSET_Y = 1.5999;

    // temporary
    private static readonly Nsbmd _objShadowNsbmd;
    private static readonly Nsbmd _jgShadowNsbmd;

    public ushort Alpha;

    private readonly MkdsContext _context;

    private Matrix4x3d _mtx;
    private Model _model;
    private ShadowModel _jgModel;

    // temporary
    static ObjectShadow()
    {
        var imd = new Imd(MapObjPlaceholderModels.ObjShadow);
        _objShadowNsbmd = imd.ToNsbmd("obj_shadow");
        _objShadowNsbmd.TextureSet = imd.ToNsbtx().TextureSet;

        imd = new Imd(MapObjPlaceholderModels.JgShadow);
        _jgShadowNsbmd = imd.ToNsbmd("jg_shadow"); //found in Race.carc
    }

    public ObjectShadow(MkdsContext context)
    {
        _context = context;

        _model = new Model(context, _objShadowNsbmd);
        _model.SetPolygonId(63);
        _model.SetLightEnableFlag(0);

        //if (rconf_getDisplayMode() != RACE_DISPLAY_MODE_STAFF_ROLL)
        _jgModel = new ShadowModel(context, _jgShadowNsbmd, 63);
    }

    public void SetPositionXZ(Vector3d position)
        => _mtx.Row3 = new Vector3d(position.X, _mtx.Row3.Y * 16.0, position.Z) / 16.0;

    public void SetPosition(Vector3d position)
        => _mtx.Row3 = new Vector3d(position.X, position.Y + OFFSET_Y, position.Z) / 16.0;

    public void ApplyMaterial()
    {
        var modelMat = _model.RenderObj.ModelResource.Materials.Materials[0];
        var texInfo = _model.RenderObj.MaterialTextureInfos[0];
        var texture = texInfo.Texture;

        _context.RenderContext.GeState.TexImageParam = texInfo.TexImageParam;
        _context.RenderContext.ApplyTexParams(texture);
        _context.RenderContext.GeState.MaterialColor0 = modelMat.DiffuseAmbient;
        _context.RenderContext.GeState.MaterialColor1 = modelMat.SpecularEmission;
    }

    public void Render(double? scale, Matrix4x3d camMtx, ushort alpha)
    {
        _context.RenderContext.GeState.RestoreMatrix(30);
        _context.RenderContext.GeState.MultMatrix(_mtx);
        if (scale is not null)
            _context.RenderContext.GeState.Scale(new Vector3d(scale.Value));

        var modelMat = _model.RenderObj.ModelResource.Materials.Materials[0];
        var polyAttr = modelMat.PolygonAttribute;

        if (alpha >= Alpha)
            alpha = Alpha;

        polyAttr.Alpha = alpha;
        _context.RenderContext.GeState.PolygonAttr = polyAttr;
        _context.RenderContext.RenderShp(_model.RenderObj.ModelResource.Shapes.Shapes[0], _model.RenderObj.ShapeProxies[0]);
    }

    public void RenderMat(double? scale, Matrix4x3d camMtx, ushort alpha)
    {
        ApplyMaterial();
        Render(scale, camMtx, alpha);
    }

    public void RenderJgShadowTransformed(Vector3d scale, Matrix4x3d mtx, ushort alpha, ushort maxAlpha)
    {
        if (alpha >= maxAlpha)
            alpha = maxAlpha;
        _jgModel.Alpha = (byte)alpha;
        _jgModel.Render(mtx, scale);
    }

    public void RenderJgShadow(ushort alpha, ushort maxAlpha)
    {
        if (alpha >= maxAlpha)
            alpha = maxAlpha;
        _jgModel.Alpha = (byte)alpha;
        _jgModel.Render();
    }

    public bool SetParams(Vector3d position, double scale, ushort alpha)
    {
        bool collided;
        double scaleDiv10;
        Vector3d floorNormal = Vector3d.UnitY;

        collided = false;
        position.Y += OFFSET_Y;

        // todo
        //if (col_collide(&position, 0, 0, FX32_CONST(10), COL_COLLIDE_FLAGS_IS_MAPOBJ, -2,
        //                NULL, &floorNormal, NULL, NULL, NULL, NULL, NULL, NULL))
        //{
        collided = true;
        var quaternion = MObjUtil.QtrnFromForwardVec(floorNormal);

        var mtx4 = Matrix4d.CreateFromQuaternion(quaternion);
        _mtx = new Matrix4x3d(mtx4.Row0.Xyz, mtx4.Row1.Xyz, mtx4.Row2.Xyz, Vector3d.Zero);

        scaleDiv10 = scale / 10;
        _mtx.Row0 *= scaleDiv10;
        _mtx.Row1 *= scaleDiv10;
        _mtx.Row2 *= scaleDiv10;
        //}
        //else
        //{
        //    _mtx.Row0 = Matrix3d.Identity.Row0;
        //    _mtx.Row1 = Matrix3d.Identity.Row1;
        //    _mtx.Row2 = Matrix3d.Identity.Row2;
        //}

        _mtx.Row3 = position / 16.0;
        Alpha = alpha;
        return collided;
    }

    public bool IsPointOnShadowFloor(Vector3d position)
    {
        //u32 outColFlags = 0;
        //col_collide(position, 0, 0, FX32_CONST(10), COL_COLLIDE_FLAGS_IS_MAPOBJ, -2,
        //            NULL, NULL, NULL, &outColFlags, NULL, NULL, NULL, NULL);
        //return outColFlags & COL_FLAGS_MAP2D_SHADOW;
        return false;
    }
}
