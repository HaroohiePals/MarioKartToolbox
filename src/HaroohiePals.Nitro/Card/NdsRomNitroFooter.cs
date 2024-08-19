using HaroohiePals.IO;
using System;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public sealed class NdsRomNitroFooter
{
    public NdsRomNitroFooter() { }

    public NdsRomNitroFooter(EndianBinaryReaderEx er) => er.ReadObject(this);
    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

    public uint NitroCode;
    public uint ModuleParamsOffset;
    public uint Unknown;
}
