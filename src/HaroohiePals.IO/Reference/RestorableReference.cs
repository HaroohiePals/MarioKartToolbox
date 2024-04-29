namespace HaroohiePals.IO.Reference;

public class RestorableReference<TRef> : IRestorableReference where TRef : IReferenceable<TRef>
{
    private TRef _item { get; set; }
    private object _target { get; set; }
    private string _propertyName { get; set; }

    public RestorableReference(TRef item, object target, string propertyName)
    {
        _item = item;
        _target = target;
        _propertyName = propertyName;
    }

    public void Restore()
    {
        var property = _target.GetType().GetProperty(_propertyName);

        var newReference = _item?.GetReference((x) =>
        {
            property.SetValue(_target, null);
        });

        property.SetValue(_target, newReference);
    }
}
