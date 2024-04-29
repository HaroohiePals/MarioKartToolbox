using HaroohiePals.Graphics;
using OpenTK.Mathematics;
using Vector3d = OpenTK.Mathematics.Vector3d;
using Vector4 = System.Numerics.Vector4;

namespace HaroohiePals.Nitro.G3;

public sealed class GeometryEngineState
{
    private const int MATERIAL_COLOR_1_SHININESS_FLAG = 0x8000;

    public bool TranslucentPass { get; set; } = false;

    public GxPolygonAttr   PolygonAttr    { get; set; } = 0x1F008F;
    public GxTexImageParam TexImageParam  { get; set; }
    public uint            MaterialColor0 { get; set; } = 0x2108A108;
    public uint            MaterialColor1 { get; set; } = 0x2108A108;

    private readonly Vector3d[] _lightVectors = new Vector3d[4]
    {
        (-0.5774, -0.5774, -0.5774),
        (-1, 0, 0),
        (1, 0, 0),
        (0, -1, 0)
    };

    public Rgb555[] LightColors { get; } = new Rgb555[4]
    {
        new(31, 31, 31),
        new(31, 0, 0),
        new(0, 31, 0),
        new(0, 0, 31)
    };

    public GxMtxMode MatrixMode { get; set; } = GxMtxMode.PositionVector;

    private readonly Matrix4d[] _positionMatrixStack = new Matrix4d[31];
    private readonly Matrix3d[] _directionMatrixStack = new Matrix3d[31];
    private          Matrix4d   _textureMatrixStack;

    public  Matrix4d PositionMatrix { get; private set; } = Matrix4d.Identity;
    public  Matrix3d DirectionMatrix { get; private set; } = Matrix3d.Identity;
    private Matrix4d _textureMatrix = Matrix4d.Identity;

    public Vector2d TexCoord { get; set; }

    public void Translate(in Vector3d translation)
    {
        var m = Matrix4d.CreateTranslation(translation);
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = m * PositionMatrix;
        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = m * _textureMatrix;
    }

    public void Scale(in Vector3d scale)
    {
        var m = Matrix4d.Scale(scale);
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = m * PositionMatrix;
        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = m * _textureMatrix;
    }

    public void MultMatrix(in Matrix4d mtx)
    {
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = mtx * PositionMatrix;
        if (MatrixMode == GxMtxMode.PositionVector)
        {
            DirectionMatrix = new Matrix3d(
                mtx[0, 0], mtx[0, 1], mtx[0, 2],
                mtx[1, 0], mtx[1, 1], mtx[1, 2],
                mtx[2, 0], mtx[2, 1], mtx[2, 2]) * DirectionMatrix;
        }

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = mtx * _textureMatrix;
    }

    public void MultMatrix(in Matrix4x3d mtx)
    {
        var mtx4 = new Matrix4d(
            new(mtx.Row0, 0),
            new(mtx.Row1, 0),
            new(mtx.Row2, 0),
            new(mtx.Row3, 1));
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = mtx4 * PositionMatrix;

        if (MatrixMode == GxMtxMode.PositionVector)
            DirectionMatrix = new Matrix3d(mtx.Row0, mtx.Row1, mtx.Row2) * DirectionMatrix;

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = mtx4 * _textureMatrix;
    }

    public void MultMatrix(in Matrix3d mtx)
    {
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = new Matrix4d(mtx) * PositionMatrix;
        if (MatrixMode == GxMtxMode.PositionVector)
            DirectionMatrix = mtx * DirectionMatrix;

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = new Matrix4d(mtx) * _textureMatrix;
    }

    public void LoadMatrix(in Matrix4d mtx)
    {
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = mtx;
        if (MatrixMode == GxMtxMode.PositionVector)
            DirectionMatrix = new Matrix3d(mtx);

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = mtx;
    }

    public void LoadMatrix(in Matrix4x3d mtx)
    {
        var mtx4 = new Matrix4d(
            new(mtx.Row0, 0),
            new(mtx.Row1, 0),
            new(mtx.Row2, 0),
            new(mtx.Row3, 1));
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
            PositionMatrix = mtx4;
        if (MatrixMode == GxMtxMode.PositionVector)
            DirectionMatrix = new Matrix3d(mtx4);

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = mtx4;
    }

    public void StoreMatrix(int index)
    {
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
        {
            _positionMatrixStack[index] = PositionMatrix;
            _directionMatrixStack[index] = DirectionMatrix;
        }

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrixStack = _textureMatrix;
    }

    public void RestoreMatrix(int index)
    {
        if (MatrixMode == GxMtxMode.Position || MatrixMode == GxMtxMode.PositionVector)
        {
            PositionMatrix = _positionMatrixStack[index];
            DirectionMatrix = _directionMatrixStack[index];
        }

        if (MatrixMode == GxMtxMode.Texture)
            _textureMatrix = _textureMatrixStack;
    }

    public void SetLightVector(int light, Vector3d vec)
    {
        _lightVectors[light] = Vector3d.TransformRow(vec, DirectionMatrix);
    }

    private static Vector4 Rgb555ToRgba8(Rgb555 color)
        => new(color.R / 31f, color.G / 31f, color.B / 31f, 1);

    public unsafe void ToUniform(ref GeStateUniform result)
    {
        result.PolygonAttr   = PolygonAttr;
        result.TexImageParam = TexImageParam;
        result.Flags         = 0;
        if ((MaterialColor1 & MATERIAL_COLOR_1_SHININESS_FLAG) != 0)
            result.Flags |= GeStateUniform.ShininessFlag;
        if (TranslucentPass)
            result.Flags |= GeStateUniform.AlphaPassFlag;
        result.Diffuse  = Rgb555ToRgba8((Rgb555)(MaterialColor0 & 0x7FFF));
        result.Ambient  = Rgb555ToRgba8((Rgb555)((MaterialColor0 >> 16) & 0x7FFF));
        result.Specular = Rgb555ToRgba8((Rgb555)(MaterialColor1 & 0x7FFF));
        result.Emission = Rgb555ToRgba8((Rgb555)((MaterialColor1 >> 16) & 0x7FFF));
        for (int i = 0; i < 4; i++)
        {
            result.SetLightVector(i, _lightVectors[i]);
            result.SetLightColor(i, LightColors[i]);
        }

        for (int i = 0; i < 31; i++)
        {
            result.SetPosStackMtx(i, _positionMatrixStack[i]);
            result.SetDirStackMtx(i, _directionMatrixStack[i]);
        }

        result.SetPosMtx(PositionMatrix);
        result.SetDirMtx(DirectionMatrix);
        result.SetTexMtx(_textureMatrix);
    }
}
