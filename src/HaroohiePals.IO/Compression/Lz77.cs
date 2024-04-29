using System;
using System.IO;

namespace HaroohiePals.IO.Compression
{
    public static class Lz77
    {
        public static byte[] Compress(byte[] src)
        {
            var context = new CompressionWindow(src, 3, 18, 4096);
            var dst     = new MemoryStream();
            dst.WriteByte(0x10);
            dst.WriteByte((byte)(src.Length & 0xFF));
            dst.WriteByte((byte)((src.Length >> 8) & 0xFF));
            dst.WriteByte((byte)((src.Length >> 16) & 0xFF));

            while (context.Position < src.Length)
            {
                long blockStart = dst.Position;
                dst.WriteByte(0); //to be filled in later
                byte header = 0;
                for (int i = 0; i < 8; i++)
                {
                    header         <<= 1;
                    var (pos, len) =   context.FindRun();
                    if (len > 0)
                    {
                        header |= 1;
                        uint back = context.Position - pos;
                        dst.WriteByte((byte)((((back - 1) >> 8) & 0xF) | ((((uint)len - 3) & 0xF) << 4)));
                        dst.WriteByte((byte)((back - 1) & 0xFF));
                        context.Slide(len);
                    }
                    else
                    {
                        dst.WriteByte(src[context.Position]);
                        context.Slide(1);
                    }

                    if (context.Position >= src.Length)
                    {
                        header <<= 7 - i;
                        break;
                    }
                }

                long curPos = dst.Position;
                dst.Position = blockStart;
                dst.WriteByte(header);
                dst.Position = curPos;
            }

            while ((dst.Position % 4) != 0)
                dst.WriteByte(0);
            return dst.ToArray();
        }

        public static byte[] Decompress(ReadOnlySpan<byte> src)
        {
            uint outSize = IOUtil.ReadU24Le(src[1..]);
            var  dst     = new byte[outSize];
            int  srcOffs = 4;
            int  dstOffs = 0;
            while (true)
            {
                byte header = src[srcOffs++];
                for (int i = 0; i < 8; i++)
                {
                    if ((header & 0x80) == 0)
                        dst[dstOffs++] = src[srcOffs++];
                    else
                    {
                        byte a = src[srcOffs++];
                        byte b = src[srcOffs++];

                        int offs   = (((a & 0xF) << 8) | b) + 1;
                        int length = (a >> 4) + 3;

                        for (int j = 0; j < length; j++)
                        {
                            dst[dstOffs] = dst[dstOffs - offs];
                            dstOffs++;
                        }
                    }

                    if (dstOffs >= outSize)
                        return dst;
                    header <<= 1;
                }
            }
        }
    }
}