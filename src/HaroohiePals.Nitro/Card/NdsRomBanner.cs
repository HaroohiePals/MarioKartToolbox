using HaroohiePals.IO;
using System;
using System.IO;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public sealed class NdsRomBanner
{
    public const int SIZE = 0x840;

    public NdsBannerHeader Header;

    public NdsBannerDataV1 Data;

    public NdsRomBanner() { }

    public NdsRomBanner(byte[] data)
    {
        using (var m = new MemoryStream(data))
        {
            var er = new EndianBinaryReaderEx(m);

            Header = new NdsBannerHeader(er);
            Data = new NdsBannerDataV1(er);
        }
    }

    public byte[] Write()
    {
        using (var m = new MemoryStream())
        {
            var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            Header.CRC16_v1 = Data.GetCrc();
            Header.Write(ew);
            Data.Write(ew);
            ew.Close();
            return m.ToArray();
        }
    }
}