using OpenTK.Mathematics;

namespace HaroohiePals.Mathematics;

public record struct AxisAlignedBoundingBox(Vector3d Minimum, Vector3d Maximum)
{
    public static readonly AxisAlignedBoundingBox Zero = new(Vector3d.Zero, Vector3d.Zero);
}