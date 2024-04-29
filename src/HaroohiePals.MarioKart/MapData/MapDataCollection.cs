using HaroohiePals.IO.Reference;
using System.Collections;
using System.ComponentModel;

namespace HaroohiePals.MarioKart.MapData;

public class MapDataCollection<T> : IMapDataCollection, IList<T> where T : IMapDataEntry
{
    protected readonly List<T> Entries = new();

    [Browsable(false)]
    public int Count => Entries.Count;

    [Browsable(false)]
    public bool IsReadOnly => false;

    public T this[int index]
    {
        get => Entries[index];
        set => Entries[index] = value;
    }

    public MapDataCollection() { }

    public MapDataCollection(IEnumerable<T> entries)
    {
        AddRange(entries);
    }

    public void Clear()
    {
        Entries.ForEach(x => x.ReleaseReferences());
        Entries.Clear();
    }

    public void AddRange(IEnumerable<T> items) => Entries.AddRange(items);

    public void Add(T item) => Entries.Add(item);

    public int IndexOf(T entry) => Entries.IndexOf(entry);

    public IEnumerator<T> GetEnumerator() => Entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => Entries.GetEnumerator();

    public void Insert(int index, T item) => Entries.Insert(index, item);

    public void RemoveAt(int index)
    {
        var item = Entries[index];
        Remove(item);
    }

    public bool Remove(T item)
    {
        item?.ReleaseReferences();
        return Entries.Remove(item);
    }

    public int Move(int targetIndex, T item, MapDataCollection<T> targetCollection = null)
    {
        if (targetCollection == null)
            targetCollection = this;

        if (targetCollection == this)
        {
            int currentIndex = IndexOf(item);
            if (currentIndex < targetIndex)
                targetIndex--;
        }

        Entries.Remove(item);

        if (targetIndex < 0)
            targetIndex = 0;
        if (targetIndex > targetCollection.Count)
            targetIndex = targetCollection.Count;

        targetCollection.Insert(targetIndex, item);

        return targetCollection.IndexOf(item);
    }

    public bool Contains(T item) => Entries.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => Entries.CopyTo(array, arrayIndex);

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
        => Entries.ForEach(x => x.ResolveReferences(resolverCollection));
}