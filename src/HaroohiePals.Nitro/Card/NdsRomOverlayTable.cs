using HaroohiePals.IO;
using System.IO;

namespace HaroohiePals.Nitro.Card;

public class NdsRomOverlayTable
{
    public NdsRomOverlayTableEntry[] Entries { get; private set; }
    public int Length => Entries.Length;

    public NdsRomOverlayTable(EndianBinaryReaderEx er, uint overlayTableSize)
    {
        uint entriesCount = overlayTableSize / NdsRomOverlayTableEntry.SIZE;
        Entries = new NdsRomOverlayTableEntry[entriesCount];
        for (int i = 0; i < entriesCount; i++)
        {
            Entries[i] = new NdsRomOverlayTableEntry(er);
        }
    }

    public void Write(EndianBinaryWriterEx ew)
    {
        foreach (var v in Entries)
        {
            v.Write(ew);
        }
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
}
