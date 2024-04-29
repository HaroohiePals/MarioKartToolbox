using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace HaroohiePals.Nitro.G3.OpenGL;

public static class GLNitroVertexData
{
    public static void SetupVertexAttribPointers()
    {
        //aPosition
        GL.VertexAttribPointer(NitroVertexData.PosIdx, 3, VertexAttribPointerType.Float, false, NitroVertexData.Size,
            Marshal.OffsetOf<NitroVertexData>(nameof(NitroVertexData.Position)));

        //aColor
        GL.VertexAttribPointer(NitroVertexData.NrmClrIdx, 3, VertexAttribPointerType.Float, false, NitroVertexData.Size,
            Marshal.OffsetOf<NitroVertexData>(nameof(NitroVertexData.NormalOrColor)));

        //aTexCoord
        GL.VertexAttribPointer(NitroVertexData.TexCoordIdx, 3, VertexAttribPointerType.Float, false, NitroVertexData.Size,
            Marshal.OffsetOf<NitroVertexData>(nameof(NitroVertexData.TexCoord)));

        //aMtxId
        GL.VertexAttribIPointer(NitroVertexData.MtxIdIdx, 1, VertexAttribIntegerType.UnsignedInt, NitroVertexData.Size,
            Marshal.OffsetOf<NitroVertexData>(nameof(NitroVertexData.MtxId)));

        EnableAllAttribs();
    }

    public static void EnableAllAttribs()
    {
        GL.EnableVertexAttribArray(NitroVertexData.PosIdx);
        GL.EnableVertexAttribArray(NitroVertexData.NrmClrIdx);
        GL.EnableVertexAttribArray(NitroVertexData.TexCoordIdx);
        GL.EnableVertexAttribArray(NitroVertexData.MtxIdIdx);
    }

    public static void SetupFixedColor(Vector3 color)
    {
        GL.DisableVertexAttribArray(NitroVertexData.NrmClrIdx);
        GL.VertexAttrib3(NitroVertexData.NrmClrIdx, color);
    }

    public static void SetupFixedTexCoord(Vector2 texCoord)
    {
        GL.DisableVertexAttribArray(NitroVertexData.TexCoordIdx);
        GL.VertexAttrib2(NitroVertexData.TexCoordIdx, texCoord);
    }
}