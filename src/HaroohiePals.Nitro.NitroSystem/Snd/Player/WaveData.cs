using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.Snd;

namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class WaveData
    {
        public WaveData()
        {
            Param = new WaveParam();
        }

        public WaveData(EndianBinaryReaderEx er)
        {
            Param = new WaveParam(er);
            uint length = (uint)(Param.LoopStart * 4 + Param.LoopLen * 4);
            Samples = er.Read<byte>((int)length);
        }

        public void Write(EndianBinaryWriterEx er)
        {
            Param.Write(er);
            er.Write(Samples, 0, Samples.Length);
            er.WritePadding(4);
        }

        public WaveParam Param;
        public byte[]    Samples;

        public class WaveParam
        {
            public WaveParam() { }
            public WaveParam(EndianBinaryReaderEx er) => er.ReadObject(this);
            public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [Type(FieldType.U8)]
            public NitroChannel.SoundFormat Format;

            [Type(FieldType.U8)]
            public bool Loop;

            public ushort Rate;
            public ushort Timer;
            public ushort LoopStart;
            public uint   LoopLen;
        }
    }
}