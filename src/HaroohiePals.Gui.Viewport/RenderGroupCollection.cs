using System.Collections;

namespace HaroohiePals.Gui.Viewport;

public class RenderGroupCollection : IList<RenderGroup>
{
    private readonly List<RenderGroup> _renderGroups = new();

    public int  Count      => _renderGroups.Count;
    public bool IsReadOnly => false;
    public IEnumerator<RenderGroup> GetEnumerator() => _renderGroups.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_renderGroups).GetEnumerator();
    public void Add(RenderGroup item) => _renderGroups.Add(item);
    public void Clear() => _renderGroups.Clear();
    public bool Contains(RenderGroup item) => _renderGroups.Contains(item);
    public void CopyTo(RenderGroup[] array, int arrayIndex) => _renderGroups.CopyTo(array, arrayIndex);
    public bool Remove(RenderGroup item) => _renderGroups.Remove(item);
    public int IndexOf(RenderGroup item) => _renderGroups.IndexOf(item);
    public void Insert(int index, RenderGroup item) => _renderGroups.Insert(index, item);
    public void RemoveAt(int index) => _renderGroups.RemoveAt(index);

    public RenderGroup this[int index]
    {
        get => _renderGroups[index];
        set => _renderGroups[index] = value;
    }

    public bool GetObjectTransform(object obj, int subIndex, out Transform transform)
        => GetObjectTransformGroup(obj, subIndex, out transform) != null;

    public RenderGroup GetObjectTransformGroup(object obj, int subIndex, out Transform transform)
    {
        foreach (var g in _renderGroups)
        {
            if (g.ContainsObject(obj) && g.GetObjectTransform(obj, subIndex, out transform))
                return g;
        }

        transform = new Transform(new(0), new(0), new(1));
        return null;
    }

    public bool SetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        foreach (var g in _renderGroups)
        {
            if (g.ContainsObject(obj) && g.SetObjectTransform(obj, subIndex, transform))
                return true;
        }

        return false;
    }

    public void Update(float deltaTime)
    {
        foreach (var group in _renderGroups)
        {
            if (!group.Enabled)
                continue;

            group.Update(deltaTime);
        }
    }
}