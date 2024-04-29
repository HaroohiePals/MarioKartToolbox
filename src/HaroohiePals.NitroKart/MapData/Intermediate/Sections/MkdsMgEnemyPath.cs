using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("MgEnemyPath")]
public sealed class MkdsMgEnemyPath : IMapDataEntry
{
    public MapDataCollection<MkdsMgEnemyPoint> Points { get; } = new();

    public MapDataReferenceCollection<MkdsMgEnemyPoint> Next { get; set; } = new();

    public MapDataReferenceCollection<MkdsMgEnemyPoint> Previous { get; set; } = new();

    public MkdsMgEnemyPath() { }

    public MkdsMgEnemyPath(NkmdMepa.MepaEntry mepaEntry, List<NkmdMepo.MepoEntry> mepoEntries)
    {
        Points.AddRange(mepoEntries.GetRange(mepaEntry.StartIndex, mepaEntry.Length)
            .Select(x => new MkdsMgEnemyPoint(x)));

        for (int j = 0; j < 8; j++)
        {
            if (mepaEntry.Previous[j] != 0xFF)
                Previous.Add(new UnresolvedBinaryMapDataReference<MkdsMgEnemyPoint>(mepaEntry.Previous[j]));
            if (mepaEntry.Next[j] != 0xFF)
                Next.Add(new UnresolvedBinaryMapDataReference<MkdsMgEnemyPoint>(mepaEntry.Next[j]));
        }
    }

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        for (int i = 0; i < Previous.Count; i++)
        {
            if (Previous[i]?.IsResolved is false)
                Previous[i] = resolverCollection.Resolve(Previous[i], x => Previous.Remove(x));
        }

        for (int i = 0; i < Next.Count; i++)
        {
            if (Next[i]?.IsResolved is false)
                Next[i] = resolverCollection.Resolve(Next[i], x => Next.Remove(x));
        }

        // Remove null references that can result by resolving a weak reference
        for (int i = 0; i < Previous.Count; i++)
        {
            if (Previous[i] is null)
                Previous.Remove(Previous[i--]);
        }

        for (int i = 0; i < Next.Count; i++)
        {
            if (Next[i] is null)
                Next.Remove(Next[i--]);
        }

        Points.ResolveReferences(resolverCollection);
    }

    public void ReleaseReferences()
    {
        while (Next.Count > 0)
            Next[0].Release();

        while (Previous.Count > 0)
            Previous[0].Release();

        foreach (var point in Points)
            point.ReleaseReferences();
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsMgEnemyPath();

        entry.Points.AddRange(Points.Select(x => (MkdsMgEnemyPoint)x.Clone()));

        foreach (var next in Next)
        {
            if (next?.IsResolved is true)
                entry.Next.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(next.Target));
            else
                entry.Next.Add(next);
        }

        foreach (var prev in Previous)
        {
            if (prev?.IsResolved is true)
                entry.Previous.Add(new WeakMapDataReference<MkdsMgEnemyPoint>(prev.Target));
            else
                entry.Previous.Add(prev);
        }

        return entry;
    }
}