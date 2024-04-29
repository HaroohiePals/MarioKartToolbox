using OpenTK.Mathematics;

namespace HaroohiePals.MarioKart.MapData;

public interface IRotatedPoint : IPoint
{
    Vector3d Rotation { get; set; }
}