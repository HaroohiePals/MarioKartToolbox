using HaroohiePals.IO;
using HaroohiePals.IO.Serialization;
using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.Card;

[Serializable]
public sealed class NdsRomHeader
{
    public NdsRomHeader() { }

    public NdsRomHeader(EndianBinaryReader er)
    {
        GameName    = er.ReadString(Encoding.ASCII, 12).TrimEnd('\0');
        GameCode    = er.ReadString(Encoding.ASCII, 4).TrimEnd('\0');
        MakerCode   = er.ReadString(Encoding.ASCII, 2).TrimEnd('\0');
        ProductId   = er.Read<byte>();
        DeviceType  = er.Read<byte>();
        DeviceSize  = er.Read<byte>();
        ReservedA   = er.Read<byte>(9);
        GameVersion = er.Read<byte>();
        Property    = er.Read<byte>();

        MainRomOffset    = er.Read<uint>();
        MainEntryAddress = er.Read<uint>();
        MainRamAddress   = er.Read<uint>();
        MainSize         = er.Read<uint>();
        SubRomOffset     = er.Read<uint>();
        SubEntryAddress  = er.Read<uint>();
        SubRamAddress    = er.Read<uint>();
        SubSize          = er.Read<uint>();

        FntOffset = er.Read<uint>();
        FntSize   = er.Read<uint>();

        FatOffset = er.Read<uint>();
        FatSize   = er.Read<uint>();

        MainOvtOffset = er.Read<uint>();
        MainOvtSize   = er.Read<uint>();

        SubOvtOffset = er.Read<uint>();
        SubOvtSize   = er.Read<uint>();

        RomParamA    = er.Read<byte>(8);
        BannerOffset = er.Read<uint>();
        SecureCRC    = er.Read<ushort>();
        RomParamB    = er.Read<byte>(2);

        MainAutoloadDone = er.Read<uint>();
        SubAutoloadDone  = er.Read<uint>();

        RomParamC  = er.Read<byte>(8);
        RomSize    = er.Read<uint>();
        HeaderSize = er.Read<uint>();
        ReservedB  = er.Read<byte>(0x38);

        LogoData  = er.Read<byte>(0x9C);
        LogoCRC   = er.Read<ushort>();
        HeaderCRC = er.Read<ushort>();
    }

    public void Write(EndianBinaryWriter er)
    {
        var    m = new MemoryStream();
        byte[] header;
        using (var ew = new EndianBinaryWriter(m, Endianness.LittleEndian))
        {
            ew.Write(GameName.PadRight(12, '\0')[..12], Encoding.ASCII, false);
            ew.Write(GameCode.PadRight(4, '\0')[..4], Encoding.ASCII, false);
            ew.Write(MakerCode.PadRight(2, '\0')[..2], Encoding.ASCII, false);
            ew.Write(ProductId);
            ew.Write(DeviceType);
            ew.Write(DeviceSize);
            ew.Write(ReservedA, 0, 9);
            ew.Write(GameVersion);
            ew.Write(Property);

            ew.Write(MainRomOffset);
            ew.Write(MainEntryAddress);
            ew.Write(MainRamAddress);
            ew.Write(MainSize);
            ew.Write(SubRomOffset);
            ew.Write(SubEntryAddress);
            ew.Write(SubRamAddress);
            ew.Write(SubSize);

            ew.Write(FntOffset);
            ew.Write(FntSize);

            ew.Write(FatOffset);
            ew.Write(FatSize);

            ew.Write(MainOvtOffset);
            ew.Write(MainOvtSize);

            ew.Write(SubOvtOffset);
            ew.Write(SubOvtSize);

            ew.Write(RomParamA, 0, 8);
            ew.Write(BannerOffset);
            ew.Write(SecureCRC);
            ew.Write(RomParamB, 0, 2);

            ew.Write(MainAutoloadDone);
            ew.Write(SubAutoloadDone);

            ew.Write(RomParamC, 0, 8);
            ew.Write(RomSize);
            ew.Write(HeaderSize);
            ew.Write(ReservedB, 0, 0x38);

            ew.Write(LogoData, 0, 0x9C);
            LogoCRC = Crc16.GetCrc16(LogoData);
            ew.Write(LogoCRC);

            header = m.ToArray();
        }

        HeaderCRC = Crc16.GetCrc16(header);

        er.Write(header);
        er.Write(HeaderCRC);
    }

    public string GameName;  //12
    public string GameCode;  //4
    public string MakerCode; //2
    public byte   ProductId;
    public byte   DeviceType;
    public byte   DeviceSize;

    [ArraySize(9)]
    public byte[] ReservedA;

    public byte GameVersion;
    public byte Property;

    [XmlIgnore]
    public uint MainRomOffset;

    public uint MainEntryAddress;
    public uint MainRamAddress;

    [XmlIgnore]
    public uint MainSize;

    [XmlIgnore]
    public uint SubRomOffset;

    public uint SubEntryAddress;
    public uint SubRamAddress;

    [XmlIgnore]
    public uint SubSize;

    [XmlIgnore]
    public uint FntOffset;

    [XmlIgnore]
    public uint FntSize;

    [XmlIgnore]
    public uint FatOffset;

    [XmlIgnore]
    public uint FatSize;

    [XmlIgnore]
    public uint MainOvtOffset;

    [XmlIgnore]
    public uint MainOvtSize;

    [XmlIgnore]
    public uint SubOvtOffset;

    [XmlIgnore]
    public uint SubOvtSize;

    [ArraySize(8)]
    public byte[] RomParamA;

    [XmlIgnore]
    public uint BannerOffset;

    public ushort SecureCRC;

    [ArraySize(2)]
    public byte[] RomParamB;

    public uint MainAutoloadDone;
    public uint SubAutoloadDone;

    [ArraySize(8)]
    public byte[] RomParamC; //8

    [XmlIgnore]
    public uint RomSize;

    [XmlIgnore]
    public uint HeaderSize;

    [ArraySize(0x38)]
    public byte[] ReservedB;

    [ArraySize(0x9C)]
    public byte[] LogoData;

    [XmlIgnore]
    public ushort LogoCRC;

    [XmlIgnore]
    public ushort HeaderCRC;
}