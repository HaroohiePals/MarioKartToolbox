using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System.IO;

namespace HaroohiePals.Sound
{
    public class Wav
    {
        public const uint RiffSignature = 0x46464952;

        /// <summary>
        /// Generates a WAV file with the given <paramref name="data"/> and params.
        /// </summary>
        /// <param name="data">The raw sample Data.</param>
        /// <param name="sampleRate">The SampleRate in Hz.</param>
        /// <param name="bitsPerSample">The number of Bits per Sample.</param>
        /// <param name="nrChannel">The number of Channels.</param>
        public Wav(byte[] data, uint sampleRate, ushort bitsPerSample, ushort nrChannel)
        {
            Signature = RiffSignature;
            Wave      = new WaveData(data, sampleRate, bitsPerSample, nrChannel);
        }

        public Wav(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Wav(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                er.BeginChunk();
                er.ReadObject(this);
                er.EndChunk();
            }
        }

        public byte[] Write()
        {
            var m  = new MemoryStream();
            var er = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            er.BeginChunk();
            er.WriteObject(this);
            er.EndChunk();
            byte[] b = m.ToArray();
            er.Close();
            return b;
        }

        [Constant(RiffSignature)]
        public uint Signature;

        [ChunkSize(-8)]
        public uint FileSize;

        public WaveData Wave;

        public class WaveData
        {
            public const uint WaveSignature = 0x45564157;
            public const uint ListSignature = 0x5453494C;

            public WaveData(byte[] data, uint sampleRate, ushort bitsPerSample, ushort nrChannel)
            {
                Signature = WaveSignature;
                Fmt       = new FmtBlock(sampleRate, bitsPerSample, nrChannel);
                Data      = new DataBlock(data);
            }

            public WaveData(EndianBinaryReaderEx er)
            {
                Signature = er.ReadSignature(WaveSignature);
                Fmt       = new FmtBlock(er);
                uint sig    = er.Read<uint>();
                uint length = er.Read<uint>();
                er.BaseStream.Position -= 8;
                if (sig == ListSignature)
                    er.BaseStream.Position += length + 8;
                Data = new DataBlock(er);
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(WaveSignature);
                Fmt.Write(er);
                Data.Write(er);
                SMPL?.Write(er);
            }

            public uint     Signature;
            public FmtBlock Fmt;

            public class FmtBlock
            {
                public const uint FmtSignature = 0x20746D66;

                public enum WaveFormat : ushort
                {
                    WaveFormatPcm        = 0x0001,
                    IbmFormatAdpcm       = 0x0002,
                    IbmFormatMuLaw       = 0x0007,
                    IbmFormatALaw        = 0x0006,
                    WaveFormatExtensible = 0xFFFE
                }

                public FmtBlock(uint sampleRate, ushort bitsPerSample, ushort nrChannel)
                {
                    Signature     = FmtSignature;
                    SectionSize   = 16;
                    AudioFormat   = WaveFormat.WaveFormatPcm;
                    NrChannel     = nrChannel;
                    SampleRate    = sampleRate;
                    BitsPerSample = bitsPerSample;
                    ByteRate      = sampleRate * bitsPerSample * nrChannel / 8;
                    BlockAlign    = (ushort)(nrChannel * bitsPerSample / 8);
                }

                public FmtBlock(EndianBinaryReaderEx er)
                {
                    Signature     = er.ReadSignature(FmtSignature);
                    SectionSize   = er.Read<uint>();
                    AudioFormat   = (WaveFormat)er.Read<ushort>();
                    NrChannel     = er.Read<ushort>();
                    SampleRate    = er.Read<uint>();
                    ByteRate      = er.Read<uint>();
                    BlockAlign    = er.Read<ushort>();
                    BitsPerSample = er.Read<ushort>();
                }

                public void Write(EndianBinaryWriterEx er)
                {
                    er.BeginChunk();
                    er.WriteObject(this);
                    er.EndChunk();
                }

                [Constant(FmtSignature)]
                public uint Signature;

                [ChunkSize(-8)]
                public uint SectionSize;

                [Type(FieldType.U16)]
                public WaveFormat AudioFormat;

                public ushort NrChannel;
                public uint   SampleRate;
                public uint   ByteRate;
                public ushort BlockAlign;
                public ushort BitsPerSample;
            }

            public DataBlock Data;

            public class DataBlock
            {
                public const uint DataSignature = 0x61746164;

                public DataBlock(byte[] data)
                {
                    Signature   = DataSignature;
                    SectionSize = (uint)data.Length;
                    Data        = data;
                }

                public DataBlock(EndianBinaryReaderEx er)
                {
                    er.BeginChunk();
                    er.ReadObject(this);
                    er.EndChunk();
                }

                public void Write(EndianBinaryWriterEx er)
                {
                    er.BeginChunk();
                    er.WriteObject(this);
                    er.EndChunk();
                }

                [Constant(DataSignature)]
                public uint Signature;

                [ChunkSize(-8)]
                public uint SectionSize;

                [ArraySize(nameof(SectionSize))]
                public byte[] Data;
            }

            public SMPLBlock SMPL;

            public class SMPLBlock
            {
                public const uint SmplSignature = 0x6C706D73;

                public SMPLBlock(uint loopStart, uint loopEnd, uint baseKey = 60)
                {
                    Signature   = SmplSignature;
                    SectionSize = 0x3C;
                    BaseMidiKey = baseKey;
                    LoopCount   = 1;
                    Loops = new[]
                    {
                        new Loop()
                        {
                            Start = loopStart,
                            End   = loopEnd
                        }
                    };
                }

                public void Write(EndianBinaryWriterEx er)
                {
                    er.Write(SmplSignature);
                    er.Write((uint)(0x24 + Loops.Length * 0x18));
                    er.Write(Manufacturer);
                    er.Write(Product);
                    er.Write(SamplePeriod);
                    er.Write(BaseMidiKey);
                    er.Write(MidiPitchFraction);
                    er.Write(SMPTEFormat);
                    er.Write(SMPTEOffset);
                    er.Write((uint)Loops.Length);
                    er.Write(SamplerData);
                    foreach (var loop in Loops)
                        loop.Write(er);
                }

                public uint Signature;
                public uint SectionSize;
                public uint Manufacturer;
                public uint Product;
                public uint SamplePeriod;
                public uint BaseMidiKey;
                public uint MidiPitchFraction;
                public uint SMPTEFormat;
                public uint SMPTEOffset;
                public uint LoopCount;
                public uint SamplerData;

                public Loop[] Loops;

                public class Loop
                {
                    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

                    public uint CuePointId;
                    public uint Type;
                    public uint Start;
                    public uint End;
                    public uint Fraction;
                    public uint PlayCount;
                }
            }
        }

        public byte[] GetChannelData(int channel)
        {
            byte[] result = new byte[Wave.Data.Data.Length / Wave.Fmt.NrChannel];
            int    offs   = 0;
            for (int i = 0; i < Wave.Data.Data.Length; i += Wave.Fmt.NrChannel * Wave.Fmt.BitsPerSample / 8)
            {
                for (int j = 0; j < Wave.Fmt.BitsPerSample / 8; j++)
                {
                    result[offs++] = Wave.Data.Data[i + channel * Wave.Fmt.BitsPerSample / 8 + j];
                }
            }

            return result;
        }
    }
}