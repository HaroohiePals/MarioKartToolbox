using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;

namespace HaroohiePals.NitroKart.Actions;

public class RestorableMapDataReferenceCollection<TRef> : IRestorableReference where TRef : IReferenceable<TRef>, IMapDataEntry
{
    MapDataReferenceCollection<TRef> _collection;
    private List<TRef> _items { get; set; } = new List<TRef>();
    private int _index { get; set; } = -1;

    public RestorableMapDataReferenceCollection(MapDataReferenceCollection<TRef> collection, TRef item) : this(collection, new List<TRef> { item }) { }

    public RestorableMapDataReferenceCollection(MapDataReferenceCollection<TRef> collection, IEnumerable<TRef> items = null)
    {
        _collection = collection;

        if (items == null)
        {
            foreach (var item in _collection)
                _items.Add(item.Target);
        }
        else
        {
            _index = _collection.FindIndex(x => x.Target.Equals(items.FirstOrDefault()));
            _items = items.ToList();
        }
    }

    public void Restore()
    {
        var insertIndex = _index;

        foreach (var item in _items)
        {
            if (_collection.All(x => !x.Target.Equals(item)))
            {
                var reference = item.GetReference((x) => { _collection.Remove(x); });

                if (insertIndex == -1)
                    _collection.Add(reference);
                else
                    _collection.Insert(insertIndex++, reference);
            }
        }
    }
}
