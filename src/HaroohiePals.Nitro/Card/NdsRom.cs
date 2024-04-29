using HaroohiePals.IO;
using HaroohiePals.IO.Archive;
using HaroohiePals.Nitro.Fs;
using System;
using System.IO;
using System.Linq;

namespace HaroohiePals.Nitro.Card;

public sealed class NdsRom
{
    public NdsRom() { }

    public NdsRom(byte[] data)
        : this(new MemoryStream(data)) { }

    public NdsRom(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new NdsRomHeader(er);

            if (er.BaseStream.Length >= 0x4000)
            {
                er.BaseStream.Position = 0x1000;

                KeyPadding0 = er.Read<byte>(0x600);
                PTable = er.Read<uint>(Blowfish.P_TABLE_ENTRY_COUNT);
                KeyPadding1 = er.Read<byte>(0x5B8);

                SBoxes = new uint[Blowfish.S_BOX_COUNT][];
                for (int i = 0; i < Blowfish.S_BOX_COUNT; i++)
                    SBoxes[i] = er.Read<uint>(Blowfish.S_BOX_ENTRY_COUNT);

                KeyPadding2 = er.Read<byte>(0x400);
            }

            er.BaseStream.Position = Header.MainRomOffset;
            Arm9Binary = er.Read<byte>((int)Header.MainSize);
            if (er.Read<uint>() == 0xDEC00621) //Nitro Footer
            {
                er.BaseStream.Position -= 4;
                StaticFooter = new NitroFooter(er);
            }

            er.BaseStream.Position = Header.SubRomOffset;
            Arm7Binary = er.Read<byte>((int)Header.SubSize);

            er.BaseStream.Position = Header.FntOffset;
            Fnt = new NdsRomFileNameTable(er);

            er.BaseStream.Position = Header.MainOvtOffset;
            Arm9OverlayTable = new NdsRomOverlayTable[Header.MainOvtSize / 32];
            for (int i = 0; i < Header.MainOvtSize / 32; i++) Arm9OverlayTable[i] = new NdsRomOverlayTable(er);

            er.BaseStream.Position = Header.SubOvtOffset;
            Arm7OverlayTable = new NdsRomOverlayTable[Header.SubOvtSize / 32];
            for (int i = 0; i < Header.SubOvtSize / 32; i++) Arm7OverlayTable[i] = new NdsRomOverlayTable(er);

            er.BaseStream.Position = Header.FatOffset;
            Fat = new FatEntry[Header.FatSize / 8];
            for (int i = 0; i < Header.FatSize / 8; i++)
                Fat[i] = new FatEntry(er);

            if (Header.BannerOffset != 0)
            {
                er.BaseStream.Position = Header.BannerOffset;
                Banner = new NdsRomBanner(er);
            }

            FileData = new byte[Header.FatSize / 8][];
            for (int i = 0; i < Header.FatSize / 8; i++)
            {
                er.BaseStream.Position = Fat[i].FileTop;
                FileData[i] = er.Read<byte>((int)Fat[i].FileSize);
            }

            //RSA Signature
            if (Header.RomSize + 0x88 <= er.BaseStream.Length)
            {
                er.BaseStream.Position = Header.RomSize;
                byte[] rsaSig = er.Read<byte>(0x88);
                if (rsaSig[0] == 'a' && rsaSig[1] == 'c')
                {
                    RsaSignature = rsaSig;
                }
            }
        }
    }

    public byte[] Write(bool trimmed = true)
    {
        using (var m = new MemoryStream())
        {
            Write(m, trimmed);

            return m.ToArray();
        }
    }

    public void Write(Stream stream, bool trimmed = true)
    {
        using (var er = new EndianBinaryWriterEx(stream, Endianness.LittleEndian))
        {
            //Header
            //skip the header, and write it afterwards

            WriteBlowfish(er);

            er.BaseStream.Position = 0x4000;
            Header.HeaderSize = (uint)er.BaseStream.Position;
            //MainRom
            Header.MainRomOffset = (uint)er.BaseStream.Position;
            Header.MainSize = (uint)Arm9Binary.Length;
            er.Write(Arm9Binary, 0, Arm9Binary.Length);
            StaticFooter?.Write(er);
            WriteArm9OverlayTable(er);

            er.WritePadding(0x200, 0xFF);
            //SubRom
            Header.SubRomOffset = (uint)er.BaseStream.Position;
            Header.SubSize = (uint)Arm7Binary.Length;
            er.Write(Arm7Binary, 0, Arm7Binary.Length);
            WriteArm7OverlayTable(er);

            er.WritePadding(0x200, 0xFF);
            //FNT
            Header.FntOffset = (uint)er.BaseStream.Position;
            Fnt.Write(er);
            Header.FntSize = (uint)er.BaseStream.Position - Header.FntOffset;
            er.WritePadding(0x200, 0xFF);
            //FAT
            Header.FatOffset = (uint)er.BaseStream.Position;
            Header.FatSize = (uint)Fat.Length * 8;
            //Skip the fat, and write it after writing the data itself
            er.BaseStream.Position += Header.FatSize;
            WriteBanner(er);
            WriteFiles(er);

            er.WritePadding(4);
            Header.RomSize = (uint)er.BaseStream.Position;
            uint capacitySize = Header.RomSize;
            capacitySize |= capacitySize >> 16;
            capacitySize |= capacitySize >> 8;
            capacitySize |= capacitySize >> 4;
            capacitySize |= capacitySize >> 2;
            capacitySize |= capacitySize >> 1;
            capacitySize++;
            if (capacitySize < 0x20000)
            {
                capacitySize = 0x20000;
            }

            uint capacitySize2 = capacitySize;
            int capacity = -18;
            while (capacitySize2 != 0)
            {
                capacitySize2 >>= 1;
                capacity++;
            }

            Header.DeviceSize = (byte)((capacity < 0) ? 0 : capacity);
            //RSA
            if (RsaSignature != null)
            {
                er.Write(RsaSignature, 0, 0x88);
            }

            //if writing untrimmed write padding up to the power of 2 size of the rom
            if (!trimmed)
            {
                er.WritePadding((int)capacitySize, 0xFF);
            }

            //Fat
            er.BaseStream.Position = Header.FatOffset;
            foreach (var fatEntry in Fat)
            {
                fatEntry.Write(er);
            }
            //Header
            er.BaseStream.Position = 0;
            Header.Write(er);
        }
    }

    private void WriteBlowfish(EndianBinaryWriterEx er)
    {
        if (PTable is null || SBoxes is null || PTable.All(p => p == 0))
        {
            return;
        }

        er.BaseStream.Position = 0x1000;
        if (KeyPadding0 is not null)
        {
            if (KeyPadding0.Length != 0x600)
            {
                throw new Exception();
            }

            er.Write(KeyPadding0);
        }
        else
        {
            er.BaseStream.Position += 0x600;
        }

        if (PTable.Length != Blowfish.P_TABLE_ENTRY_COUNT)
        {
            throw new Exception();
        }

        er.Write(PTable);

        if (KeyPadding1 is not null)
        {
            if (KeyPadding1.Length != 0x5B8)
                throw new Exception();
            er.Write(KeyPadding1);
        }
        else
        {
            er.BaseStream.Position += 0x5B8;
        }

        if (SBoxes.Length != Blowfish.S_BOX_COUNT)
        {
            throw new Exception();
        }

        for (int i = 0; i < Blowfish.S_BOX_COUNT; i++)
        {
            if (SBoxes[i] is null || SBoxes[i].Length != Blowfish.S_BOX_ENTRY_COUNT)
            {
                throw new Exception();
            }

            er.Write(SBoxes[i]);
        }

        if (KeyPadding2 is not null)
        {
            if (KeyPadding2.Length != 0x400)
            {
                throw new Exception();
            }

            er.Write(KeyPadding2);
        }
        else
        {
            er.BaseStream.Position += 0x400;
        }

        WriteTestPatterns(er);
    }

    private void WriteTestPatterns(EndianBinaryWriterEx writer)
    {
        writer.Write(new byte[] { 0xFF, 0x00, 0xFF, 0x00, 0xAA, 0x55, 0xAA, 0x55 });

        for (int i = 8; i < 0x200; i++)
            writer.Write((byte)(i & 0xFF));

        for (int i = 0; i < 0x200; i++)
            writer.Write((byte)(0xFF - (i & 0xFF)));

        for (int i = 0; i < 0x200; i++)
            writer.Write((byte)0);

        for (int i = 0; i < 0x200; i++)
            writer.Write((byte)0xFF);

        for (int i = 0; i < 0x200; i++)
            writer.Write((byte)0x0F);

        for (int i = 0; i < 0x200; i++)
            writer.Write((byte)0xF0);

        for (int i = 0; i < 0x200; i++)
            writer.Write((byte)0x55);

        for (int i = 0; i < 0x1FF; i++)
            writer.Write((byte)0xAA);

        writer.Write((byte)0);
    }

    private void WriteArm9OverlayTable(EndianBinaryWriterEx er)
    {
        if (Arm9OverlayTable.Length == 0)
        {
            Header.MainOvtOffset = 0;
            Header.MainOvtSize = 0;
            return;
        }

        er.WritePadding(0x200);
        Header.MainOvtOffset = (uint)er.BaseStream.Position;
        Header.MainOvtSize = (uint)Arm9OverlayTable.Length * 0x20;
        foreach (var v in Arm9OverlayTable)
        {
            v.Write(er);
        }

        foreach (var v in Arm9OverlayTable)
        {
            er.WritePadding(0x200);
            Fat[v.FileId].FileTop = (uint)er.BaseStream.Position;
            Fat[v.FileId].FileBottom = (uint)er.BaseStream.Position + (uint)FileData[v.FileId].Length;
            er.Write(FileData[v.FileId], 0, FileData[v.FileId].Length);
        }
    }

    private void WriteArm7OverlayTable(EndianBinaryWriterEx er)
    {
        //I assume this works the same as the arm9 ovt?
        if (Arm7OverlayTable.Length == 0)
        {
            Header.SubOvtOffset = 0;
            Header.SubOvtSize = 0;
            return;
        }

        er.WritePadding(0x200);
        Header.SubOvtOffset = (uint)er.BaseStream.Position;
        Header.SubOvtSize = (uint)Arm7OverlayTable.Length * 0x20;
        foreach (var v in Arm7OverlayTable)
        {
            v.Write(er);
        }

        foreach (var v in Arm7OverlayTable)
        {
            er.WritePadding(0x200);
            Fat[v.FileId].FileTop = (uint)er.BaseStream.Position;
            Fat[v.FileId].FileBottom = (uint)er.BaseStream.Position + (uint)FileData[v.FileId].Length;
            er.Write(FileData[v.FileId], 0, FileData[v.FileId].Length);
        }
    }

    private void WriteBanner(EndianBinaryWriterEx er)
    {
        if (Banner != null)
        {
            er.WritePadding(0x200, 0xFF);
            Header.BannerOffset = (uint)er.BaseStream.Position;
            Banner.Write(er);
        }
        else
        {
            Header.BannerOffset = 0;
        }
    }

    private void WriteFiles(EndianBinaryWriterEx er)
    {
        for (int i = (int)(Header.MainOvtSize / 32 + Header.SubOvtSize / 32); i < FileData.Length; i++)
        {
            er.WritePadding(0x200, 0xFF);
            Fat[i].FileTop = (uint)er.BaseStream.Position;
            Fat[i].FileBottom = (uint)er.BaseStream.Position + (uint)FileData[i].Length;
            er.Write(FileData[i], 0, FileData[i].Length);
        }
    }

    public NdsRomHeader Header;

    public byte[] KeyPadding0;
    public uint[] PTable;
    public byte[] KeyPadding1;
    public uint[][] SBoxes;
    public byte[] KeyPadding2;

    public byte[] Arm9Binary;
    public NitroFooter StaticFooter;

    public byte[] Arm7Binary;
    public NdsRomFileNameTable Fnt;

    public NdsRomOverlayTable[] Arm9OverlayTable;
    public NdsRomOverlayTable[] Arm7OverlayTable;

    public FatEntry[] Fat;
    public NdsRomBanner Banner;

    public byte[][] FileData;

    public byte[] RsaSignature;

    //      public byte[] GetDecompressedARM9()
    //      {
    //          //if (StaticFooter != null) return ARM9.Decompress(MainRom, StaticFooter._start_ModuleParamsOffset);
    //          /*else*/
    //          return Arm9.Decompress(MainRom);
    //      }

    public NitroFsArchive ToArchive()
    {
        return new NitroFsArchive(Fnt.DirectoryTable, Fnt.NameTable, FileData);
    }

    public void FromArchive(Archive archive)
    {
        int nrOverlays = Arm9OverlayTable.Length + Arm7OverlayTable.Length;

        var nitroArc = new NitroFsArchive(archive, (ushort)nrOverlays);
        Fnt.DirectoryTable = nitroArc.DirTable;
        Fnt.NameTable = nitroArc.NameTable;

        int nrFiles = nitroArc.FileData.Length;

        var fat = new FatEntry[nrOverlays + nrFiles];
        Array.Copy(Fat, fat, nrOverlays);
        Fat = fat;

        var fileData = new byte[nrOverlays + nrFiles][];
        Array.Copy(FileData, fileData, nrOverlays);
        FileData = fileData;

        for (int i = nrOverlays; i < nrFiles + nrOverlays; i++)
        {
            Fat[i] = new FatEntry(0, 0);
            FileData[i] = nitroArc.FileData[i - nitroArc.FileIdOffset];
        }
    }
}