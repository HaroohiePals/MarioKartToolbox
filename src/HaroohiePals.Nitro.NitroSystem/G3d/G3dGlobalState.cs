using HaroohiePals.Graphics;
using HaroohiePals.Nitro.G3;
using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public sealed class G3dGlobalState
{
    public Matrix4d CameraMatrix = Matrix4d.Identity;

    public readonly Vector3d[] LightVectors = new Vector3d[4]
    {
        (-0.5774, -0.5774, -0.5774),
        (-1, 0, 0),
        (1, 0, 0),
        (0, -1, 0)
    };

    public uint MaterialColor0 = 0x4210C210;
    public uint MaterialColor1 = 0x4210C210;

    public GxPolygonAttr PolygonAttr = new()
    {
        LightMask   = 0xF,
        PolygonMode = GxPolygonMode.Modulate,
        CullMode    = GxCull.Back,
        PolygonId   = 0,
        Alpha       = 31
    };

    public readonly Rgb555[] LightColors = new Rgb555[4]
    {
        new(31, 31, 31),
        new(31, 0, 0),
        new(0, 31, 0),
        new(0, 0, 31)
    };

    public Vector3d BaseTrans = Vector3d.Zero;
    public Matrix3d BaseRot = Matrix3d.Identity;
    public Vector3d BaseScale = Vector3d.One;
    public GxTexImageParam TexImageParam = 0;

    public Matrix4d GetInvertedCameraMatrix() => CameraMatrix.Inverted();

    public void FlushP(GeometryEngineState geState)
    {
        geState.MatrixMode = GxMtxMode.PositionVector;

        geState.LoadMatrix(CameraMatrix);

        geState.SetLightVector(0, LightVectors[0]);
        geState.SetLightVector(1, LightVectors[1]);
        geState.SetLightVector(2, LightVectors[2]);
        geState.SetLightVector(3, LightVectors[3]);

        geState.LightColors[0] = LightColors[0];
        geState.LightColors[1] = LightColors[1];
        geState.LightColors[2] = LightColors[2];
        geState.LightColors[3] = LightColors[3];

        geState.MaterialColor0 = MaterialColor0;
        geState.MaterialColor1 = MaterialColor1;
        geState.PolygonAttr    = PolygonAttr;

        geState.MultMatrix(new Matrix4x3d(BaseRot.Row0, BaseRot.Row1, BaseRot.Row2, BaseTrans));
        geState.Scale(BaseScale);

        geState.TexImageParam = TexImageParam;
    }
}
