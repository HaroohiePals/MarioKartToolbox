using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public sealed class NdsBannerHeader
{
    public byte Version;
    public byte ReservedA;

    [XmlIgnore]
    public ushort CRC16_v1;

    [ArraySize(28)]
    public byte[] ReservedB;

    public NdsBannerHeader() { }

    public NdsBannerHeader(EndianBinaryReaderEx er)
    {
        er.ReadObject(this);
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.WriteObject(this);
    }
}