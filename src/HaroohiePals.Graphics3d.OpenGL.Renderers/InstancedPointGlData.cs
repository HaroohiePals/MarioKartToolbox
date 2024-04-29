using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

struct InstancedPointGlData
{
    [GLVertexAttrib(0)]
    public Matrix4 Transform;

    [GLVertexAttrib(4)]
    public Vector3 Color;

    [GLVertexAttribI(5)]
    public uint PickingId;

    [GLVertexAttribI(6)]
    public uint UseTexture;

    [GLVertexAttribI(7)]
    public uint Hover;

    [GLVertexAttribI(8)]
    public uint Highlight;

    [GLVertexAttrib(9)]
    public float TexCoordAngle;

    public static int Size => Marshal.SizeOf<InstancedPointGlData>();

    public override string ToString() => $"{Transform.Row3.Xyz}";
}