using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

static class GLVertexData
{
    public static void SetupVertexAttribPointers()
    {
        //aPosition
        GL.VertexAttribPointer(VertexData.PosIdx, 3, VertexAttribPointerType.Float, false, VertexData.Size,
            Marshal.OffsetOf<VertexData>(nameof(VertexData.Position)));

        //aColor
        GL.VertexAttribPointer(VertexData.NrmClrIdx, 3, VertexAttribPointerType.Float, false, VertexData.Size,
            Marshal.OffsetOf<VertexData>(nameof(VertexData.NormalOrColor)));

        //aTexCoord
        GL.VertexAttribPointer(VertexData.TexCoordIdx, 3, VertexAttribPointerType.Float, false, VertexData.Size,
            Marshal.OffsetOf<VertexData>(nameof(VertexData.TexCoord)));

        //aMtxId
        GL.VertexAttribIPointer(VertexData.MtxIdIdx, 1, VertexAttribIntegerType.UnsignedInt, VertexData.Size,
            Marshal.OffsetOf<VertexData>(nameof(VertexData.MtxId)));

        EnableAllAttribs();
    }

    public static void EnableAllAttribs()
    {
        GL.EnableVertexAttribArray(VertexData.PosIdx);
        GL.EnableVertexAttribArray(VertexData.NrmClrIdx);
        GL.EnableVertexAttribArray(VertexData.TexCoordIdx);
        GL.EnableVertexAttribArray(VertexData.MtxIdIdx);
    }

    public static void SetupFixedColor(Vector3 color)
    {
        GL.DisableVertexAttribArray(VertexData.NrmClrIdx);
        GL.VertexAttrib3(VertexData.NrmClrIdx, color);
    }

    public static void SetupFixedTexCoord(Vector2 texCoord)
    {
        GL.DisableVertexAttribArray(VertexData.TexCoordIdx);
        GL.VertexAttrib2(VertexData.TexCoordIdx, texCoord);
    }
}