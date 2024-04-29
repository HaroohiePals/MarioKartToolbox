using HaroohiePals.Graphics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace HaroohiePals.Nitro.G3
{
    public abstract class DisplayListBuffer : IDisposable
    {
        [Flags]
        public enum DlFlags
        {
            HasColors    = (1 << 0),
            HasNormals   = (1 << 1),
            HasTexCoords = (1 << 2)
        }

        protected NitroVertexData[] _vtxData;
        protected uint[]       _idxData;

        public DlFlags Flags { get; }

        public DisplayListBuffer(ReadOnlySpan<byte> dl)
        {
            var     vertices  = new List<NitroVertexData>();
            var     indices   = new List<uint>();
            var     normal    = Vector3.Zero;
            var     texCoord  = Vector2.Zero;
            var     mode      = (GxBegin)(-1);
            var     color     = Vector3.Zero;
            int     vtxX      = 0;
            int     vtxY      = 0;
            int     vtxZ      = 0;
            bool    useNormal = false;
            uint    mtxId     = NitroVertexData.CurMtxId;
            int     vtxCount  = 0;
            DlFlags flags     = 0;
            GxCmdUtil.ParseDl(dl, (op, param) =>
            {
                switch (op)
                {
                    case GxCmd.RestoreMatrix:
                        if ((param[0] & 0x1F) == 0x1F)
                            throw new Exception();
                        mtxId = param[0] & 0x1F;
                        break;
                    case GxCmd.Color:
                    {
                        var rgb5 = (Rgb555)(param[0] & 0x7FFF);
                        color     =  new Vector3(rgb5.R / 31f, rgb5.G / 31f, rgb5.B / 31f);
                        useNormal =  false;
                        flags     |= DlFlags.HasColors;
                        break;
                    }
                    case GxCmd.Normal:
                        normal = new Vector3(((short)((param[0] & 0x3FF) << 6) >> 6) / 512f,
                            ((short)(((param[0] >> 10) & 0x3FF) << 6) >> 6) / 512f,
                            ((short)(((param[0] >> 20) & 0x3FF) << 6) >> 6) / 512f);
                        useNormal =  true;
                        flags     |= DlFlags.HasNormals;
                        break;
                    case GxCmd.TexCoord:
                        texCoord =  new Vector2((short)(param[0] & 0xFFFF) / 16f, (short)(param[0] >> 16) / 16f);
                        flags    |= DlFlags.HasTexCoords;
                        break;
                    case GxCmd.Vertex:
                        vtxX = (short)(param[0] & 0xFFFF);
                        vtxY = (short)(param[0] >> 16);
                        vtxZ = (short)(param[1] & 0xFFFF);
                        break;
                    case GxCmd.VertexShort:
                        vtxX = (short)((param[0] & 0x3FF) << 6);
                        vtxY = (short)(((param[0] >> 10) & 0x3FF) << 6);
                        vtxZ = (short)(((param[0] >> 20) & 0x3FF) << 6);
                        break;
                    case GxCmd.VertexXY:
                        vtxX = (short)(param[0] & 0xFFFF);
                        vtxY = (short)(param[0] >> 16);
                        break;
                    case GxCmd.VertexXZ:
                        vtxX = (short)(param[0] & 0xFFFF);
                        vtxZ = (short)(param[0] >> 16);
                        break;
                    case GxCmd.VertexYZ:
                        vtxY = (short)(param[0] & 0xFFFF);
                        vtxZ = (short)(param[0] >> 16);
                        break;
                    case GxCmd.VertexDiff:
                        vtxX += (short)((param[0] & 0x3FF) << 6) >> 6;
                        vtxY += (short)(((param[0] >> 10) & 0x3FF) << 6) >> 6;
                        vtxZ += (short)(((param[0] >> 20) & 0x3FF) << 6) >> 6;
                        break;
                    case GxCmd.Begin:
                        mode     = (GxBegin)(param[0] & 3);
                        vtxCount = 0;
                        break;
                    case GxCmd.End:
                        mode = (GxBegin)(-1);
                        break;
                }

                if (mode != (GxBegin)(-1) && GxCmdUtil.IsVertex(op))
                {
                    vertices.Add(new NitroVertexData()
                    {
                        Position      = new Vector3(vtxX / 4096f, vtxY / 4096f, vtxZ / 4096f),
                        NormalOrColor = useNormal ? normal : color,
                        TexCoord      = texCoord,
                        MtxId         = mtxId | (useNormal ? NitroVertexData.HasNormalFlag : 0)
                    });

                    void emitQuad(int idxA, int idxB, int idxC, int idxD)
                    {
                        var vtxA = vertices[idxA].Position;
                        var vtxB = vertices[idxB].Position;
                        var vtxC = vertices[idxC].Position;
                        var vtxD = vertices[idxD].Position;

                        //Check which diagonal is shorter
                        if ((vtxC - vtxA).LengthSquared < (vtxD - vtxB).LengthSquared)
                        {
                            indices.Add((uint)idxA);
                            indices.Add((uint)idxB);
                            indices.Add((uint)idxC);

                            indices.Add((uint)idxA);
                            indices.Add((uint)idxC);
                            indices.Add((uint)idxD);
                        }
                        else
                        {
                            indices.Add((uint)idxA);
                            indices.Add((uint)idxB);
                            indices.Add((uint)idxD);

                            indices.Add((uint)idxB);
                            indices.Add((uint)idxC);
                            indices.Add((uint)idxD);
                        }
                    }

                    switch (mode)
                    {
                        case GxBegin.Triangles:
                            indices.Add((uint)(vertices.Count - 1));
                            break;
                        case GxBegin.Quads:
                            if ((vtxCount % 4) == 3)
                                emitQuad(vertices.Count - 4, vertices.Count - 3, vertices.Count - 2,
                                    vertices.Count - 1);
                            break;
                        case GxBegin.TriangleStrip:
                            if (vtxCount < 3)
                                indices.Add((uint)(vertices.Count - 1));
                            else if ((vtxCount & 1) != 0)
                            {
                                indices.Add((uint)(vertices.Count - 2));
                                indices.Add((uint)(vertices.Count - 3));
                                indices.Add((uint)(vertices.Count - 1));
                            }
                            else
                            {
                                indices.Add((uint)(vertices.Count - 3));
                                indices.Add((uint)(vertices.Count - 2));
                                indices.Add((uint)(vertices.Count - 1));
                            }

                            break;
                        case GxBegin.QuadStrip:
                            if (vtxCount >= 3 && (vtxCount & 1) == 1)
                                emitQuad(vertices.Count - 4, vertices.Count - 3, vertices.Count - 1,
                                    vertices.Count - 2);
                            break;
                    }

                    vtxCount++;
                }
            });
            _vtxData = vertices.ToArray();
            _idxData = indices.ToArray();
            Flags    = flags;
        }

        public abstract void Bind();
        public abstract void Draw();

        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}