using OpenTK.Mathematics;
using System.ComponentModel;

namespace HaroohiePals.NitroKart.Race;

public class Driver
{
    public Vector3d Position { get; set; }
    //public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Vector3d Direction { get; set; } = Vector3d.UnitX;
    public Vector3d Up { get; set; } = Vector3d.UnitY;
    public Matrix3d MainMtx
    {
        get
        {
            var matrix = Matrix3d.Identity;

            var forward = Direction.Normalized();
            var right = Vector3d.Cross(Up, forward).Normalized();
            var up = Vector3d.Cross(right, forward).Normalized();

            matrix.Row0 = right;
            matrix.Row1 = up;
            matrix.Row2 = forward;

            return matrix;
        }
    }
    public double Field2C0 { get; set; } = 20;

    [DisplayName("Elevation")]
    public double Field2BC { get; set; } = -8;
}
