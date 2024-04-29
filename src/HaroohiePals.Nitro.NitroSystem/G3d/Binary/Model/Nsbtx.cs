using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class Nsbtx
{
    public const uint Btx0Signature = 0x30585442;

    public Nsbtx()
    {
        Header = new G3dFileHeader(Btx0Signature, 1);
        TextureSet = new G3dTextureSet();
    }

    public Nsbtx(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Nsbtx(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new G3dFileHeader(er, Btx0Signature);
            if (Header.NrBlocks > 0)
            {
                er.BaseStream.Position = Header.BlockOffsets[0];
                TextureSet = new G3dTextureSet(er);
            }
        }
    }

    public byte[] Write()
    {
        var m = new MemoryStream();
        using (var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian))
        {
            er.BeginChunk(8);
            {
                Header.NrBlocks = 1;
                Header.Write(er);

                er.WriteCurposRelative(0x10);
                TextureSet.Write(er);
            }
            er.EndChunk();
            return m.ToArray();
        }
    }

    public G3dFileHeader Header;
    public G3dTextureSet TextureSet;
}