using ImGuiNET;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

static class ImGuizmoUtils
{
    public static Vector4 BuildPlane(Vector3 pPoint1, Vector3 pNormal)
    {
        var normal = pNormal.Normalized();
        Vector4 result;
        result.W = Vector3.Dot(normal, pPoint1);
        result.X = normal.X;
        result.Y = normal.Y;
        result.Z = normal.Z;
        return result;
    }

    public static float IntersectRayPlane(Vector3 rOrigin, Vector3 rVector, Vector4 plane)
    {
        float numer = Vector3.Dot(plane.Xyz, rOrigin) - plane.W;
        float denom = Vector3.Dot(plane.Xyz, rVector);

        if (MathF.Abs(denom) < ImGuizmoConsts.FLT_EPSILON) // normal is orthogonal to vector, cant intersect
            return -1.0f;

        return -(numer / denom);
    }

    public static void ComputeCameraRay(out Vector3 rayOrigin, out Vector3 rayDir, Vector2 position, Vector2 size,
        Matrix4 projMtx, Matrix4 viewMtx, bool reversed)
    {
        var io = ImGui.GetIO();

        var mViewProjInverse = (viewMtx * projMtx).Inverted();

        float mox = (io.MousePos.X - position.X) / size.X * 2f - 1f;
        float moy = (1f - (io.MousePos.Y - position.Y) / size.Y) * 2f - 1f;

        float zNear = reversed ? 1f - ImGuizmoConsts.FLT_EPSILON : 0f;
        float zFar = reversed ? 0f : 1f - ImGuizmoConsts.FLT_EPSILON;

        rayOrigin = Vector3.TransformPerspective((mox, moy, zNear), mViewProjInverse);
        var rayEnd = Vector3.TransformPerspective((mox, moy, zFar), mViewProjInverse);
        rayDir = (rayEnd - rayOrigin).Normalized();
    }

    public static System.Numerics.Vector2 WorldToPos(Vector3 worldPos, Matrix4 mat, Vector2 position, Vector2 size)
    {
        var test = Vector3.Project(worldPos, position.X, position.Y, size.X, size.Y, -1, 1, mat);
        test.Y = size.Y - test.Y + position.Y + position.Y;

        return new(test.X, test.Y);
    }

    public static Quaternion MtxToQuat(Matrix3 mtx)
    {
        var dst = new Quaternion();
        float tr = mtx[0, 0] + mtx[1, 1] + mtx[2, 2];
        if (tr > 0)
        {
            float s = MathF.Sqrt(tr + 1);
            dst.W = s / 2;
            float invS = 0.5f / s;
            dst.X = (mtx[1, 2] - mtx[2, 1]) * invS;
            dst.Y = (mtx[2, 0] - mtx[0, 2]) * invS;
            dst.Z = (mtx[0, 1] - mtx[1, 0]) * invS;
        }
        else if (mtx[0, 0] >= mtx[1, 1] && mtx[0, 0] >= mtx[2, 2])
        {
            float s = MathF.Sqrt(1 + mtx[0, 0] - mtx[1, 1] - mtx[2, 2]);
            dst.X = s / 2;
            float invS = 0.5f / s;
            dst.Y = (mtx[0, 1] + mtx[1, 0]) * invS;
            dst.Z = (mtx[0, 2] + mtx[2, 0]) * invS;
            dst.W = (mtx[1, 2] - mtx[2, 1]) * invS;
        }
        else if (mtx[1, 1] > mtx[2, 2])
        {
            float s = MathF.Sqrt(1 + mtx[1, 1] - mtx[0, 0] - mtx[2, 2]);
            dst.Y = s / 2;
            float invS = 0.5f / s;
            dst.X = (mtx[1, 0] + mtx[0, 1]) * invS;
            dst.Z = (mtx[2, 1] + mtx[1, 2]) * invS;
            dst.W = (mtx[2, 0] - mtx[0, 2]) * invS;
        }
        else
        {
            float s = MathF.Sqrt(1 + mtx[2, 2] - mtx[0, 0] - mtx[1, 1]);
            dst.Z = s / 2;
            float invS = 0.5f / s;
            dst.X = (mtx[2, 0] + mtx[0, 2]) * invS;
            dst.Y = (mtx[2, 1] + mtx[1, 2]) * invS;
            dst.W = (mtx[0, 1] - mtx[1, 0]) * invS;
        }

        return dst;
    }

    public static Vector3 TransformPoint(this Vector3 vec, Matrix4 mtx)
        => TransformPoint(new Vector4(vec, 0), mtx).Xyz;

