using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj;

public static class MObjUtil
{
    //public static byte[] GetMapObjFile(MkdsContext context, string relPath)
    //    => context.Course.MainArchive.GetFileData($"MapObj/{relPath}");

    public static T GetMapObjFile<T>(MkdsContext context, string relPath)
        => context.Course.GetMainFileOrDefault<T>($"MapObj/{relPath}");

    public static bool ExistsMapObjFile(MkdsContext context, string relPath) 
        => ExistsMapObjFile(context.Course, relPath);

    public static bool ExistsMapObjFile(IMkdsCourse course, string relPath)
        => course.ExistsMainFile($"MapObj/{relPath}");

    public static MObjModel LoadModel(MkdsContext context, RenderPart renderPart, string fileName)
    {
        if (renderPart != null)
            renderPart.Type = RenderPart.RenderPartType.Normal;
        var model = new MObjModel(context);
        model.Nsbmd = GetMapObjFile<Nsbmd>(context, fileName);
        model.Model = new Model(context, model.Nsbmd);
        model.Model.SetEmi(new Rgb555(10, 10, 10));
        model.Model.SetLightEnableFlag(0b0010);
        model.Scale = Vector3d.One;
        return model;
    }

    public static MObjModel LoadShadowModel(MkdsContext context, RenderPart renderPart, string fileName)
    {
        if (renderPart != null)
            renderPart.Type = RenderPart.RenderPartType.Normal;
        var model = new MObjModel(context);
        var nsbmd = GetMapObjFile<Nsbmd>(context, fileName);
        model.ShadowModel = new ShadowModel(context, nsbmd, 63);
        model.Scale       = Vector3d.One;
        return model;
    }

    public static MObjModel LoadBillboardModel(MkdsContext context, RenderPart renderPart, string fileName)
    {
        if (renderPart != null)
            renderPart.Type = RenderPart.RenderPartType.Billboard;
        var model = new MObjModel(context);
        var nsbmd = GetMapObjFile<Nsbmd>(context, fileName);
        model.BbModel = new BillboardModel(context, nsbmd);
        model.BbModel.SetEmission(new Rgb555(10, 10, 10));
        model.BbModel.SetLightMask(1 << 1);
        model.Scale = Vector3d.One;
        return model;
    }

    public static MObjModel LoadTexAnimBillboardModel(MkdsContext context, RenderPart renderPart,
        string nsbmdFileName, string nsbtpFileName)
    {
        if (renderPart != null)
            renderPart.Type = RenderPart.RenderPartType.Billboard;
        var model = new MObjModel(context);
        var nsbmd = GetMapObjFile<Nsbmd>(context, nsbmdFileName);
        var nsbtp = GetMapObjFile<Nsbtp>(context, nsbtpFileName);
        model.BbModel = new BillboardModel(context, nsbmd, nsbtp);
        model.BbModel.SetEmission(new Rgb555(10, 10, 10));
        model.BbModel.SetLightMask(1 << 1);
        model.Scale = Vector3d.One;
        return model;
    }

    public static AnimManager anim_20EA514(Model model, Nsbca animRes)
    {
        var result = new AnimManager(AnimManager.AnimKind.Jnt, model, 1);
        result.RegisterAllAnims(animRes);
        result.SetAnim(0);
        return result;
    }

    public static AnimManager anim_20EA514(Model model, Nsbta animRes)
    {
        var result = new AnimManager(AnimManager.AnimKind.Srt, model, 1);
        result.RegisterAllAnims(animRes);
        result.SetAnim(0);
        return result;
    }

    public static AnimManager anim_20EA514(MkdsContext context, Model model, Nsbtp animRes)
    {
        var result = new AnimManager(AnimManager.AnimKind.Pat, model, 1);
        result.RegisterAllAnims(animRes);
        foreach (var anmObj in result.AnmObjs)
            context.ModelManager.InitializeTexturePatternAnimationObject(anmObj);
        result.SetAnim(0);
        return result;
    }

    public static void LoadNsbcaAnim(MkdsContext context, MObjModel model, string fileName)
    {
        var nsbca = GetMapObjFile<Nsbca>(context, fileName);
        model.NsbcaAnim = anim_20EA514(model.Model, nsbca);
    }

    public static void LoadNsbtaAnim(MkdsContext context, MObjModel model, string fileName)
    {
        var nsbta = GetMapObjFile<Nsbta>(context, fileName);
        model.NsbtaAnim = anim_20EA514(model.Model, nsbta);
    }

    public static void LoadNsbtpAnim(MkdsContext context, MObjModel model, string fileName)
    {
        var nsbtp = GetMapObjFile<Nsbtp>(context, fileName);
        model.NsbtpAnim = anim_20EA514(context, model.Model, nsbtp);
    }

    public static Matrix3d EulerAnglesToMtx33(Vector3d angles)
    {
        var rotX = Matrix3d.CreateRotationX(MathHelper.DegreesToRadians(angles.X));
        var rotY = Matrix3d.CreateRotationY(MathHelper.DegreesToRadians(angles.Y));
        var rotZ = Matrix3d.CreateRotationZ(MathHelper.DegreesToRadians(angles.Z));
        return rotX * rotY * rotZ;
    }

