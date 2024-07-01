using HaroohiePals.Mathematics;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System.Drawing;
using static System.Formats.Asn1.AsnWriter;

namespace HaroohiePals.NitroKart.Extensions;

public static class AreaExtensions
{
    private const string EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE = "Unknown shape type for MkdsArea";

    private static readonly Vector3d BaseSize = new Vector3(50, 50, 50);

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
                    Matrix4.CreateScale((Vector3)(area.LengthVector * BaseSize)) *
                    rotMatrix *
                    Matrix4.CreateTranslation((Vector3)area.Position);

            case MkdsAreaShapeType.Cylinder:
                var lengthVec = (Vector3)new Vector3d(area.LengthVector.X, area.LengthVector.Y, area.LengthVector.X);
                return
                    Matrix4.CreateScale(lengthVec * (Vector3)BaseSize) *
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

        var scale = transform.Scale;

        switch (area.Shape)
        {
            case MkdsAreaShapeType.Box:
                area.LengthVector = scale / BaseSize;
                break;
            case MkdsAreaShapeType.Cylinder:
                // For cylinder, scale X and Z are the same
                area.LengthVector = new Vector3d(scale.X / BaseSize.X, scale.Y / BaseSize.Y, scale.X / BaseSize.Z);
                break;
            default:
                throw new Exception(EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE);
        }
    }

    public static AxisAlignedBoundingBox GetLocalBounds(this MkdsArea area)
    {
        var size = Vector3d.Zero;

        switch (area.Shape)
        {
            case MkdsAreaShapeType.Box:
                size = area.LengthVector * BaseSize;
                return new AxisAlignedBoundingBox(new(-size.X, 0, -size.Z), new(size.X, size.Y * 2, size.Z));
            case MkdsAreaShapeType.Cylinder:
                size = new Vector3d(
                    area.LengthVector.X * BaseSize.X,
                    area.LengthVector.Y * BaseSize.Y,
                    area.LengthVector.X * BaseSize.Z);
                return new AxisAlignedBoundingBox(new(-size.X, -size.Y, -size.Z), new(size.X, size.Y, size.Z));
            default:
                throw new Exception(EXCEPTION_MESSAGE_UNKNOWN_SHAPE_TYPE);
        }
        
    }
}
