using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj;

class Pathwalker
{
    public PathwalkerPath Path;
    public double Speed;
    public MkdsPath ResPath;
    public int PartIdx;
    public double PartSpeed;
    public double PartProgress;
    public bool IsForwards;
    public MkdsPathPoint PrevPoit;
    public MkdsPathPoint CurPoit;

    public double Progress => IsForwards ? PartProgress : 1 - PartProgress;
    public bool HasEnded => !Path.Loop && (IsForwards ? PartIdx == Path.Parts.Length - 1 : PartIdx == 0);

    public virtual void Init(int initialPoint, bool forwards)
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
        PartSpeed = v7 != 0 ? Speed * v7 : 1;
        PartProgress = forwards ? 0 : 1;
        IsForwards = forwards;

        int v10 = initialPoint;
        if (!IsForwards && Path.Loop && v10 == Path.Parts.Length)
            v10 = 0;
        PrevPoit = ResPath.Points[v10];

        int v14 = IsForwards ? initialPoint + 1 : initialPoint - 1;
        if (IsForwards && Path.Loop && v14 == Path.Parts.Length)
            v14 = 0;
        CurPoit = ResPath.Points[v14];
    }

    public virtual bool Update()
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
                        PartIdx = 0;
                        PrevPoit = CurPoit;
                        CurPoit = ResPath.Points[1];
                        double v4 = Path.Parts[PartIdx].OneDivLength;
                        if (v4 != 0)
                            PartProgress *= v4;
                        PartSpeed = v4 != 0 ? Speed * v4 : 1;
                    }
                    else
                    {
                        IsForwards = false;
                        PartProgress = 1;
                        PrevPoit = CurPoit;
                        CurPoit = ResPath.Points[PartIdx];
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
                        PartIdx = Path.Parts.Length - 1;
                        PrevPoit = CurPoit;
                        CurPoit = ResPath.Points[Path.Parts.Length - 1];
                        double v12 = Path.Parts[PartIdx].OneDivLength;
                        if (v12 != 0)
                            PartProgress *= v12;
                        PartProgress += 1;
                        PartSpeed = v12 != 0 ? Speed * v12 : 1;
                    }
                    else
                    {
                        IsForwards = true;
                        PartProgress = 0;
                        PrevPoit = CurPoit;
                        CurPoit = ResPath.Points[1];
                    }
                }
                else
                {
                    PartProgress *= Path.Parts[PartIdx].Length;
                    PartIdx--;
                    PrevPoit = CurPoit;
                    CurPoit = ResPath.Points[PartIdx];
                    double v14 = Path.Parts[PartIdx].OneDivLength;
                    if (v14 != 0)
                        PartProgress *= v14;
                    PartProgress += 1;
                    PartSpeed = v14 != 0 ? Speed * v14 : 1;
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
        IsForwards = !IsForwards;
    }

    public void SetSpeed(double speed)
    {
        Speed = speed;
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
            part.Length = part.LinLength;
            part.OneDivLength = part.OneDivLinLength;
        }
    }

    protected void InitFromPath(MkdsPath path, double speed)
    {
        ResPath = path;
        Path = new PathwalkerPath(path);
        Speed = speed;
        Init(0, true);
    }

    public static Pathwalker FromPath(MkdsPath path, double speed)
    {
        var pw = new Pathwalker();
        pw.InitFromPath(path, speed);
        return pw;
    }
}