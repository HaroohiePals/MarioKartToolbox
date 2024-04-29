using HaroohiePals.Gui.Viewport;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.MarioKartToolbox.Extensions;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

internal class CourseViewportCollision : IViewportCollision
{
    private MkdsKcl _collision;

    public CourseViewportCollision(IMkdsCourse course)
    {
        _collision = course.Collision;
    }

    public bool FindIntersection(Vector3d start, Vector3d end, out Vector3d intersection)
    {
        var rayDir = (end - start).Normalized();

        bool result = _collision.FindIntersection(start, rayDir, out intersection);

        if (result)
        {
            double length = (start - end).LengthSquared;
            double intersectionLength = (start - intersection).LengthSquared;

            if (intersectionLength < length)
                return true;
        }

        return false;
    }
}
