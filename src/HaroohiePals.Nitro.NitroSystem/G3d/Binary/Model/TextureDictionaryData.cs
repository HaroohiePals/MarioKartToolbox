using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using HaroohiePals.Nitro.G3;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class TextureDictionaryData : IG3dDictionaryData
{
    public static ushort DataSize => 8;

    public const uint ParamExOrigWMask = 0x000007ff;
    public const uint ParamExOrigHMask = 0x003ff800;
    public const uint ParamExWHSameMask = 0x80000000;

    public const int ParamExOrigWShift = 0;
    public const int ParamExOrigHShift = 11;
    public const int ParamExWHSameShift = 31;

    public void Read(EndianBinaryReaderEx er) => er.ReadObject(this);
    public void Write(EndianBinaryWriterEx er) => er.WriteObject(this);

    [Type(FieldType.U32)]
    public GxTexImageParam TexImageParam;

    public uint ExtraParam;
}