using HaroohiePals.IO;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.G2d.Intermediate
{
    public class Link
    {
        public const uint LinkBlockType = 0x4B4E494C;

        public Link(EndianBinaryReaderEx er)
        {
            BlockType = er.ReadSignature(LinkBlockType);
            BlockSize = er.Read<uint>();
            FileName  = er.ReadString(Encoding.ASCII, (int)BlockSize - 8).TrimEnd('\0');
        }

        public uint   BlockType;
        public uint   BlockSize;
        public string FileName;
    }
}