#nullable enable
using HaroohiePals.Actions;
using HaroohiePals.NitroKart.Course;

namespace HaroohiePals.NitroKart.Actions;

public class SetMkdsCourseGlobalMapGraphicsAction : IAction
{
    private readonly IMkdsCourse _course;
    private readonly MkdsMapGraphics? _oldGraphics;
    private readonly MkdsMapGraphics _newGraphics;

    public bool IsCreateDelete { get; }

    public SetMkdsCourseGlobalMapGraphicsAction(IMkdsCourse course, MkdsMapGraphics newGraphics)
    {
        _course = course;
        _oldGraphics = course.GlobalMapGraphics;
        _newGraphics = newGraphics;
    }

    public void Do() => _course.GlobalMapGraphics = _newGraphics;
    public void Undo() => _course.GlobalMapGraphics = _oldGraphics;
}
