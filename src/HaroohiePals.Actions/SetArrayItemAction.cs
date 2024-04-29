namespace HaroohiePals.Actions;

public sealed class SetArrayItemAction : IAction
{
    private readonly Array  _source;
    private readonly int    _index;
    private readonly object _newValue;
    private readonly object _oldValue;

    public bool IsCreateDelete => false;

    public SetArrayItemAction(Array source, int index, object value)
    {
        _source   = source;
        _index    = index;
        _newValue = value;
        _oldValue = _source.GetValue(index);
    }

    public void Do() => _source.SetValue(_newValue, _index);
    public void Undo() => _source.SetValue(_oldValue, _index);
    public override string ToString() => $"{_source}[{_index}] => {_newValue}";
}