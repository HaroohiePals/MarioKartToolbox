using System.Reflection;

namespace HaroohiePals.Actions;

public sealed class SetPropertyAction : IAction
{
    private class SourceProperty
    {
        public SourceProperty(object source, string propertyName)
            : this(source, source.GetType().GetProperty(propertyName)) { }

        public SourceProperty(object source, PropertyInfo property)
        {
            _source   = source;
            _property = property;

            _oldValue = _property.GetValue(_source);
        }

        private readonly object _source;
        private readonly object _oldValue;

        private readonly PropertyInfo _property;

        public void SetValue(object value) => _property.SetValue(_source, value);

        public void Revert() => SetValue(_oldValue);
    }

    private readonly SourceProperty[] _source;
    private readonly string           _propertyName;
    private readonly object           _newValue;

    public bool IsCreateDelete => false;

    public SetPropertyAction(object source, string propertyName, object newValue)
    {
        _source       = new[] { new SourceProperty(source, propertyName) };
        _propertyName = propertyName;
        _newValue     = newValue;
    }

    public SetPropertyAction(object source, PropertyInfo property, object newValue)
    {
        _source       = new[] { new SourceProperty(source, property) };
        _propertyName = property.Name;
        _newValue     = newValue;
    }

    public SetPropertyAction(IEnumerable<object> source, string propertyName, object newValue)
    {
        _source       = source.Select(x => new SourceProperty(x, propertyName)).ToArray();
        _propertyName = propertyName;
        _newValue     = newValue;
    }

    public void Do()
    {
        foreach (var x in _source)
            x.SetValue(_newValue);
    }

    public void Undo()
    {
        foreach (var x in _source)
            x.Revert();
    }

    public override string ToString() => $"{_propertyName} => {_newValue}";
}