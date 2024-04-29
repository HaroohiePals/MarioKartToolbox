using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("EnemyPath")]
public sealed class MkdsEnemyPath : ConnectedPath<MkdsEnemyPath, MkdsEnemyPoint>
{
    public MkdsEnemyPath()
    {
    }

    public MkdsEnemyPath(NkmdIpatEpatEntry epatEntry, List<NkmdEpoi.EpoiEntry> epoiEntries)
    {
        foreach (byte prev in epatEntry.Previous.Where(prev => prev != 0xFF))
            Previous.Add(new UnresolvedBinaryMapDataReference<MkdsEnemyPath>(prev));

        foreach (byte next in epatEntry.Next.Where(next => next != 0xFF))
            Next.Add(new UnresolvedBinaryMapDataReference<MkdsEnemyPath>(next));

        Points.AddRange(epoiEntries.GetRange(epatEntry.StartIndex, epatEntry.Length).Select(x => new MkdsEnemyPoint(x)));
    }
}