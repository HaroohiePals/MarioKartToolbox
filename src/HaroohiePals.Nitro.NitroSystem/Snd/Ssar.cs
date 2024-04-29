using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Ssar
    {
        public const uint SsarSignature = 0x52415353;
        public const uint DataSignature = 0x41544144;

        public Ssar(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Ssar(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                FileHeader = new BinaryFileHeader(er);
                if (FileHeader.Signature != SsarSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, SsarSignature, 0);
                BlockHeader = new BinaryBlockHeader(er);
                if (BlockHeader.Kind != DataSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, DataSignature, 0x10);
                BaseOffset  = er.Read<uint>();
                NrSequences = er.Read<uint>();
                Sequences   = new SequenceInfo[NrSequences];
                for (int i = 0; i < NrSequences; i++)
                    Sequences[i] = new SequenceInfo(er);

                er.BaseStream.Position = BaseOffset;
                Data                   = er.Read<byte>((int)(er.BaseStream.Length - er.BaseStream.Position));
            }
        }

        private BinaryFileHeader  FileHeader;
        private BinaryBlockHeader BlockHeader;
        public  uint              BaseOffset;
        public  uint              NrSequences;
        public  SequenceInfo[]    Sequences;
        public  byte[]            Data;

        public class SequenceInfo
        {
            public SequenceInfo(EndianBinaryReaderEx er) => er.ReadObject(this);

            public uint   Offset;
            public ushort Bank;
            public byte   Volume;
            public byte   ChannelPriority;
            public byte   PlayerPriority;
            public byte   PlayerNr;
            public ushort Reserved;
        }
    }
}