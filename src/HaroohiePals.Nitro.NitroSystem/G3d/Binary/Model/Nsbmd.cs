using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class Nsbmd
{
    public const uint Bmd0Signature = 0x30444D42;

    public Nsbmd()
    {
        Header = new G3dFileHeader(Bmd0Signature, 2);
        ModelSet = new G3dModelSet();
    }

    public Nsbmd(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Nsbmd(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new G3dFileHeader(er, Bmd0Signature);
            if (Header.NrBlocks > 0)
            {
                er.BaseStream.Position = Header.BlockOffsets[0];
                ModelSet = new G3dModelSet(er);
            }

            if (Header.NrBlocks > 1)
            {
                er.BaseStream.Position = Header.BlockOffsets[1];
                TextureSet = new G3dTextureSet(er);
            }
        }
    }

    public byte[] Write()
    {
        var m = new MemoryStream();
        var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
        Header.NrBlocks = (ushort)(TextureSet != null ? 2 : 1);
        Header.Write(er);

        long curpos = er.BaseStream.Position;
        er.BaseStream.Position = 16;
        er.Write((uint)curpos);
        er.BaseStream.Position = curpos;

        ModelSet.Write(er);
        if (TextureSet != null)
        {
            curpos = er.BaseStream.Position;
            er.BaseStream.Position = 20;
            er.Write((uint)curpos);
            er.BaseStream.Position = curpos;

            TextureSet.Write(er);
        }

        er.BaseStream.Position = 8;
        er.Write((uint)er.BaseStream.Length);
        byte[] b = m.ToArray();
        er.Close();
        return b;
    }

    public G3dFileHeader Header;

    public G3dModelSet ModelSet;
    public G3dTextureSet TextureSet;
}