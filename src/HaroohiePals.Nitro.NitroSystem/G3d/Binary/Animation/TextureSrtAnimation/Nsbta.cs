using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;

public sealed class Nsbta
{
    public const uint Bta0Signature = 0x30415442;

    public Nsbta(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Nsbta(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new G3dFileHeader(er, Bta0Signature);
            if (Header.NrBlocks > 0)
            {
                er.BaseStream.Position = Header.BlockOffsets[0];
                TextureSrtAnimationSet = new G3dTextureSrtAnimationSet(er);
            }
        }
    }

    public byte[] Write()
    {
        var m = new MemoryStream();
        var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
        Header.NrBlocks = 1;
        Header.Write(er);

        long curpos = er.BaseStream.Position;
        er.BaseStream.Position = 16;
        er.Write((uint)curpos);
        er.BaseStream.Position = curpos;

        TextureSrtAnimationSet.Write(er);

        er.BaseStream.Position = 8;
        er.Write((uint)er.BaseStream.Length);
        byte[] b = m.ToArray();
        er.Close();
        return b;
    }

    public G3dFileHeader Header;

    public G3dTextureSrtAnimationSet TextureSrtAnimationSet;
}