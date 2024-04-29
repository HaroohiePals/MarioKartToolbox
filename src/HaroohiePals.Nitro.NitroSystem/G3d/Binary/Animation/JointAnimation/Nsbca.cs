using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;

public sealed class Nsbca
{
    public const uint Bca0Signature = 0x30414342;

    public Nsbca(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Nsbca(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new G3dFileHeader(er, Bca0Signature);
            if (Header.NrBlocks > 0)
            {
                er.BaseStream.Position = Header.BlockOffsets[0];
                JointAnimationSet = new G3dJointAnimationSet(er);
            }
        }
    }

    public G3dFileHeader Header;

    public G3dJointAnimationSet JointAnimationSet;
}
