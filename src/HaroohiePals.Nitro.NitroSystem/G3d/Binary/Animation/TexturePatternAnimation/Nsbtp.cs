using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;

public sealed class Nsbtp
{
    public const uint Btp0Signature = 0x30505442;

    public Nsbtp(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Nsbtp(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new G3dFileHeader(er, Btp0Signature);
            if (Header.NrBlocks > 0)
            {
                er.BaseStream.Position = Header.BlockOffsets[0];
                TexturePatternAnimationSet = new G3dTexturePatternAnimationSet(er);
            }
        }
    }

    public G3dFileHeader Header;

    public G3dTexturePatternAnimationSet TexturePatternAnimationSet;
}
