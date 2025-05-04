using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System;
using System.IO;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public class NdsRomOverlayTableEntry
{
    public const uint SIZE = 32;

    public NdsRomOverlayTableEntry() { }

    public NdsRomOverlayTableEntry(EndianBinaryReaderEx er)
    {
        er.ReadObject(this);
        uint tmp = er.Read<uint>();
        Compressed = tmp & 0xFFFFFF;
        Flag       = (NdsRomOverlayTableEntryFlags)(tmp >> 24);
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.WriteObject(this);
        er.Write(((uint)Flag & 0xFF) << 24 | (Compressed & 0xFFFFFF));
    }

    public byte[] Write()
    {
        using (var m = new MemoryStream())
        {
            var ew = new EndianBinaryWriterEx(m, Endianness.LittleEndian);
            Write(ew);
            ew.Close();
            return m.ToArray();
        }
    }

    [XmlAttribute]
    public uint Id;

    public uint RamAddress;
    public uint RamSize;
    public uint BssSize;
    public uint SinitInit;
    public uint SinitInitEnd;

    [XmlIgnore]
    public uint FileId;

    [Ignore]
    public uint Compressed; //:24;

    [XmlAttribute]
    [Ignore]
    public NdsRomOverlayTableEntryFlags Flag; // :8;
}
