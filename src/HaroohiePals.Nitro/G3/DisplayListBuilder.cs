using HaroohiePals.Graphics;
using HaroohiePals.IO;
using HaroohiePals.Nitro.Fx;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;
using System.IO;

namespace HaroohiePals.Nitro.G3
{
    public class DisplayListBuilder
    {
        private readonly MemoryStream         _stream = new();
        private readonly EndianBinaryWriterEx _writer;

        private readonly GxCmd[] _cmdQueue = new GxCmd[4];
        private          int     _cmdQueueCount;
        private          long    _cmdAddress;

        private bool _useCmdPacking;

        public DisplayListBuilder(bool useCmdPacking = true)
        {
            _useCmdPacking = useCmdPacking;
            _writer        = new EndianBinaryWriterEx(_stream);
        }

        /// <summary>
        /// Flushes commands that are remaining in the queue to the stream
        /// </summary>
        /// <param name="forceClear">When false the queue contents are kept when there are
        /// fewer than 4 commands queued, such that more commands can still be added.
        /// When true the queue is cleared, such that no more commands will be added to
        /// the current command word.</param>
        private void FlushQueue(bool forceClear = false)
        {
            if (_cmdQueueCount == 0)
                return;

            long curPos = _writer.BaseStream.Position;
            _writer.BaseStream.Position = _cmdAddress;
            for (int i = 0; i < _cmdQueueCount; i++)
                _writer.Write((byte)_cmdQueue[i]);
            _writer.BaseStream.Position = curPos;

            if (_cmdQueueCount == 4 || forceClear)
                _cmdQueueCount = 0;
        }

        private void WriteCommandId(GxCmd cmd)
        {
            if (!_useCmdPacking)
                _writer.Write((uint)cmd);
            else
            {
                if (_cmdQueueCount == 0)
                {
                    _cmdAddress = _writer.BaseStream.Position;
                    _writer.Write((uint)0);
                }

                _cmdQueue[_cmdQueueCount] = cmd;
                if (++_cmdQueueCount == 4)
                    FlushQueue();
            }
        }

        private void WriteCommand(GxCmd cmd, params uint[] args)
            => WriteCommand(cmd, (ReadOnlySpan<uint>)args);

        private void WriteCommand(GxCmd cmd, ReadOnlySpan<uint> args)
        {
            WriteCommandId(cmd);

            Debug.Assert(GxCmdUtil.GetParamCount(cmd) == args.Length);

            _writer.Write(args);

            if (_useCmdPacking && args.Length == 0)
            {
                _writer.Write((uint)0);
                if (_cmdQueueCount != 0)
                    _writer.BaseStream.Position -= 4;
            }
        }

        private void WriteCommand(GxCmd cmd, ReadOnlySpan<byte> args)
        {
            WriteCommandId(cmd);

            Debug.Assert(GxCmdUtil.GetParamCount(cmd) * 4 == args.Length);

            _writer.Write(args);

            if (_useCmdPacking && args.Length == 0)
            {
                _writer.Write((uint)0);
                if (_cmdQueueCount != 0)
                    _writer.BaseStream.Position -= 4;
            }
        }

        private (int x, int y, int z) ToVecFx10(in Vector3d vector)
        {
            int x = (int)System.Math.Round(vector.X * 512d);
            x = System.Math.Clamp(x, -512, 511);
            int y = (int)System.Math.Round(vector.Y * 512d);
            y = System.Math.Clamp(y, -512, 511);
            int z = (int)System.Math.Round(vector.Z * 512d);
            z = System.Math.Clamp(z, -512, 511);

            return (x, y, z);
        }

        public void Nop()
            => WriteCommand(GxCmd.Nop);

        public void PushMatrix()
            => WriteCommand(GxCmd.PushMatrix);

        public void PopMatrix(int count)
            => WriteCommand(GxCmd.PopMatrix, (uint)(count & 0x3F));

        public void StoreMatrix(uint index)
            => WriteCommand(GxCmd.StoreMatrix, index & 0x1F);

        public void RestoreMatrix(uint index)
            => WriteCommand(GxCmd.RestoreMatrix, index & 0x1F);

        public void Identity()
            => WriteCommand(GxCmd.Identity);

        public void LoadMatrix44(in Matrix4d mtx)
        {
            var args = new uint[16];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    args[i * 4 + j] = (uint)Fx32Util.FromDouble(mtx[i, j]);
                }
            }

