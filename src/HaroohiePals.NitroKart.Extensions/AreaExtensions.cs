using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Extensions;

public static class AreaExtensions
{
    public static Vector3d GetRotation(this MkdsArea area)
    {
        //Read Handle conversion
        var matrix = Matrix3d.Identity;
        matrix.Row0 = area.XVector;
        matrix.Row1 = area.YVector;
        matrix.Row2 = area.ZVector;

        var rotation = new Vector3d
        (
            MathHelper.RadiansToDegrees(Math.Atan2(matrix[1, 2], matrix[2, 2])),
            MathHelper.RadiansToDegrees(Math.Atan2(-matrix[0, 2], Math.Sqrt(matrix[1, 2] * matrix[1, 2] + matrix[2, 2] * matrix[2, 2]))),
            MathHelper.RadiansToDegrees(Math.Atan2(matrix[0, 1], matrix[0, 0]))
        );

        return rotation;
    }

    public static void SetRotation(this MkdsArea area, Vector3d rotation)
    {
        //Write Handle conversion
        var orientationMatrix =
            Matrix3d.CreateRotationX(MathHelper.DegreesToRadians(rotation.X)) *
            Matrix3d.CreateRotationY(MathHelper.DegreesToRadians(rotation.Y)) *
            Matrix3d.CreateRotationZ(MathHelper.DegreesToRadians(rotation.Z));

        area.XVector = orientationMatrix.Row0;
        area.YVector = orientationMatrix.Row1;
        area.ZVector = orientationMatrix.Row2;
    }

    public static Matrix4 GetTransformMatrix(this MkdsArea area)
    {
        var rotMatrix = Matrix4.Identity;
        rotMatrix.Row0 = new((Vector3)area.XVector, 0);
        rotMatrix.Row1 = new((Vector3)area.YVector, 0);
        rotMatrix.Row2 = new((Vector3)area.ZVector, 0);

        switch (area.Shape)
        {
            case MkdsAreaShapeType.Box:
                return
                    Matrix4.CreateScale((Vector3)(area.LengthVector * new Vector3(50, 50, 50))) *
                    rotMatrix *
                    Matrix4.CreateTranslation((Vector3)area.Position);

            case MkdsAreaShapeType.Cylinder:
                var lengthVec = (Vector3)new Vector3d(area.LengthVector.X, area.LengthVector.Y, area.LengthVector.X);
                return
                    Matrix4.CreateScale(lengthVec * new Vector3(50, 50, 50)) *
                    rotMatrix *
                    Matrix4.CreateTranslation((Vector3)area.Position);
            default:
                throw new Exception();
        }
    }
}
