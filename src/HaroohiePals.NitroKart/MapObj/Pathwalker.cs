using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj
{
    public class Pathwalker
    {
        public class PwPathPart
        {
            public Vector3d P0;
            public Vector3d P1;
            public Vector3d P2;
            public Vector3d P3;
            public double   Length;
            public double   OneDivLength;
            public double   HermLength;
            public double   OneDivHermLength;
            public double   LinLength;
            public double   OneDivLinLength;
            public Vector3d Field48;

            public PwPathPart(bool a2, bool a3, in Vector3d? poitA, in Vector3d poitB, in Vector3d poitC,
                in Vector3d? poitD)
            {
                double   v8;
                double   v11;
                double   v15;
                double   v17;
                double   v20;
                double   v24;
                double   v29;
                Vector3d ab;
                Vector3d direction;
                Vector3d diff;
                Vector3d a2a;
                Vector3d src;
                Vector3d v36;

                v8  = 0;
                v29 = 0;
                P0  = poitB;
                P3  = poitC;
                ab  = poitC - poitB;
                v11 = ab.Length;
                if (ab.Length <= 0.001)
                {
                    P1           = P0;
                    P2           = P3;
                    Length       = 0;
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
                            v8  = v11 * ((1 - v15) / 2);
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

                    LinLength        = v11;
                    OneDivLinLength  = 1 / LinLength;
                    HermLength       = v29 + v11 + v8;
                    OneDivHermLength = 1 / HermLength;
                    Length           = HermLength;
                    OneDivLength     = OneDivHermLength;
                    if (a3)
                    {
                        v20       = Vector3d.Dot(direction, Vector3d.UnitY);
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

        public class PwPath
        {
            public PwPathPart[] Parts;
            public bool         Loop;

            public PwPath(MkdsPath path)
            {
                Loop  = path.Loop;
                Parts = new PwPathPart[Loop ? path.Points.Count : path.Points.Count - 1];
                if (path.Points.Count == 2)
                {
                    Parts[0] = new PwPathPart(false, true,
                        null, path.Points[0].Position, path.Points[1].Position, null);
                    if (Loop)
                    {
                        Parts[1] = new PwPathPart(false, true,
                            null, path.Points[1].Position, path.Points[0].Position, null);
                    }
                }
                else
                {
                    if (Loop)
                    {
                        Parts[0] = new PwPathPart(false, true,
                            path.Points[^1].Position, path.Points[0].Position,
                            path.Points[1].Position, path.Points[2].Position);
                    }
                    else
                    {
                        Parts[0] = new PwPathPart(false, true,
                            null, path.Points[0].Position, path.Points[1].Position,
                            path.Points[2].Position);
                    }

                    int v16;
                    for (v16 = 1; v16 < path.Points.Count - 2; v16++)
                    {
                        Parts[v16] = new PwPathPart(false, true,
                            path.Points[v16 - 1].Position, path.Points[v16].Position,
                            path.Points[v16 + 1].Position, path.Points[v16 + 2].Position);
                    }

                    if (Loop)
                    {
                        Parts[v16] = new PwPathPart(false, true,
                            path.Points[v16 - 1].Position, path.Points[v16].Position,
                            path.Points[v16 + 1].Position, path.Points[0].Position);
                    }
                    else
                    {
                        Parts[v16] = new PwPathPart(false, true,
                            path.Points[v16 - 1].Position, path.Points[v16].Position,
                            path.Points[v16 + 1].Position, null);
                    }

                    if (Loop)
                    {
                        Parts[v16 + 1] = new PwPathPart(false, true,
                            path.Points[v16].Position, path.Points[v16 + 1].Position,
                            path.Points[0].Position, path.Points[1].Position);
                    }
                }
            }
        }

        public PwPath    Path;
        public double    Speed;
        public MkdsPath      ResPath;
        public int       PartIdx;
        public double    PartSpeed;
        public double    PartProgress;
        public bool      IsForwards;
        public MkdsPathPoint PrevPoit;
        public MkdsPathPoint CurPoit;

        public void Init(int initialPoint, bool forwards)
        {
            if (Path.Loop)
            {
                if (initialPoint == 0 && !forwards)
                    initialPoint = Path.Parts.Length;
            }
            else if (initialPoint == 0 && !forwards)
                forwards = true;
            else if (initialPoint == Path.Parts.Length && forwards)
                forwards = false;

            PartIdx = forwards ? initialPoint : initialPoint - 1;
            double v7 = Path.Parts[PartIdx].OneDivLength;
            PartSpeed    = v7 != 0 ? Speed * v7 : 1;
            PartProgress = forwards ? 0 : 1;
            IsForwards   = forwards;

            int v10 = initialPoint;
            if (!IsForwards && Path.Loop && v10 == Path.Parts.Length)
                v10 = 0;
            PrevPoit = ResPath.Points[v10];

            int v14 = IsForwards ? initialPoint + 1 : initialPoint - 1;
            if (IsForwards && Path.Loop && v14 == Path.Parts.Length)
                v14 = 0;
            CurPoit = ResPath.Points[v14];
        }

        public bool Update()
        {
            if (IsForwards)
            {
                PartProgress += PartSpeed;
                if (PartProgress >= 1)
                {
                    if (PartIdx == Path.Parts.Length - 1)
                    {
                        if (Path.Loop)
                        {
                            PartProgress -= 1;
                            PartProgress *= Path.Parts[PartIdx].Length;
                            PartIdx      =  0;
                            PrevPoit     =  CurPoit;
                            CurPoit      =  ResPath.Points[1];
                            double v4 = Path.Parts[PartIdx].OneDivLength;
                            if (v4 != 0)
                                PartProgress *= v4;
                            PartSpeed = v4 != 0 ? Speed * v4 : 1;
                        }
                        else
                        {
                            IsForwards   = false;
                            PartProgress = 1;
                            PrevPoit     = CurPoit;
                            CurPoit      = ResPath.Points[PartIdx];
                        }
                    }
                    else
                    {
                        PartProgress -= 1;
                        PartProgress *= Path.Parts[PartIdx].Length;
                        PartIdx++;
                        PrevPoit = CurPoit;
                        if (PartIdx == Path.Parts.Length - 1 && Path.Loop)
                            CurPoit = ResPath.Points[0];
                        else
                            CurPoit = ResPath.Points[PartIdx + 1];
                        double v7 = Path.Parts[PartIdx].OneDivLength;
                        if (v7 != 0)
                            PartProgress *= v7;
                        PartSpeed = v7 != 0 ? Speed * v7 : 1;
                    }

                    return true;
                }
            }
            else
            {
                PartProgress -= PartSpeed;
                if (PartProgress <= 0)
                {
                    if (PartIdx == 0)
                    {
                        if (Path.Loop)
                        {
                            PartProgress *= Path.Parts[PartIdx].Length;
                            PartIdx      =  Path.Parts.Length - 1;
                            PrevPoit     =  CurPoit;
                            CurPoit      =  ResPath.Points[Path.Parts.Length - 1];
                            double v12 = Path.Parts[PartIdx].OneDivLength;
                            if (v12 != 0)
                                PartProgress *= v12;
                            PartProgress += 1;
                            PartSpeed    =  v12 != 0 ? Speed * v12 : 1;
                        }
                        else
                        {
                            IsForwards   = true;
                            PartProgress = 0;
                            PrevPoit     = CurPoit;
                            CurPoit      = ResPath.Points[1];
                        }
                    }
                    else
                    {
                        PartProgress *= Path.Parts[PartIdx].Length;
                        PartIdx--;
                        PrevPoit = CurPoit;
                        CurPoit  = ResPath.Points[PartIdx];
                        double v14 = Path.Parts[PartIdx].OneDivLength;
                        if (v14 != 0)
                            PartProgress *= v14;
                        PartProgress += 1;
                        PartSpeed    =  v14 != 0 ? Speed * v14 : 1;
                    }

                    return true;
                }
            }

            return false;
        }

        public void GotoPartEnd()
        {
            PartProgress = 1;
        }

        public void Reverse()
        {
            (PrevPoit, CurPoit) = (CurPoit, PrevPoit);
            IsForwards          = !IsForwards;
        }

        public void SetSpeed(double speed)
        {
            Speed     = speed;
            PartSpeed = speed * Path.Parts[PartIdx].OneDivLength;
        }

        public Vector3d CalcCurrentPointXYZ()
            => PathInterpolate.InterpolateXYZ(Path.Parts[PartIdx], PartProgress);

        public void pw_20D8BF8_XYZ(out Vector3d a2, out Vector3d a3)
        {
            PathInterpolate.pw_20D939C(Path.Parts[PartIdx], PartProgress, PartSpeed, out a2, out a3);
            if (!IsForwards)
                a3 = -a3;
        }

        public Vector3d CalcCurrentPointXZLinearY()
            => PathInterpolate.InterpolateXZLinearY(Path.Parts[PartIdx], PartProgress);

        public Vector3d CalcCurrentPointXZ()
            => PathInterpolate.InterpolateXZ(Path.Parts[PartIdx], PartProgress);

        public void pw_20D8B18_XZ(out Vector3d a2, out Vector3d a3)
        {
            PathInterpolate.pw_20D9270_XZ(Path.Parts[PartIdx], PartProgress, PartSpeed, out a2, out a3);
            if (!IsForwards)
                a3 = -a3;
        }

        public Vector3d CalcCurrentPointLinearXYZ()
            => Path.Parts[PartIdx].P0 * (1 - PartProgress) + Path.Parts[PartIdx].P3 * PartProgress;

        public void CalcCurrentPointLinearXYZSpecial(out Vector3d a2, out Vector3d a3)
        {
            a2 = Path.Parts[PartIdx].P0 * (1 - PartProgress) + Path.Parts[PartIdx].P3 * PartProgress;
            a3 = (Path.Parts[PartIdx].P3 - Path.Parts[PartIdx].P0) * PartSpeed;
            if (!IsForwards)
                a3 = -a3;
        }

        public Vector3d CalcCurrentPointLinearXZ()
        {
            var result = Path.Parts[PartIdx].P0 * (1 - PartProgress) + Path.Parts[PartIdx].P3 * PartProgress;
            result.Y = 0;
            return result;
        }

        public void MakeLengthLinear()
        {
            foreach (var part in Path.Parts)
            {
                part.Length       = part.LinLength;
                part.OneDivLength = part.OneDivLinLength;
            }
        }

        public double Progress => IsForwards ? PartProgress : 1 - PartProgress;
        public bool   HasEnded => !Path.Loop && (IsForwards ? PartIdx == Path.Parts.Length - 1 : PartIdx == 0);

        public static Pathwalker FromPath(MkdsPath path, double speed)
        {
            var pw = new Pathwalker();
            pw.ResPath = path;
            pw.Path    = new PwPath(path);
            pw.Speed   = speed;
            pw.Init(0, true);
            return pw;
        }
    }
}