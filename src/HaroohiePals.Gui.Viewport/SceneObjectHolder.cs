namespace HaroohiePals.Gui.Viewport;

public class SceneObjectHolder
{
    private readonly Dictionary<object, uint> _selection = new();

    public HashSet<object> EnabledObjects { get; } = new();

    public event Action SelectionChanged;

    public void SetSelection(object obj, int subIndex = -1)
    {
        ClearSelection();
        AddToSelection(obj, subIndex);
    }

    public void AddToSelection(object obj, int subIndex = -1)
    {
        if (subIndex < -1)
            throw new ArgumentOutOfRangeException(nameof(subIndex), subIndex, "Sub index should be >= -1");

        if (!_selection.ContainsKey(obj))
            _selection.Add(obj, 1u << subIndex + 1);
        else
            _selection[obj] |= 1u << subIndex + 1;

        SelectionChanged?.Invoke();
    }

    public void RemoveFromSelection(object obj)
    {
        _selection.Remove(obj);

        SelectionChanged?.Invoke();
    }

    public void RemoveSubIndexFromSelection(object obj, int subIndex = -1)
    {
        if (subIndex < -1)
            throw new ArgumentOutOfRangeException(nameof(subIndex), subIndex, "Sub index should be >= -1");

        if (!_selection.ContainsKey(obj))
            return;

        uint mask = _selection[obj];
        mask &= ~(1u << subIndex + 1);
        if (mask == 0)
            _selection.Remove(obj);
        else
            _selection[obj] = mask;

        SelectionChanged?.Invoke();
    }

    public bool IsSelected(object obj) => _selection.ContainsKey(obj);

    public bool IsSubIndexSelected(object obj, int subIndex = -1)
    {
        if (subIndex < -1)
            throw new ArgumentOutOfRangeException(nameof(subIndex), subIndex, "Sub index should be >= -1");

        if (!_selection.ContainsKey(obj))
            return false;

        return (_selection[obj] & 1u << subIndex + 1) != 0;
    }

    public void ClearSelection()
    {
        _selection.Clear();
        SelectionChanged?.Invoke();
    }

    public int SelectionSize => _selection.Count;
    public IEnumerable<object> GetSelection() => _selection.Keys;

    public bool IsEnabled(object obj) => EnabledObjects.Contains(obj);
}