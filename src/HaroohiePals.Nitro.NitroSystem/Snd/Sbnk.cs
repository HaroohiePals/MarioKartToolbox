using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.Snd.Player;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.Snd
{
    public class Sbnk
    {
        public const uint SbnkSignature = 0x4B4E4253;
        public const uint DataSignature = 0x41544144;

        public Sbnk(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Sbnk(Stream stream)
        {
            using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
            {
                FileHeader = new BinaryFileHeader(er);
                if (FileHeader.Signature != SbnkSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, SbnkSignature, 0);
                BlockHeader = new BinaryBlockHeader(er);
                if (BlockHeader.Kind != DataSignature)
                    throw new SignatureNotCorrectException(FileHeader.Signature, DataSignature, 0x10);
                er.Read<byte>(0x20); //WaveArcLink is not in the file
                NrInstruments = er.Read<uint>();
                Instruments   = new Instrument[NrInstruments];
                for (int i = 0; i < NrInstruments; i++)
                {
                    Instruments[i] = new Instrument(er);
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
                    er.Write(new byte[0x20], 0, 0x20);
                    er.Write((uint)Instruments.Length);
                    long offspos = er.BaseStream.Position;
                    for (int i = 0; i < Instruments.Length; i++)
                    {
                        Instruments[i].Write(er);
                    }

                    for (int i = 0; i < Instruments.Length; i++)
                    {
                        if (Instruments[i].Type != 0)
                        {
                            long curpos = er.BaseStream.Position;
                            er.BaseStream.Position = offspos + 4 * i + 1;
                            er.Write((ushort)curpos);
                            er.BaseStream.Position = curpos;
                            Instruments[i].Param.Write(er);
                        }
                    }

                    er.WritePadding(4);
                }
                er.EndChunk();
            }
            er.EndChunk();
            byte[] data = m.ToArray();
            er.Close();
            return data;
        }

        private  BinaryFileHeader  FileHeader;
        private  BinaryBlockHeader BlockHeader;
        internal WaveArcLink[]     WaveArcLink = new WaveArcLink[4]; //4
        public   uint              NrInstruments;
        public   Instrument[]      Instruments;

        public void AssignWaveArc(int index, Swar waveArc)
        {
            if (WaveArcLink[index] == null) WaveArcLink[index] = new WaveArcLink();
            if (WaveArcLink[index].WaveArc != null)
            {
                if (WaveArcLink[index].WaveArc == waveArc) return;
                if (WaveArcLink[index] == WaveArcLink[index].WaveArc.TopLink)
                    WaveArcLink[index].WaveArc.TopLink = WaveArcLink[index].Next;
                else
                {
                    var prev = WaveArcLink[index].WaveArc.TopLink;
                    while (prev != null)
                    {
                        if (WaveArcLink[index] == prev.Next)
                            break;
                        prev = prev.Next;
                    }

                    prev.Next = WaveArcLink[index].Next;
                }
            }

            var next = waveArc.TopLink;
            waveArc.TopLink            = WaveArcLink[index];
            WaveArcLink[index].Next    = next;
            WaveArcLink[index].WaveArc = waveArc;
        }

        public InstData ReadInstData(int prgNo, int key)
        {
            if (prgNo < 0)
                return null;
            if (prgNo >= NrInstruments)
                return null;
            var result = new InstData();
            result.Type = Instruments[prgNo].Type;
            switch (result.Type)
            {
                case InstData.InstType.Pcm:
                case InstData.InstType.Psg:
                case InstData.InstType.Noise:
                case InstData.InstType.DirectPcm:
                case InstData.InstType.Null:
                {
                    result.Param.Wave = new ushort[2];
                    var param = (SimpleInstrumentParam)Instruments[prgNo].Param;
                    result.Param.Wave[0]     = param.Param.Wave[0];
                    result.Param.Wave[1]     = param.Param.Wave[1];
                    result.Param.OriginalKey = param.Param.OriginalKey;
                    result.Param.Attack      = param.Param.Attack;
                    result.Param.Decay       = param.Param.Decay;
                    result.Param.Sustain     = param.Param.Sustain;
                    result.Param.Release     = param.Param.Release;
                    result.Param.Pan         = param.Param.Pan;
                    return result;
                }
                case InstData.InstType.DrumSet:
                {
                    if (key < ((DrumSetParam)Instruments[prgNo].Param).Min ||
                        key > ((DrumSetParam)Instruments[prgNo].Param).Max)
                        return null;
                    var param =
                        ((DrumSetParam)Instruments[prgNo].Param).SubInstruments[
                            key - ((DrumSetParam)Instruments[prgNo].Param).Min];
                    result.Type              = param.Type;
                    result.Param.Wave        = new ushort[2];
                    result.Param.Wave[0]     = param.Param.Wave[0];
                    result.Param.Wave[1]     = param.Param.Wave[1];
                    result.Param.OriginalKey = param.Param.OriginalKey;
                    result.Param.Attack      = param.Param.Attack;
                    result.Param.Decay       = param.Param.Decay;
                    result.Param.Sustain     = param.Param.Sustain;
                    result.Param.Release     = param.Param.Release;
                    result.Param.Pan         = param.Param.Pan;
                    return result;
                }
                case InstData.InstType.KeySplit:
                {
                    int i = 0;
                    while (key > ((KeySplitParam)Instruments[prgNo].Param).Key[i])
                    {
                        i++;
                        if (i >= 8)
                            return null;
                    }

                    var param = ((KeySplitParam)Instruments[prgNo].Param).SubInstruments[i];
                    result.Type              = param.Type;
                    result.Param.Wave        = new ushort[2];
                    result.Param.Wave[0]     = param.Param.Wave[0];
                    result.Param.Wave[1]     = param.Param.Wave[1];
                    result.Param.OriginalKey = param.Param.OriginalKey;
                    result.Param.Attack      = param.Param.Attack;
                    result.Param.Decay       = param.Param.Decay;
                    result.Param.Sustain     = param.Param.Sustain;
                    result.Param.Release     = param.Param.Release;
                    result.Param.Pan         = param.Param.Pan;
                    return result;
                }
                default: return null;
            }
        }

        internal WaveData GetWaveData(int waveArc, int wave)
        {
            if (WaveArcLink[waveArc] != null && WaveArcLink[waveArc].WaveArc != null &&
                wave < WaveArcLink[waveArc].WaveArc.WaveCount)
                return WaveArcLink[waveArc].WaveArc.GetWaveDataAddress(wave);
            return null;
        }

        public class Instrument
        {
            public Instrument(EndianBinaryReaderEx er)
            {
                Type   = er.Read<InstData.InstType>();
                Offset = er.Read<ushort>();
                er.Read<byte>(); //alignment
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = Offset;
                if ((int)Type > 0 && (int)Type < 16)
                    Param = new SimpleInstrumentParam(er);
                else if (Type == InstData.InstType.DrumSet)
                    Param = new DrumSetParam(er);
                else if (Type == InstData.InstType.KeySplit)
                    Param = new KeySplitParam(er);
                er.BaseStream.Position = curpos;
            }

            public void Write(EndianBinaryWriterEx er)
            {
                er.Write(Type);
                er.Write<ushort>(0);
                er.Write<byte>(0);
            }

            public InstData.InstType Type;
            public ushort            Offset;

            public InstrumentParam Param;
        }

        public abstract class InstrumentParam
        {
            public abstract void Write(EndianBinaryWriterEx er);
        }

        public class SimpleInstrumentParam : InstrumentParam
        {
            public SimpleInstrumentParam() { }

            public SimpleInstrumentParam(EndianBinaryReaderEx er)
            {
                Param = new InstData.InstParam(er);
            }

            public override void Write(EndianBinaryWriterEx er)
            {
                Param.Write(er);
            }

            public InstData.InstParam Param;
        }

        public class DrumSetParam : InstrumentParam
        {
            public DrumSetParam() { }

            public DrumSetParam(EndianBinaryReaderEx er)
            {
                Min            = er.Read<byte>();
                Max            = er.Read<byte>();
                SubInstruments = new InstData[Max - Min + 1];
                for (int i = 0; i < SubInstruments.Length; i++)
                    SubInstruments[i] = new InstData(er);
            }

            public override void Write(EndianBinaryWriterEx er)
            {
                er.Write(Min);
                er.Write(Max);
                for (int i = 0; i < SubInstruments.Length; i++)
                    SubInstruments[i].Write(er);
            }

            public byte       Min;
            public byte       Max;
            public InstData[] SubInstruments;
        }

        public class KeySplitParam : InstrumentParam
        {
            public KeySplitParam() { }

            public KeySplitParam(EndianBinaryReaderEx er)
            {
                Key = er.Read<byte>(8);
                int nr = 0;
                for (int i = 0; i < 8; i++)
                {
                    nr++;
                    if (Key[i] == 0)
                        break;
                }

                SubInstruments = new InstData[nr];
                for (int i = 0; i < nr; i++)
                    SubInstruments[i] = new InstData(er);
            }

            public override void Write(EndianBinaryWriterEx er)
            {
                er.Write(Key, 0, 8);
                for (int i = 0; i < SubInstruments.Length; i++)
                    SubInstruments[i].Write(er);
            }

            public byte[]     Key; //8
            public InstData[] SubInstruments;
        }
    }
}