            WriteCommand(GxCmd.LoadMatrix44, args);
        }

        public void LoadMatrix43(in Matrix4x3d mtx)
        {
            var args = new uint[12];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    args[i * 3 + j] = (uint)Fx32Util.FromDouble(mtx[i, j]);
                }
            }

            WriteCommand(GxCmd.LoadMatrix43, args);
        }

        public void MultMatrix44(in Matrix4d mtx)
        {
            var args = new uint[16];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    args[i * 4 + j] = (uint)Fx32Util.FromDouble(mtx[i, j]);
                }
            }

            WriteCommand(GxCmd.MultMatrix44, args);
        }

        public void MultMatrix43(in Matrix4x3d mtx)
        {
            var args = new uint[12];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    args[i * 3 + j] = (uint)Fx32Util.FromDouble(mtx[i, j]);
                }
            }

            WriteCommand(GxCmd.MultMatrix43, args);
        }

        public void MultMatrix33(in Matrix3d mtx)
        {
            var args = new uint[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    args[i * 3 + j] = (uint)Fx32Util.FromDouble(mtx[i, j]);
                }
            }

            WriteCommand(GxCmd.MultMatrix33, args);
        }

        public void Scale(in Vector3d scale)
            => WriteCommand(GxCmd.Scale,
                (uint)Fx32Util.FromDouble(scale.X),
                (uint)Fx32Util.FromDouble(scale.Y),
                (uint)Fx32Util.FromDouble(scale.Z));

        public void Translate(in Vector3d translation)
            => WriteCommand(GxCmd.Scale,
                (uint)Fx32Util.FromDouble(translation.X),
                (uint)Fx32Util.FromDouble(translation.Y),
                (uint)Fx32Util.FromDouble(translation.Z));

        public void Color(Rgb555 color)
            => WriteCommand(GxCmd.Color, color);

        public void Normal(in Vector3d normal)
        {
            var (x, y, z) = ToVecFx10(normal);
            WriteCommand(GxCmd.Normal, (uint)((x & 0x3FF) | ((y & 0x3FF) << 10) | ((z & 0x3FF) << 20)));
        }

        public void TexCoord(in Vector2d texCoord)
        {
            short s = (short)System.Math.Round(texCoord.X * 16d, MidpointRounding.AwayFromZero);
            short t = (short)System.Math.Round(texCoord.Y * 16d, MidpointRounding.AwayFromZero);

            WriteCommand(GxCmd.TexCoord, (uint)((ushort)s | ((ushort)t << 16)));
        }

        public void Vertex(in Vector3d vertex)
        {
            int x = Fx32Util.FromDouble(vertex.X);
            int y = Fx32Util.FromDouble(vertex.Y);
            int z = Fx32Util.FromDouble(vertex.Z);

            WriteCommand(GxCmd.Vertex, (uint)((ushort)x | ((ushort)y << 16)), (ushort)z);
        }

        public void Vertex10(in Vector3d vertex)
        {
            int x = (int)System.Math.Round(vertex.X * 64d);
            int y = (int)System.Math.Round(vertex.Y * 64d);
            int z = (int)System.Math.Round(vertex.Z * 64d);

            WriteCommand(GxCmd.VertexShort, (uint)((x & 0x3FF) | ((y & 0x3FF) << 10) | ((z & 0x3FF) << 20)));
        }

        public void VertexXY(in Vector2d vertex)
        {
            int x = Fx32Util.FromDouble(vertex.X);
            int y = Fx32Util.FromDouble(vertex.Y);

            WriteCommand(GxCmd.VertexXY, (uint)((ushort)x | ((ushort)y << 16)));
        }

        public void VertexXZ(in Vector2d vertex)
        {
            int x = Fx32Util.FromDouble(vertex.X);
            int z = Fx32Util.FromDouble(vertex.Y);

            WriteCommand(GxCmd.VertexXZ, (uint)((ushort)x | ((ushort)z << 16)));
        }

        public void VertexYZ(in Vector2d vertex)
        {
            int y = Fx32Util.FromDouble(vertex.X);
            int z = Fx32Util.FromDouble(vertex.Y);

            WriteCommand(GxCmd.VertexYZ, (uint)((ushort)y | ((ushort)z << 16)));
        }

        public void VertexDiff(in Vector3d diff)
        {
            int x = Fx32Util.FromDouble(diff.X);
            int y = Fx32Util.FromDouble(diff.Y);
            int z = Fx32Util.FromDouble(diff.Z);

            WriteCommand(GxCmd.VertexDiff, (uint)((x & 0x3FF) | ((y & 0x3FF) << 10) | ((z & 0x3FF) << 20)));
        }

        public void PolygonAttr(GxPolygonAttr attr)
            => WriteCommand(GxCmd.PolygonAttr, attr);

        public void TexImageParam(GxTexImageParam param)
            => WriteCommand(GxCmd.TexImageParam, param);

        public void TexPlttBase(ushort plttBase)
            => WriteCommand(GxCmd.TexPlttBase, plttBase & 0x1FFFu);

        public void MaterialColor0(Rgb555 diffuse, bool setVtxColor, Rgb555 ambient)
            => WriteCommand(GxCmd.MaterialColor0,
                (diffuse & 0x7FFFu) | (setVtxColor ? 0x8000u : 0u) | ((ambient & 0x7FFFu) << 16));

        public void MaterialColor1(Rgb555 specular, bool shininess, Rgb555 emission)
            => WriteCommand(GxCmd.MaterialColor1,
                (specular & 0x7FFFu) | (shininess ? 0x8000u : 0u) | ((emission & 0x7FFFu) << 16));

        public void Shininess(ReadOnlySpan<byte> table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (table.Length != 128)
                throw new ArgumentException(nameof(table));

            WriteCommand(GxCmd.Shininess, table);
        }

        public void LightVector(int light, in Vector3d vector)
        {
            if (light < 0 || light > 3)
                throw new ArgumentOutOfRangeException(nameof(light));

            var (x, y, z) = ToVecFx10(vector);
            WriteCommand(GxCmd.LightVector,
                (uint)((x & 0x3FF) | ((y & 0x3FF) << 10) | ((z & 0x3FF) << 20) | (light << 30)));
        }

        public void LightColor(int light, Rgb555 color)
        {
            if (light < 0 || light > 3)
                throw new ArgumentOutOfRangeException(nameof(light));

            WriteCommand(GxCmd.LightColor, (uint)((color & 0x7FFF) | (light << 30)));
        }

        public void Begin(GxBegin mode)
            => WriteCommand(GxCmd.Begin, (uint)mode & 3);

        public void End()
            => WriteCommand(GxCmd.End);

        public void SwapBuffers(GxSortMode sortMode, GxBufferMode bufferMode)
            => WriteCommand(GxCmd.SwapBuffers, ((uint)sortMode & 1) | (((uint)bufferMode & 1) << 1));

        public void ViewPort(byte x1, byte y1, byte x2, byte y2)
            => WriteCommand(GxCmd.Viewport, (uint)(x1 | (y1 << 8) | (x2 << 16) | (y2 << 24)));

        public void BoxTest(in Vector3d xyz, in Vector3d whd)
        {
            int x = Fx32Util.FromDouble(xyz.X);
            int y = Fx32Util.FromDouble(xyz.Y);
            int z = Fx32Util.FromDouble(xyz.Z);
            int w = Fx32Util.FromDouble(whd.X);
            int h = Fx32Util.FromDouble(whd.Y);
            int d = Fx32Util.FromDouble(whd.Z);

            WriteCommand(GxCmd.BoxTest,
                (uint)((ushort)x | ((ushort)y << 16)),
                (uint)((ushort)z | ((ushort)w << 16)),
                (uint)((ushort)h | ((ushort)d << 16)));
        }

        public void PositionTest(in Vector3d position)
        {
            int x = Fx32Util.FromDouble(position.X);
            int y = Fx32Util.FromDouble(position.Y);
            int z = Fx32Util.FromDouble(position.Z);

            WriteCommand(GxCmd.PositionTest, (uint)((ushort)x | ((ushort)y << 16)), (ushort)z);
        }

        public void VectorTest(in Vector3d vector)
        {
            var (x, y, z) = ToVecFx10(vector);
            WriteCommand(GxCmd.VectorTest, (uint)((x & 0x3FF) | ((y & 0x3FF) << 10) | ((z & 0x3FF) << 20)));
        }

        public byte[] ToArray()
        {
            FlushQueue();
            return _stream.ToArray();
        }
    }
}