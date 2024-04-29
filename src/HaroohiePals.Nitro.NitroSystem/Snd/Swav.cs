using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.Snd.Player;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Swav
    {
        public const uint SwavSignature = 0x56415753;
        public const uint DataSignature = 0x41544144;

        public Swav(WaveData data)
        {
            FileHeader  = new BinaryFileHeader(SwavSignature, 1);
            BlockHeader = new BinaryBlockHeader(DataSignature);
            WaveData    = data;
        }

        public Swav(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Swav(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                FileHeader = new BinaryFileHeader(er);
                if (FileHeader.Signature != SwavSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, SwavSignature, 0);
                BlockHeader = new BinaryBlockHeader(er);
                if (BlockHeader.Kind != DataSignature)
                    throw new SignatureNotCorrectException(BlockHeader.Kind, DataSignature, 0x10);
                WaveData = new WaveData(er);
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
            WaveData.Write(er);
            er.EndChunk();
            er.EndChunk();
            byte[] data = m.ToArray();
            er.Close();
            return data;
        }

        private BinaryFileHeader  FileHeader;
        private BinaryBlockHeader BlockHeader;
        public  WaveData          WaveData;
    }
}