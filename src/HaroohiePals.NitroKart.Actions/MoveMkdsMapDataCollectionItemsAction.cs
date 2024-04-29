using HaroohiePals.Actions;
using HaroohiePals.MarioKart.MapData;

namespace HaroohiePals.NitroKart.Actions;

public class MoveMkdsMapDataCollectionItemsAction<T> : IAction where T : IMapDataEntry
{
    private readonly IReadOnlyList<T> _entries;
    private readonly MapDataCollection<T> _source;
    private readonly MapDataCollection<T> _target;
    private readonly int _targetIndex;

    private Dictionary<IMapDataEntry, int> _restoreIndices = new();

    public bool IsCreateDelete => false;

    public MoveMkdsMapDataCollectionItemsAction(IEnumerable<T> entries, MapDataCollection<T> source, int targetIndex) : this(entries, source, source, targetIndex) { }

    public MoveMkdsMapDataCollectionItemsAction(IEnumerable<T> entries, MapDataCollection<T> source, MapDataCollection<T> target, int targetIndex)
    {
        _entries = entries.ToArray();
        _source = source;
        _target = target;
        _targetIndex = targetIndex;
    }

    public void Do()
    {
        _restoreIndices.Clear();

        int targetIndex = _targetIndex;

        foreach (var entry in _entries)
        {
            int restoreIndex = _source.IndexOf(entry);
            if (_source == _target && targetIndex <= restoreIndex)
                restoreIndex++;

            _restoreIndices.Add(entry, restoreIndex);
            targetIndex = _source.Move(targetIndex, entry, _target) + 1;
        }
    }

    public void Undo()
    {
        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var entry = _entries[i];
            _target.Move(_restoreIndices[entry], entry, _source);
        }
    }
}
