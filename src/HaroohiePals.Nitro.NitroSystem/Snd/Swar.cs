using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.Snd.Player;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Swar
    {
        public const uint SwarSignature = 0x52415753;
        public const uint DataSignature = 0x41544144;

        public Swar()
        {
            FileHeader  = new BinaryFileHeader(SwarSignature, 1);
            BlockHeader = new BinaryBlockHeader(DataSignature);
            WaveCount   = 0;
            WaveOffset  = new uint[0];
            Waves       = new WaveData[0];
        }

        public Swar(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Swar(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                FileHeader = new BinaryFileHeader(er);
                if (FileHeader.Signature != SwarSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, SwarSignature, 0);
                BlockHeader = new BinaryBlockHeader(er);
                if (BlockHeader.Kind != DataSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, DataSignature, 0x10);
                er.Read<uint>(); //TopLink is not in the file
                er.Read<uint>(7);
                WaveCount  = er.Read<uint>();
                WaveOffset = er.Read<uint>((int)WaveCount);
                Waves      = new WaveData[WaveCount];
                for (int i = 0; i < WaveCount; i++)
                {
                    er.BaseStream.Position = WaveOffset[i];
                    Waves[i]               = new WaveData(er);
                }
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk(8);
            {
                FileHeader.FileSize   = 0;
                FileHeader.DataBlocks = 1;
                FileHeader.Write(er);
                er.BeginChunk(4);
                {
                    BlockHeader.Size = 0;
                    BlockHeader.Write(er);
                    er.Write((uint)0);
                    er.Write(new uint[7]);
                    er.Write((uint)Waves.Length);
                    long offspos = er.BaseStream.Position;
                    er.Write(new uint[Waves.Length]);
                    for (int i = 0; i < Waves.Length; i++)
                    {
                        long curpos = er.BaseStream.Position;
                        er.BaseStream.Position = offspos + 4 * i;
                        er.Write((uint)curpos);
                        er.BaseStream.Position = curpos;
                        Waves[i].Write(er);
                    }
                }
                er.EndChunk();
            }
            er.EndChunk();
            byte[] result = m.ToArray();
            er.Close();
            return result;
        }

        private  BinaryFileHeader  FileHeader;
        private  BinaryBlockHeader BlockHeader;
        internal WaveArcLink       TopLink;
        public   uint              WaveCount;
        public   uint[]            WaveOffset;

        public WaveData[] Waves;

        public WaveData GetWaveDataAddress(int wave)
        {
            return Waves[wave];
        }
    }
}