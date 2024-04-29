using System;
using System.IO;

namespace HaroohiePals.IO.Compression
{
    public class Yaz0
    {
        public static byte[] Compress(byte[] src)
        {
            var context = new CompressionWindow(src, 3, 273, 4096);
            var dst     = new MemoryStream();
            dst.WriteByte((byte)'Y');
            dst.WriteByte((byte)'a');
            dst.WriteByte((byte)'z');
            dst.WriteByte((byte)'0');
            dst.WriteByte((byte)((src.Length >> 24) & 0xFF));
            dst.WriteByte((byte)((src.Length >> 16) & 0xFF));
            dst.WriteByte((byte)((src.Length >> 8) & 0xFF));
            dst.WriteByte((byte)(src.Length & 0xFF));
            dst.Write(new byte[8]);

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
                        uint back = context.Position - pos;

                        if (len >= 18)
                        {
                            dst.WriteByte((byte)(((back - 1) >> 8) & 0xF));
                            dst.WriteByte((byte)((back - 1) & 0xFF));
                            dst.WriteByte((byte)((len - 0x12) & 0xFF));
                        }
                        else
                        {
                            dst.WriteByte((byte)((((back - 1) >> 8) & 0xF) | ((((uint)len - 2) & 0xF) << 4)));
                            dst.WriteByte((byte)((back - 1) & 0xFF));
                        }

                        context.Slide(len);
                    }
                    else
                    {
                        header |= 1;
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

        public static byte[] Decompress(ReadOnlySpan<byte> data)
        {
            uint len     = IOUtil.ReadU32Be(data[4..]);
            var  result  = new byte[len];
            int  ptr     = 16;
            int  dstOffs = 0;
            while (true)
            {
                byte header = data[ptr++];
                for (int i = 0; i < 8; i++)
                {
                    if ((header & 0x80) != 0)
                        result[dstOffs++] = data[ptr++];
                    else
                    {
                        byte b      = data[ptr++];
                        int  offs   = ((b & 0xF) << 8 | data[ptr++]) + 1;
                        int  length = (b >> 4) + 2;
                        if (length == 2)
                            length = data[ptr++] + 0x12;
                        for (int j = 0; j < length; j++)
                        {
                            result[dstOffs] = result[dstOffs - offs];
                            dstOffs++;
                        }
                    }

                    if (dstOffs >= len)
                        return result;
                    header <<= 1;
                }
            }
        }
    }
}