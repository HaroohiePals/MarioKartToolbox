using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Sseq
    {
        public const uint SseqSignature = 0x51455353;
        public const uint DataSignature = 0x41544144;

        public Sseq(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Sseq(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                FileHeader = new BinaryFileHeader(er);
                if (FileHeader.Signature != SseqSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, SseqSignature, 0);
                BlockHeader = new BinaryBlockHeader(er);
                if (BlockHeader.Kind != DataSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, DataSignature, 0x10);
                BaseOffset             = er.Read<uint>();
                er.BaseStream.Position = BaseOffset;
                Data                   = er.Read<byte>((int)(FileHeader.FileSize - BaseOffset));
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk(8);
            FileHeader.FileSize   = 0;
            FileHeader.DataBlocks = 1;
            FileHeader.Write(er);
            er.BeginChunk(4);
            BlockHeader.Size = 0;
            BlockHeader.Write(er);
            er.Write((uint)(er.BaseStream.Position + 4));
            er.Write(Data, 0, Data.Length);
            er.EndChunk();
            er.EndChunk();
            byte[] data = m.ToArray();
            er.Close();
            return data;
        }

        private BinaryFileHeader  FileHeader;
        private BinaryBlockHeader BlockHeader;
        private uint              BaseOffset;
        public  byte[]            Data;
    }
}