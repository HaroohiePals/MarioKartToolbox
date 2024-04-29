using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;

namespace HaroohiePals.Nitro.NitroSystem.Snd.Player
{
    public class InstData
    {
        public enum InstType : byte
        {
            Invalid   = 0,
            Pcm       = 1,
            Psg       = 2,
            Noise     = 3,
            DirectPcm = 4,
            Null      = 5,
            DrumSet   = 0x10,
            KeySplit  = 0x11
        }

        public InstData() { }
        public InstData(EndianBinaryReaderEx er) => er.ReadObject(this);
        public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

        [Type(FieldType.U8)]
        public InstType  Type;

        [Align(2)]
        public InstParam Param = new();

        public class InstParam
        {
            public InstParam() { }
            public InstParam(EndianBinaryReaderEx er) => er.ReadObject(this);
            public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

            [ArraySize(2)]
            public ushort[] Wave;

            public byte OriginalKey;
            public byte Attack;
            public byte Decay;
            public byte Sustain;
            public byte Release;
            public byte Pan;
        }
    }
}