using HaroohiePals.Actions;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;

namespace HaroohiePals.MarioKart.Actions;

public class SetMapDataEntryReferenceAction<TReference> : IAction
    where TReference : IReferenceable<TReference>
{
    private Reference<TReference> _oldReference;
    private Reference<TReference> _newReference;

    private IReferenceable<TReference> _oldValue;
    private IReferenceable<TReference> _newValue;

    private object _source;
    private string _propertyName;

    public bool IsCreateDelete { get; }

    public SetMapDataEntryReferenceAction(IMapDataEntry source, string propertyName, Reference<TReference> oldReference, IReferenceable<TReference> value)
    {
        _oldReference = oldReference;
        _oldValue = _oldReference != null ? _oldReference.Target : null;
        _newValue = value;
        _source = source;
        _propertyName = propertyName;
    }

    public void Do()
    {
        _oldReference?.Release();

        var property = _source.GetType().GetProperty(_propertyName);

        _newReference = _newValue?.GetReference((x) =>
        {
            property.SetValue(_source, null);
        });

        property.SetValue(_source, _newReference);
    }

    public void Undo()
    {
        _newReference?.Release();

        var property = _source.GetType().GetProperty(_propertyName);

        if (_oldReference is not null && _oldReference.IsResolved)
        {
            _oldReference = _oldValue?.GetReference((x) =>
            {
                property.SetValue(_source, null);
            });
        }
       
        property.SetValue(_source, _oldReference);
    }
}
