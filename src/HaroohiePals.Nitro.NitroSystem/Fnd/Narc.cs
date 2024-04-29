using HaroohiePals.IO;
using HaroohiePals.IO.Archive;
using HaroohiePals.Nitro.Fs;
using System.IO;
using System.Linq;

namespace HaroohiePals.Nitro.NitroSystem.Fnd;

public sealed class Narc
{
    public const uint NarcSignature = 0x4352414E;

    public Narc(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Narc(Stream stream)
    {
        using (var er = new EndianBinaryReaderEx(stream, Endianness.LittleEndian))
        {
            Header = new BinaryFileHeader(er);
            if (Header.Signature != NarcSignature)
                throw new SignatureNotCorrectException(Header.Signature, NarcSignature, 0);
            FileAllocationTable   = new NarcFileAllocationTable(er);
            FileNameTable   = new NarcFileNameTable(er);
            FileImageBlock = new NarcFileImageBlock(er);
        }
    }

    public Narc(Archive archive, bool hasFnt = true)
    {
        var nitroFsArc = new NitroFsArchive(archive);
        Header = new BinaryFileHeader(NarcSignature, 3);
        FileAllocationTable    = new NarcFileAllocationTable();
        FileNameTable    = new NarcFileNameTable();
        FileImageBlock  = new NarcFileImageBlock();
        if (hasFnt)
        {
            FileNameTable.DirectoryTable = nitroFsArc.DirTable;
            FileNameTable.NameTable      = nitroFsArc.NameTable;
        }

        FileAllocationTable.FileCount = (ushort)nitroFsArc.FileData.Length;
        FileAllocationTable.Entries   = new FatEntry[nitroFsArc.FileData.Length];
        uint imageLength = (uint)nitroFsArc.FileData.Sum(f => (uint)(f.Length + 3) & ~3u);
        FileImageBlock.FileImage = new byte[imageLength];
        uint offset = 0;
        for (int i = 0; i < nitroFsArc.FileData.Length; i++)
        {
            var data = nitroFsArc.FileData[i];
            FileAllocationTable.Entries[i] = new FatEntry(offset, (uint)data.Length);
            data.CopyTo(FileImageBlock.FileImage, offset);
            offset = (uint)(offset + data.Length);
            while ((offset & 3) != 0)
            {
                FileImageBlock.FileImage[offset++] = 0xFF;
            }
        }
    }

    public byte[] Write()
    {
        using (var memoryStream = new MemoryStream())
        {
            Write(memoryStream);
            return memoryStream.ToArray();
        }
    }

    public void Write(Stream stream)
    {
        using (var er = new EndianBinaryWriterEx(stream, Endianness.LittleEndian))
        {
            er.BeginChunk(8);
            {
                Header.Write(er);
                FileAllocationTable.Write(er);
                FileNameTable.Write(er);
                FileImageBlock.Write(er);
            }
            er.EndChunk();
        }
    }

    public BinaryFileHeader Header;
    public NarcFileAllocationTable FileAllocationTable;
    public NarcFileNameTable FileNameTable;
    public NarcFileImageBlock FileImageBlock;

    public NitroFsArchive ToArchive()
    {
        var fileDatas = new byte[FileAllocationTable.FileCount][];
        for (int i = 0; i < FileAllocationTable.FileCount; i++)
        {
            int fileTop = (int)FileAllocationTable.Entries[i].FileTop;
            int fileBottom = (int)FileAllocationTable.Entries[i].FileBottom;
            fileDatas[i] = FileImageBlock.FileImage[fileTop..fileBottom];
        }

        return new NitroFsArchive(FileNameTable.DirectoryTable, FileNameTable.NameTable, fileDatas);
    }
}