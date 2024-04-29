using HaroohiePals.IO;
using System;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public sealed class NitroFooter
{
    public NitroFooter() { }

    public NitroFooter(EndianBinaryReaderEx er) => er.ReadObject(this);
    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

    public uint NitroCode;
    public uint _start_ModuleParamsOffset;
    public uint Unknown;
}
