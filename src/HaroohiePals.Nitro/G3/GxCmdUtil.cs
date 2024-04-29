using HaroohiePals.IO;
using System;

namespace HaroohiePals.Nitro.G3
{
    public static class GxCmdUtil
    {
        public static int GetParamCount(GxCmd cmd) => cmd switch
        {
            GxCmd.Nop            => 0,
            GxCmd.MatrixMode     => 1,
            GxCmd.PushMatrix     => 0,
            GxCmd.PopMatrix      => 1,
            GxCmd.StoreMatrix    => 1,
            GxCmd.RestoreMatrix  => 1,
            GxCmd.Identity       => 0,
            GxCmd.LoadMatrix44   => 16,
            GxCmd.LoadMatrix43   => 12,
            GxCmd.MultMatrix44   => 16,
            GxCmd.MultMatrix43   => 12,
            GxCmd.MultMatrix33   => 9,
            GxCmd.Scale          => 3,
            GxCmd.Translate      => 3,
            GxCmd.Color          => 1,
            GxCmd.Normal         => 1,
            GxCmd.TexCoord       => 1,
            GxCmd.Vertex         => 2,
            GxCmd.VertexShort    => 1,
            GxCmd.VertexXY       => 1,
            GxCmd.VertexXZ       => 1,
            GxCmd.VertexYZ       => 1,
            GxCmd.VertexDiff     => 1,
            GxCmd.PolygonAttr    => 1,
            GxCmd.TexImageParam  => 1,
            GxCmd.TexPlttBase    => 1,
            GxCmd.MaterialColor0 => 1,
            GxCmd.MaterialColor1 => 1,
            GxCmd.LightVector    => 1,
            GxCmd.LightColor     => 1,
            GxCmd.Shininess      => 32,
            GxCmd.Begin          => 1,
            GxCmd.End            => 0,
            GxCmd.SwapBuffers    => 1,
            GxCmd.Viewport       => 1,
            GxCmd.BoxTest        => 3,
            GxCmd.PositionTest   => 2,
            GxCmd.VectorTest     => 1,
            _                    => 0
        };

        public static bool IsValid(GxCmd cmd)
            => cmd == GxCmd.Nop ||
               cmd >= GxCmd.MatrixMode && cmd <= GxCmd.Translate ||
               cmd >= GxCmd.Color && cmd <= GxCmd.TexPlttBase ||
               cmd >= GxCmd.MaterialColor0 && cmd <= GxCmd.Shininess ||
               cmd == GxCmd.Begin || cmd == GxCmd.End ||
               cmd == GxCmd.SwapBuffers ||
               cmd == GxCmd.Viewport ||
               cmd >= GxCmd.BoxTest && cmd <= GxCmd.VectorTest;

        public static bool IsUnsafeParameterless(GxCmd cmd)
            => cmd >= GxCmd.MatrixMode && GetParamCount(cmd) == 0;

        public static bool IsVertex(GxCmd cmd)
            => cmd >= GxCmd.Vertex && cmd <= GxCmd.VertexDiff;

        public delegate void ParseDlCallback(GxCmd op, uint[] param);

        public static void ParseDl(ReadOnlySpan<byte> dl, ParseDlCallback callback)
        {
            int offs = 0;
            while (offs + 4 <= dl.Length)
            {
                uint ops = IOUtil.ReadU32Le(dl[offs..]);
                offs += 4;
                while (ops != 0)
                {
                    var op = (GxCmd)(ops & 0xFF);
                    if (op != GxCmd.Nop && IsValid(op))
                    {
                        int paramCount = GetParamCount(op);
                        var param      = new uint[paramCount];
                        for (int i = 0; i < paramCount; i++)
                        {
                            param[i] =  IOUtil.ReadU32Le(dl[offs..]);
                            offs     += 4;
                        }

                        callback(op, param);
                    }

                    ops >>= 8;
                }
            }
        }
    }
}