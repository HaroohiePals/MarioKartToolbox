using OpenTK.Mathematics;

namespace HaroohiePals.Mathematics;

public record struct Transform(Vector3d Translation, Vector3d Rotation, Vector3d Scale)
{
    public static readonly Transform Identity = new(Vector3d.Zero, Vector3d.Zero, Vector3d.One);
}
