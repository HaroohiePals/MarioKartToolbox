#nullable enable

using HaroohiePals.IO;
using System.IO;
using System.Linq;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

static class G3dDictionarySerializer
{
    private const string UNSUPPORTED_REVISION_EXCEPTION_MESSAGE
        = "Unsupported dictionary revision. Got {0}, expected {1}.";
    private const string ENTRY_SIZE_MISMATCH_EXCEPTION_MESSAGE
        = "Dictionary entry size mismatch. Got {0}, expected {1}.";
    private const string INVALID_NUMBER_OF_BYTES_READ_EXCEPTION_MESSAGE
        = "Invalid number of bytes read for dictionary entry. {0} bytes were read, expected {1} bytes.";

    private const byte DICTIONARY_REVISION = 0;
    private const int NAME_LENGTH = 16;

    public static void ReadG3dDictionary<TData>(EndianBinaryReaderEx reader, G3dDictionary<TData> dictionary)
        where TData : notnull, IG3dDictionaryData, new()
    {
        long startPosition = reader.BaseStream.Position;

        // Header
        byte revision = reader.Read<byte>();
        byte entryCount = reader.Read<byte>();
        ushort dictionarySize = reader.Read<ushort>();
        reader.Read<byte>(2);
        ushort entriesOffset = reader.Read<ushort>();
        ThrowIfInvalidRevision(revision);

        // Entries
        reader.BaseStream.Position = startPosition + entriesOffset;
        ushort entrySize = reader.Read<ushort>();
        ushort namesOffset = reader.Read<ushort>();
        ThrowIfIncorrectEntrySize<TData>(entrySize);
        var data = ReadData<TData>(reader, entryCount);

        reader.BaseStream.Position = startPosition + entriesOffset + namesOffset;
        dictionary.Clear();
        for (int i = 0; i < entryCount; i++)
        {
            string name = reader.ReadString(Encoding.ASCII, NAME_LENGTH).TrimEnd('\0');
            dictionary.Add(name, data[i]);
        }

        reader.BaseStream.Position = startPosition + dictionarySize;
    }

    public static void WriteG3dDictionary<TData>(EndianBinaryWriterEx writer, G3dDictionary<TData> dictionary)
        where TData : notnull, IG3dDictionaryData, new()
    {
        long startpos = writer.BaseStream.Position;
        WriteHeader(writer, dictionary.Count);
        WriteNodes(writer, dictionary);
        WriteData(writer, dictionary);
        WriteNames(writer, dictionary);

        long curpos = writer.BaseStream.Position;
        writer.BaseStream.Position = startpos + 2;
        writer.Write((ushort)(curpos - startpos));
        writer.BaseStream.Position = curpos;
    }

    private static void WriteHeader(EndianBinaryWriterEx writer, int itemCount)
    {
        writer.Write<byte>(DICTIONARY_REVISION);
        writer.Write((byte)itemCount);
        writer.Write<ushort>(0);
        writer.Write<ushort>(8);
        writer.Write((ushort)((itemCount + 1) * 4 + 8));
    }

    private static void WriteNodes<TData>(EndianBinaryWriterEx writer, G3dDictionary<TData> dictionary)
        where TData : notnull, IG3dDictionaryData, new()
    {
        var nodes = PatriciaTreeBuilder.BuildFrom(dictionary.Select(item => item.Name));
        foreach (var node in nodes)
        {
            node.Write(writer);
        }
    }

    private static void WriteData<TData>(EndianBinaryWriterEx writer, G3dDictionary<TData> dictionary)
        where TData : notnull, IG3dDictionaryData, new()
    {
        writer.Write(TData.DataSize);
        writer.Write((ushort)(4 + TData.DataSize * dictionary.Count));
        foreach (var item in dictionary)
        {
            item.Data.Write(writer);
        }
    }

    private static void WriteNames<TData>(EndianBinaryWriterEx writer, G3dDictionary<TData> dictionary)
        where TData : notnull, IG3dDictionaryData, new()
    {
        foreach (var item in dictionary)
        {
            writer.Write(item.Name.PadRight(NAME_LENGTH, '\0'), Encoding.ASCII, false);
        }
    }

    private static void ThrowIfInvalidRevision(byte revision)
    {
        if (revision != DICTIONARY_REVISION)
        {
            throw new InvalidDataException(string.Format(
                UNSUPPORTED_REVISION_EXCEPTION_MESSAGE, revision, DICTIONARY_REVISION));
        }
    }

    private static void ThrowIfIncorrectEntrySize<TData>(ushort entrySize)
        where TData : notnull, IG3dDictionaryData, new()
    {
        if (entrySize != TData.DataSize)
        {
            throw new InvalidDataException(string.Format(
                ENTRY_SIZE_MISMATCH_EXCEPTION_MESSAGE, entrySize, TData.DataSize));
        }
    }

    private static TData[] ReadData<TData>(EndianBinaryReaderEx reader, byte entryCount)
        where TData : notnull, IG3dDictionaryData, new()
    {
        var data = new TData[entryCount];
        for (int i = 0; i < entryCount; i++)
        {
            long entryStart = reader.BaseStream.Position;
            data[i] = new TData();
            data[i].Read(reader);

            long dataRead = reader.BaseStream.Position - entryStart;
            if (dataRead != TData.DataSize)
            {
                throw new InvalidDataException(string.Format(
                    INVALID_NUMBER_OF_BYTES_READ_EXCEPTION_MESSAGE, dataRead, TData.DataSize));
            }
        }

        return data;
    }
}
