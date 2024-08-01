using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Extensions;

public static class MkdsAreaExtensions
{
    private const string EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE = "Unknown shape type for MkdsArea";

    private static readonly Box3d BoxUnitBounds = new Box3d(new(-50, 0, -50), new(50, 100, 50));
    private static readonly Box3d CylinderUnitBounds = new Box3d(new(-50, -50, -50), new(50, 50, 50));

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
                    Matrix4.CreateScale((Vector3)(area.LengthVector * (BoxUnitBounds.Size / 2.0))) *
                    rotMatrix *
                    Matrix4.CreateTranslation((Vector3)area.Position);

            case MkdsAreaShapeType.Cylinder:
                var lengthVec = new Vector3d(area.LengthVector.X, area.LengthVector.Y, area.LengthVector.X);
                return
                    Matrix4.CreateScale((Vector3)(lengthVec * (CylinderUnitBounds.Size / 2.0))) *
                    rotMatrix *
                    Matrix4.CreateTranslation((Vector3)area.Position);
            default:
                throw new Exception(EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE);
        }
    }

    public static Transform GetTransform(this MkdsArea area)
    {
        var mtx = area.GetTransformMatrix();
        return new Transform(mtx.ExtractTranslation(), area.GetRotation(), mtx.ExtractScale());
    }

    public static void SetTransform(this MkdsArea area, Transform transform)
    {
        area.Position = transform.Translation;
        area.SetRotation(transform.Rotation);

        switch (area.Shape)
        {
            case MkdsAreaShapeType.Box:
                area.LengthVector = transform.Scale / (BoxUnitBounds.Size / 2.0);
                break;
            case MkdsAreaShapeType.Cylinder:
                area.LengthVector = transform.Scale / (CylinderUnitBounds.Size / 2.0);
                break;
            default:
                throw new Exception(EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE);
        }
    }

    public static Box3d GetLocalBounds(this MkdsArea area)
    {
        switch (area.Shape)
        {
            case MkdsAreaShapeType.Box:
                return BoxUnitBounds.Scaled(area.LengthVector, Vector3d.Zero);
            case MkdsAreaShapeType.Cylinder:
                return CylinderUnitBounds.Scaled(area.LengthVector, Vector3d.Zero);
            default:
                throw new Exception(EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE);
        }
    }
}
