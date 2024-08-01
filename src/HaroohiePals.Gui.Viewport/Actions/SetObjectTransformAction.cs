using HaroohiePals.Actions;
using HaroohiePals.Mathematics;

namespace HaroohiePals.Gui.Viewport.Actions;

public sealed class SetObjectTransformAction : IAction
{
    private readonly RenderGroup _group;
    private readonly object _object;
    private readonly int _subIndex;
    private readonly Transform _oldTransform;
    private readonly Transform _transform;

    public bool IsCreateDelete { get; }

    public SetObjectTransformAction(RenderGroup group, object obj, int subIndex, in Transform transform)
    {
        _group = group;
        _object = obj;
        _subIndex = subIndex;
        _transform = transform;

        if (!_group.TryGetObjectTransform(_object, _subIndex, out _oldTransform))
            throw new ArgumentException("Invalid group specified", nameof(group));
    }

    public SetObjectTransformAction(RenderGroup group, object obj, int subIndex, in Transform oldTransform, in Transform transform)
    {
        _group = group;
        _object = obj;
        _subIndex = subIndex;
        _oldTransform = oldTransform;
        _transform = transform;
    }

    public void Do()
    {
        _group.TrySetObjectTransform(_object, _subIndex, _transform);
    }

    public void Undo()
    {
        _group.TrySetObjectTransform(_object, _subIndex, _oldTransform);
    }
}