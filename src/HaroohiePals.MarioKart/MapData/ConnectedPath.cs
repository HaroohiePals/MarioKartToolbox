using HaroohiePals.IO.Reference;
using System.ComponentModel;

namespace HaroohiePals.MarioKart.MapData;

public abstract class ConnectedPath<TPath, TPoint> : IReferenceable<TPath>, IMapDataEntry
    where TPath : ConnectedPath<TPath, TPoint>, new()
    where TPoint : IMapDataEntry
{
    [Browsable(false)]
    ReferenceHolder<TPath> IReferenceable<TPath>.ReferenceHolder { get; } = new();

    public MapDataReferenceCollection<TPath> Next { get; set; } = new();

    public MapDataReferenceCollection<TPath> Previous { get; set; } = new();

    public MapDataCollection<TPoint> Points { get; } = new();

    public virtual void ResolveReferences(IReferenceResolverCollection resolverCollection)
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

    public virtual void ReleaseReferences()
    {
        while (Next.Count > 0)
            Next[0].Release();

        while (Previous.Count > 0)
            Previous[0].Release();

        foreach (var point in Points)
            point.ReleaseReferences();

        this.ReleaseAllReferences();
    }

    public IMapDataEntry Clone()
    {
        var entry = new TPath();

        entry.Points.AddRange(Points.Select(x => (TPoint)x.Clone()));

        foreach (var next in Next)
        {
            if (next?.IsResolved is true)
                entry.Next.Add(new WeakMapDataReference<TPath>(next.Target));
            else
                entry.Next.Add(next);
        }

        foreach (var prev in Previous)
        {
            if (prev?.IsResolved is true)
                entry.Previous.Add(new WeakMapDataReference<TPath>(prev.Target));
            else
                entry.Previous.Add(prev);
        }

        return entry;
    }
}