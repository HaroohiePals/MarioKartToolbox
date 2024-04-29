using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public interface IViewportCollision
{
    bool FindIntersection(Vector3d start, Vector3d end, out Vector3d intersection);
}
