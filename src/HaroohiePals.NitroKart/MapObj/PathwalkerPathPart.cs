using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj;

sealed class PathwalkerPathPart
{
    public Vector3d P0;
    public Vector3d P1;
    public Vector3d P2;
    public Vector3d P3;
    public double Length;
    public double OneDivLength;
    public double HermLength;
    public double OneDivHermLength;
    public double LinLength;
    public double OneDivLinLength;
    public Vector3d Field48;

    public PathwalkerPathPart(bool a2, bool a3, in Vector3d? poitA, in Vector3d poitB, in Vector3d poitC,
        in Vector3d? poitD)
    {
        double v8;
        double v11;
        double v15;
        double v17;
        double v20;
        double v24;
        double v29;
        Vector3d ab;
        Vector3d direction;
        Vector3d diff;
        Vector3d a2a;
        Vector3d src;
        Vector3d v36;

        v8 = 0;
        v29 = 0;
        P0 = poitB;
        P3 = poitC;
        ab = poitC - poitB;
        v11 = ab.Length;
        if (ab.Length <= 0.001)
        {
            P1 = P0;
            P2 = P3;
            Length = 0;
            OneDivLength = 0;
            if (a3)
                Field48 = Vector3d.UnitY;
        }
        else
        {
            direction = ab.Normalized();
            if (poitA != null)
            {
                diff = poitC - poitA.Value;
                if (diff.LengthSquared > 0.001 * 0.001)
                {
                    diff.Normalize();
                    v15 = Vector3d.Dot(direction, diff);
                    v8 = v11 * ((1 - v15) / 2);
                    if (a2)
                        a2a = diff * v11;
                    else
                        a2a = diff * (v11 + v8);

                    P1 = P0 + a2a / 3;
                }
                else
                    P1 = P0;
            }

            if (poitD != null)
            {
                src = poitD.Value - poitB;
                if (src.LengthSquared > 0.001 * 0.001)
                {
                    src.Normalize();
                    v17 = Vector3d.Dot(direction, src);
                    v29 = v11 * (1 - v17) / 2;
                    if (a2)
                        v36 = src * v11;
                    else
                        v36 = src * (v11 + v29);

                    P2 = P3 - v36 / 3;
                }
                else
                    P2 = P3;
            }
            else
                P2 = P3;

            LinLength = v11;
            OneDivLinLength = 1 / LinLength;
            HermLength = v29 + v11 + v8;
            OneDivHermLength = 1 / HermLength;
            Length = HermLength;
            OneDivLength = OneDivHermLength;
            if (a3)
            {
                v20 = Vector3d.Dot(direction, Vector3d.UnitY);
                Field48.X = -direction.X * v20;
                Field48.Y = 1 - direction.Y * v20;
                Field48.Z = -direction.Z * v20;
                if (Field48.LengthSquared < 0.001 * 0.001)
                    v24 = 0;
                else
                {
                    v24 = Field48.Length;
                    Field48.Normalize();
                }

                if (v24 == 0)
                    Field48 = Vector3d.UnitY;
            }
        }
    }
}
