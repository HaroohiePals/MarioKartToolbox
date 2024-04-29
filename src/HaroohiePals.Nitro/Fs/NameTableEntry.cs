using HaroohiePals.IO;
using System;
using System.Text;

namespace HaroohiePals.Nitro.Fs;

public sealed class NameTableEntry
{
    private const string NAME_TOO_LONG_EXCEPTION_MESSAGE = "Name can be at most 127 characters long.";
    private const int MAXIMUM_NAME_LENGTH = 127;
    private const byte DIRECTORY_FLAG = 0x80;
    private const ushort MINIMUM_DIRECTORY_ID = 0xF000;

    private NameTableEntry(NameTableEntryType type, string name = null, ushort directoryId = 0)
    {
        Type = type;
        if (type != NameTableEntryType.EndOfDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
            if (name.Length > MAXIMUM_NAME_LENGTH)
            {
                throw new ArgumentException(NAME_TOO_LONG_EXCEPTION_MESSAGE, nameof(name));
            }

            Name = name;
        }

        if (type == NameTableEntryType.Directory)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(directoryId, MINIMUM_DIRECTORY_ID, nameof(directoryId));
            DirectoryId = directoryId;
        }
    }

    public NameTableEntry(EndianBinaryReader reader)
    {
        byte length = reader.Read<byte>();
        if (length == 0)
        {
            Type = NameTableEntryType.EndOfDirectory;
        }
        else if ((length & DIRECTORY_FLAG) != 0)
        {
            Type = NameTableEntryType.Directory;
            Name = reader.ReadString(Encoding.ASCII, length & ~DIRECTORY_FLAG);
            DirectoryId = reader.Read<ushort>();
        }
        else
        {
            Type = NameTableEntryType.File;
            Name = reader.ReadString(Encoding.ASCII, length);
        }
    }

    public void Write(EndianBinaryWriter writer)
    {
        switch (Type)
        {
            case NameTableEntryType.EndOfDirectory:
            {
                writer.Write((byte)0);
                break;
            }
            case NameTableEntryType.File:
            {
                if (Name.Length > MAXIMUM_NAME_LENGTH)
                {
                    throw new InvalidOperationException(NAME_TOO_LONG_EXCEPTION_MESSAGE);
                }

                writer.Write((byte)Name.Length);
                writer.Write(Name, Encoding.ASCII, false);
                break;
            }
            case NameTableEntryType.Directory:
            {
                if (Name.Length > MAXIMUM_NAME_LENGTH)
                {
                    throw new InvalidOperationException(NAME_TOO_LONG_EXCEPTION_MESSAGE);
                }

                writer.Write((byte)(Name.Length | DIRECTORY_FLAG));
                writer.Write(Name, Encoding.ASCII, false);
                writer.Write(DirectoryId);
                break;
            }
            default:
            {
                throw new InvalidOperationException();
            }
        }
    }

    public NameTableEntryType Type { get; }
    public string Name { get; }
    public ushort DirectoryId { get; }

    public static NameTableEntry EndOfDirectory() => new(NameTableEntryType.EndOfDirectory);
    public static NameTableEntry File(string name) => new(NameTableEntryType.File, name);

    public static NameTableEntry Directory(string name, ushort directoryId)
        => new(NameTableEntryType.Directory, name, directoryId);
}