using HaroohiePals.Actions;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;

namespace HaroohiePals.NitroKart.Actions;

public class DeleteMkdsMapDataCollectionItemsAction<T> : IAction where T : IMapDataEntry
{
    private readonly MkdsMapData _mapData;
    private readonly IReadOnlyList<T> _entries;
    private readonly MapDataCollection<T> _source;

    private Dictionary<IMapDataEntry, int> _restoreIndices = new();
    private Dictionary<IMapDataEntry, IEnumerable<IRestorableReference>> _restoreReferences = new();

    public bool IsCreateDelete => true;

    public DeleteMkdsMapDataCollectionItemsAction(MkdsMapData mapData, T entry, MapDataCollection<T> source) : this(mapData, new[] { entry }, source) { }

    public DeleteMkdsMapDataCollectionItemsAction(MkdsMapData mapData, IEnumerable<T> entries, MapDataCollection<T> source)
    {
        _mapData = mapData;
        _entries = entries.ToArray();
        _source = source;
    }

    public void Do()
    {
        _restoreIndices.Clear();
        _restoreReferences.Clear();

        foreach (var entry in _entries)
        {
            if (_mapData != null)
                _restoreReferences.Add(entry, RestorableMkdsMapDataReferenceFactory.CollectAll(_mapData, entry));

            _restoreIndices.Add(entry, _source.IndexOf(entry));
            _source.Remove(entry);
        }
    }

    public void Undo()
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var entry = _entries[i];

            _source.Insert(_restoreIndices[entry], entry);

            if (_restoreReferences.ContainsKey(entry))
            {
                foreach (var reference in _restoreReferences[entry])
                    reference.Restore();
            }
        }
    }
}