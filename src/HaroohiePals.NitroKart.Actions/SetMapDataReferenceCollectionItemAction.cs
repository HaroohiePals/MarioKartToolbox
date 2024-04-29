using HaroohiePals.Actions;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;

namespace HaroohiePals.NitroKart.Actions;

public class SetReferenceCollectionItemAction<TReference> : IAction where TReference : IReferenceable<TReference>, IMapDataEntry
{
    private MapDataReferenceCollection<TReference> _collection;

    //private Reference<TReference> _oldReference;
    //private Reference<TReference> _newReference;

    private Reference<TReference> GetOldReference()
        => _oldValue != null ? _collection.FirstOrDefault(x => x.Target.Equals(_oldValue)) : null;

    private Reference<TReference> GetNewReference()
        => _newValue != null ? _collection.FirstOrDefault(x => x.Target.Equals(_newValue)) : null;


    private IReferenceable<TReference> _oldValue;
    private IReferenceable<TReference> _newValue;

    private int _oldIndex = -1;

    public bool IsCreateDelete { get; }

    public SetReferenceCollectionItemAction(MapDataReferenceCollection<TReference> collection, Reference<TReference> oldReference, IReferenceable<TReference> value)
    {
        _collection = collection;

        //_oldReference = oldReference;
        _oldValue = oldReference != null ? oldReference.Target : null;
        _newValue = value;

        // Check if item already exists
        if (_collection.Any(x => x.Target.Equals(_newValue)))
            throw new ArgumentException($"An item with the same key has already been added.");

        _oldIndex = oldReference != null ? _collection.IndexOf(oldReference) : -1;
    }

    public void Do()
    {
        var oldReference = GetOldReference();

        // Adding a new reference
        if (_oldIndex != -1)
        {
            oldReference?.Release();
        }

        var newReference = _newValue?.GetReference((x) =>
        {
            _collection.Remove(x);
        });

        if (newReference != null)
        {
            if (_oldIndex == -1)
                _collection.Add(newReference);
            else
                _collection.Insert(_oldIndex, newReference);
        }
        else
            _collection.Remove(oldReference);
    }

    public void Undo()
    {
        var newReference = GetNewReference();

        newReference?.Release();

        // Undo Adding a new reference
        if (_oldIndex == -1)
            return;

        var oldReference = _oldValue?.GetReference((x) =>
        {
            _collection.Remove(x);
        });

        if (oldReference != null)
            _collection.Insert(_oldIndex, oldReference);
        else
            _collection.Remove(newReference);
    }
}