    public static Matrix4x3d EulerAnglesToMtx43(Vector3d angles)
    {
        var rotX = Matrix4x3d.CreateRotationX(MathHelper.DegreesToRadians(angles.X));
        var rotY = Matrix4x3d.CreateRotationY(MathHelper.DegreesToRadians(angles.Y));
        var rotZ = Matrix4x3d.CreateRotationZ(MathHelper.DegreesToRadians(angles.Z));
        return rotX * rotY * rotZ;
    }

    private static Matrix4x3d MakeBillboardMatrix(in Matrix4x3d camMtx)
    {
        var bbMtx = new Matrix3d(camMtx.Row0, camMtx.Row1, camMtx.Row2);
        bbMtx.Transpose();
        return new Matrix4x3d(bbMtx.Row0, bbMtx.Row1, bbMtx.Row2, Vector3d.Zero);
    }

    private static Matrix4x3d MakeBillboardYMatrix(in Matrix4x3d camMtx)
    {
        var dword_217B850 = new Matrix3d(
            1, 0, 0,
            0, camMtx[1, 1], camMtx[1, 2],
            0, -camMtx[1, 2], camMtx[1, 1]);
        var bbMtx = new Matrix3d(camMtx.Row0, camMtx.Row1, camMtx.Row2);
        bbMtx.Transpose();
        bbMtx = dword_217B850 * bbMtx;
        return new Matrix4x3d(bbMtx.Row0, bbMtx.Row1, bbMtx.Row2, Vector3d.Zero);
    }

    public static Matrix4x3d GetBillboardAtPos(in Vector3d position, in Matrix4x3d camMtx)
    {
        var mtx = MakeBillboardMatrix(camMtx);
        mtx.Row3 = position / 16.0;
        return mtx;
    }

    public static Matrix4x3d GetYBillboardAtPos(in Vector3d position, in Matrix4x3d camMtx)
    {
        var mtx = MakeBillboardYMatrix(camMtx);
        mtx.Row3 = position / 16.0;
        return mtx;
    }

    public static double IdxToRad(ushort idx) => idx * System.Math.PI / (1 << 15);
    public static ushort DegToIdx(double deg) => (ushort)(deg * (1 << 15) / 180.0);

    public static double SinIdx(ushort idx) => System.Math.Sin(IdxToRad(idx));
    public static double CosIdx(ushort idx) => System.Math.Cos(IdxToRad(idx));

    public static void Model2RenderModel(MkdsContext context, Model model, in Matrix4x3d mtx, in Vector3d scale,
        byte alpha)
    {
        var  resMdl   = model.RenderObj.ModelResource;
        byte oldAlpha = (byte)resMdl.Materials.Materials[0].PolygonAttribute.Alpha;
        if (alpha >= oldAlpha)
        {
            model.Render(mtx, scale);
            return;
        }

        byte oldPolygonId = (byte)resMdl.Materials.Materials[0].PolygonAttribute.PolygonId;
        byte newPolygonId = context.MObjState.GetCyclicPolygonId();
        resMdl.SetAllPolygonId(newPolygonId);
        resMdl.SetAllAlpha(alpha);
        model.Render(mtx, scale);
        resMdl.SetAllPolygonId(oldPolygonId);
        resMdl.SetAllAlpha(oldAlpha);
    }

    public static void Model2RenderShadowModel(ShadowModel model, in Matrix4x3d mtx, in Vector3d scale, byte alpha)
    {
        if (alpha < model.Alpha)
        {
            byte oldAlpha = model.Alpha;
            model.Alpha = alpha;
            model.Render(mtx, scale);
            model.Alpha = oldAlpha;
        }
        else
            model.Render(mtx, scale);
    }

    public static Quaterniond QtrnFromXAngle(ushort angle)
    {
        return new Quaterniond(
            SinIdx((ushort)(angle >> 1)),
            0,
            0,
            CosIdx((ushort)(angle >> 1))
        );
    }

    public static Quaterniond QtrnRotY(Quaterniond quat, ushort angle)
    {
        double sin = SinIdx((ushort)(angle >> 1));
        double cos = CosIdx((ushort)(angle >> 1));
        return new Quaterniond(
            quat.X * cos - quat.Z * sin,
            quat.W * sin + quat.Y * cos,
            quat.Z * cos + quat.X * sin,
            quat.W * cos - quat.Y * sin
        );
    }

    public static Quaterniond QtrnRotZ(Quaterniond quat, ushort angle)
    {
        double sin = SinIdx((ushort)(angle >> 1));
        double cos = CosIdx((ushort)(angle >> 1));
        return new Quaterniond(
            quat.X * cos + quat.Y * sin,
            quat.Y * cos - quat.X * sin,
            quat.W * sin + quat.Z * cos,
            quat.W * cos - quat.Z * sin
        );
    }

    public static Quaterniond QtrnFromEulerAngles(in Vector3d rotation)
    {
        var result = QtrnFromXAngle(DegToIdx(rotation.X));
        result = QtrnRotY(result, DegToIdx(rotation.Y));
        return QtrnRotZ(result, DegToIdx(rotation.Z));
    }
}