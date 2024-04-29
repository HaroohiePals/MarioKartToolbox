using HaroohiePals.IO;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.G2d.Intermediate
{
    public class Cmnt
    {
        public const uint CmntBlockType = 0x544E4D43;

        public Cmnt(EndianBinaryReaderEx er)
        {
            BlockType   = er.ReadSignature(CmntBlockType);
            BlockSize   = er.Read<uint>();
            CommentData = er.ReadString(Encoding.ASCII, (int)BlockSize - 8).TrimEnd('\0');
        }

        public uint   BlockType;
        public uint   BlockSize;
        public string CommentData;
    }
}