    public static Vector4 TransformPoint(this Vector4 vec, Matrix4 mtx)
    {
        Vector4 outVec = Vector4.Zero;

        outVec.X = vec.X * mtx[0, 0] + vec.Y * mtx[1, 0] + vec.Z * mtx[2, 0] + mtx[3, 0];
        outVec.Y = vec.X * mtx[0, 1] + vec.Y * mtx[1, 1] + vec.Z * mtx[2, 1] + mtx[3, 1];
        outVec.Z = vec.X * mtx[0, 2] + vec.Y * mtx[1, 2] + vec.Z * mtx[2, 2] + mtx[3, 2];
        outVec.W = vec.X * mtx[0, 3] + vec.Y * mtx[1, 3] + vec.Z * mtx[2, 3] + mtx[3, 3];

        return outVec;
    }

    //public static Vector4 TransformVector(this Vector4 vec, Matrix4 mtx)
    //{
    //    Vector4 outVec = Vector4.Zero;

    //    outVec.X = vec.X * mtx[0, 0] + vec.Y * mtx[1, 0] + vec.Z * mtx[2, 0];
    //    outVec.Y = vec.X * mtx[0, 1] + vec.Y * mtx[1, 1] + vec.Z * mtx[2, 1];
    //    outVec.Z = vec.X * mtx[0, 2] + vec.Y * mtx[1, 2] + vec.Z * mtx[2, 2];
    //    outVec.W = vec.X * mtx[0, 3] + vec.Y * mtx[1, 3] + vec.Z * mtx[2, 3];

    //    return outVec;
    //}

    public static Vector4 Transform(this Vector4 vec, Matrix4 mtx)
    {
        Vector4 outVec;

        outVec.X = vec.X * mtx[0, 0] + vec.Y * mtx[1, 0] + vec.Z * mtx[2, 0] + vec.W * mtx[3, 0];
        outVec.Y = vec.X * mtx[0, 1] + vec.Y * mtx[1, 1] + vec.Z * mtx[2, 1] + vec.W * mtx[3, 1];
        outVec.Z = vec.X * mtx[0, 2] + vec.Y * mtx[1, 2] + vec.Z * mtx[2, 2] + vec.W * mtx[3, 2];
        outVec.W = vec.X * mtx[0, 3] + vec.Y * mtx[1, 3] + vec.Z * mtx[2, 3] + vec.W * mtx[3, 3];

        return outVec;
    }

    public static Vector3 PointOnSegment(Vector3 point, Vector3 vertPos1, Vector3 vertPos2)
    {
        Vector3 c = point - vertPos1;
        Vector3 V = Vector3.Normalize(vertPos2 - vertPos1);
        float d = (vertPos2 - vertPos1).Length;
        float t = Vector3.Dot(V, c);

        if (t < 0f)
        {
            return vertPos1;
        }

        if (t > d)
        {
            return vertPos2;
        }

        return vertPos1 + V * t;
    }

    public static void OrthoNormalize(this Matrix4 matrix)
    {
        matrix.Row0 = matrix.Row0.Normalized();
        matrix.Row1 = matrix.Row1.Normalized();
        matrix.Row2 = matrix.Row2.Normalized();
    }

    public static float ComputeSnap(float value, float snap, float snapTension)
    {
        if (snap <= float.Epsilon)
        {
            return value;
        }

        float modulo = value % snap;
        float moduloRatio = Math.Abs(modulo) / snap;
        if (moduloRatio < snapTension)
        {
            value -= modulo;
        }
        else if (moduloRatio > (1f - snapTension))
        {
            value = value - modulo + snap * ((value < 0f) ? -1f : 1f);
        }

        return value;
    }

    public static Vector3 ComputeSnap(Vector3 value, Vector3 snap, float snapTension)
    {
        for (int i = 0; i < 3; i++)
        {
            value[i] = ComputeSnap(value[i], snap[i], snapTension);
        }

        return value;
    }

    public static void DecomposeMatrixToComponents(Matrix4 matrix, out Vector3d translation, out Vector3d rotation, out Vector3d scale)
    {
        translation = Vector3d.Zero;
        rotation = Vector3d.Zero;
        scale = Vector3d.Zero;

        scale[0] = matrix.Row0.Length;
        scale[1] = matrix.Row1.Length;
        scale[2] = matrix.Row2.Length;

        matrix.OrthoNormalize();

        rotation[0] = ImGuizmoConsts.RAD_TO_DEG * MathF.Atan2( matrix[1,2], matrix[2,2]);
        rotation[1] = ImGuizmoConsts.RAD_TO_DEG * MathF.Atan2(-matrix[0,2], MathF.Sqrt(matrix[1,2] * matrix[1,2] + matrix[2,2] * matrix[2,2]));
        rotation[2] = ImGuizmoConsts.RAD_TO_DEG * MathF.Atan2( matrix[0,1], matrix[0,0]);

        translation[0] = matrix.Row3.X;
        translation[1] = matrix.Row3.Y;
        translation[2] = matrix.Row3.Z;
    }
}
