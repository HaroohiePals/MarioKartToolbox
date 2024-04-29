using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("ItemPath")]
public sealed class MkdsItemPath : ConnectedPath<MkdsItemPath, MkdsItemPoint>
{
    public MkdsItemPath()
    {
    }

    public MkdsItemPath(NkmdIpatEpatEntry ipatEntry, List<NkmdIpoi.IpoiEntry> ipoiEntries)
    {
        foreach (byte prev in ipatEntry.Previous.Where(prev => prev != 0xFF))
            Previous.Add(new UnresolvedBinaryMapDataReference<MkdsItemPath>(prev));

        foreach (byte next in ipatEntry.Next.Where(next => next != 0xFF))
            Next.Add(new UnresolvedBinaryMapDataReference<MkdsItemPath>(next));

        Points.AddRange(ipoiEntries.GetRange(ipatEntry.StartIndex, ipatEntry.Length).Select(x => new MkdsItemPoint(x)));
    }
}