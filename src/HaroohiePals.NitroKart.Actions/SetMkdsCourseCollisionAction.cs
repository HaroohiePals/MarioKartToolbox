using HaroohiePals.Actions;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.NitroKart.Course;

namespace HaroohiePals.NitroKart.Actions;

public class SetMkdsCourseCollisionAction : IAction
{
    private IMkdsCourse _course;
    private MkdsKcl _oldCollision;
    private MkdsKcl _newCollision;

    public bool IsCreateDelete { get; }

    public SetMkdsCourseCollisionAction(IMkdsCourse course, MkdsKcl newCollision)
    {
        _course = course;

        _oldCollision = _course.Collision;
        _newCollision = newCollision;
    }

    public void Do() => _course.Collision = _newCollision;
    public void Undo() => _course.Collision = _oldCollision;
}