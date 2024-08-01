using HaroohiePals.Mathematics;
using OpenTK.Mathematics;
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

    public bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
        => TryGetObjectTransformGroup(obj, subIndex, out transform, out _);

    public bool TryGetObjectTransformGroup(object obj, int subIndex, out Transform transform, out RenderGroup renderGroup)
    {
        foreach (var g in _renderGroups)
        {
            if (g.ContainsObject(obj) && g.TryGetObjectTransform(obj, subIndex, out transform))
            {
                renderGroup = g;
                return true;
            }
        }

        transform = Transform.Identity;
        renderGroup = null;
        return false;
    }

    public bool TrySetObjectTransform(object obj, int subIndex, in Transform transform)
    {
        foreach (var g in _renderGroups)
        {
            if (g.ContainsObject(obj) && g.TrySetObjectTransform(obj, subIndex, transform))
                return true;
        }

        return false;
    }

    public bool TryGetLocalObjectBounds(object obj, int subIndex, out Box3d bounds)
    {
        foreach (var g in _renderGroups)
        {
            if (g.ContainsObject(obj) && g.TryGetLocalObjectBounds(obj, subIndex, out bounds))
                return true;
        }

        bounds = new Box3d();
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