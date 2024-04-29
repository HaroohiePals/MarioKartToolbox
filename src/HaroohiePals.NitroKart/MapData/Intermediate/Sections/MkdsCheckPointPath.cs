using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("CheckPointPath")]
public sealed class MkdsCheckPointPath : ConnectedPath<MkdsCheckPointPath, MkdsCheckPoint>
{
    public MkdsCheckPointPath()
    {
    }

    public MkdsCheckPointPath(NkmdCpat.CpatEntry cpatEntry, IEnumerable<NkmdCpoi.CpoiEntry> cpoiEntries)
    {
        foreach (byte prev in cpatEntry.Previous.Where(prev => prev != 0xFF))
            Previous.Add(new UnresolvedBinaryMapDataReference<MkdsCheckPointPath>(prev));

        foreach (byte next in cpatEntry.Next.Where(next => next != 0xFF))
            Next.Add(new UnresolvedBinaryMapDataReference<MkdsCheckPointPath>(next));

        Points.AddRange(cpoiEntries.Skip(cpatEntry.StartIndex).Take(cpatEntry.Length).Select(x => new MkdsCheckPoint(x)));
    }
}