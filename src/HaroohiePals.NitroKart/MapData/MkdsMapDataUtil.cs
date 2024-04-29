using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.Race;
using OpenTK.Mathematics;
using System.Linq;

namespace HaroohiePals.NitroKart.MapData;

public static class MkdsMapDataUtil
{
    private static readonly int[,] _driverPlacements = new[,]
    {
        {1, 0}, {2, 0}, {3, 0}, {4, 0},
        {3, 2}, {3, 3}, {4, 3}, {4, 4}
    };

    private static MkdsStartPoint CalculateStartPosition(MkdsMapData mapData, int driverId, double xpos, double zpos, out Vector3d position, out Vector3d rotation)
    {
        if (mapData.StageInfo.PolePosition == 1)
            xpos = 1 - xpos;

        MkdsStartPoint ktps = mapData.StartPoints[0];

        if (mapData.StartPoints.Count > 1)
        {
            for (int i = 0; i < mapData.StartPoints.Count; i++)
            {
                var curKtps = mapData.StartPoints[i];
                if (driverId == curKtps.Index)
                {
                    ktps = curKtps;
                    break;
                }
            }
        }

        var rotMtx = Matrix4d.CreateRotationX(MathHelper.DegreesToRadians(ktps.Rotation.X)) *
            Matrix4d.CreateRotationY(MathHelper.DegreesToRadians(ktps.Rotation.Y)) *
            Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians(ktps.Rotation.Z));

        double mtx00 = -rotMtx.Row0[0];
        double mtx01 = -rotMtx.Row0[1];
        double mtx02 = -rotMtx.Row0[2];
        double mtx20 = -rotMtx.Row2[0];
        double mtx21 = -rotMtx.Row2[1];
        double mtx22 = -rotMtx.Row2[2];

        position = new(ktps.Position.X + (mtx00 * xpos) + (mtx20 * zpos),
            ktps.Position.Y + (mtx01 * xpos) + (mtx21 * zpos),
            ktps.Position.Z + (mtx02 * xpos) + (mtx22 * zpos));
        rotation = ktps.Rotation;

        return ktps;
    }

    public static MkdsStartPoint GetStartPosition(MkdsMapData mapData, RaceConfig raceConfig, int player, out Vector3d position, out Vector3d rotation)
    {
        int raceStatusField4F8 = 0;

        MkdsStartPoint startPoint = null;

        int tableIdx = raceConfig.DriverCount - 1;
        int driverIdx = player;
        if (raceConfig.DisplayMode == RaceDisplayMode.StaffRoll)
            startPoint = CalculateStartPosition(mapData, 0, 0, 0, out position, out rotation);
        else
        {
            switch (raceConfig.Mode)
            {
                case RaceMode.MiniGame:
                    startPoint = CalculateStartPosition(mapData, (driverIdx + raceStatusField4F8) % 8, 0, 0, out position, out rotation);
                    break;
                case RaceMode.TimeAttack:
                    startPoint = CalculateStartPosition(mapData, 0, 0.5, 0, out position, out rotation);
                    break;
                case RaceMode.MissionRun:
                    if (raceConfig.DriverCount <= 1)
                        startPoint = CalculateStartPosition(mapData, 0, 0.5, 0, out position, out rotation);
                    else

                        startPoint = CalculateStartPosition(mapData, player, 20 * (1 - 2 * player) + 0.5, 0, 
                            out position, out rotation);
                    break;
                default:
                    int firstRowSlotCount = _driverPlacements[tableIdx, 0];
                    int driverOffset = driverIdx;
                    int driverRowSlotCount = firstRowSlotCount;
                    if (driverOffset >= firstRowSlotCount)
                    {
                        driverRowSlotCount = _driverPlacements[tableIdx, 1];
                        driverOffset -= firstRowSlotCount;
                    }
                    int driverXSlot;
                    if ((driverRowSlotCount & 1) == 1) //odd
                        driverXSlot = 2 * (driverOffset - driverRowSlotCount / 2);
                    else //even
                        driverXSlot = 2 * driverOffset + 1 - driverRowSlotCount;
                    if (driverIdx >= firstRowSlotCount && firstRowSlotCount == _driverPlacements[tableIdx, 1])
                        driverXSlot++;
                    double driverXPos = driverXSlot * 17d;
                    bool isSecondRow = false;
                    if (driverIdx >= firstRowSlotCount)
                        isSecondRow = true;
                    startPoint = CalculateStartPosition(mapData, 0, driverXPos,
                        25 * driverIdx + (isSecondRow ? 20d : 0), out position, out rotation);
                    break;
            }
        }

        return startPoint;
    }

    public static MkdsCamera FindCamera(MkdsMapData mapData, Vector3d point)
    {
        foreach (var area in mapData.Areas.Where(x => x.AreaType == MkdsAreaType.Camera))
        {
            if (IsPointInsideArea(point, area))
                return area.Camera?.Target;
        }

        return null;
    }

    private static bool IsPointInsideBoxArea(Vector3d point, MkdsArea area)
    {
        var ab = point - area.Position;
        double ydot = Vector3d.Dot(ab, area.YVector);
        if (ydot > (100 * area.LengthVector.Y) || ydot < 0)
            return false;
        double xdot = Vector3d.Dot(ab, area.XVector);
        double xlen = 50 * area.LengthVector.X;
        if (xdot < -xlen || xdot > xlen)
            return false;
        double zdot = Vector3d.Dot(ab, area.ZVector);
        double zlen = 50 * area.LengthVector.Z;
        if (zdot < -zlen || zdot > zlen)
            return false;
        return true;
    }

    private static bool IsPointInsideCylinderArea(Vector3d point, MkdsArea area)
    {
        var ab = point - area.Position;
        double dot = Vector3d.Dot(ab, area.YVector);
        if (dot > (50 * area.LengthVector.Y))
            return false;
        double res1 = Vector3d.Dot(ab, area.XVector);
        double res2 = Vector3d.Dot(ab, area.ZVector);
        double len = 50 * area.LengthVector.X;
        if ((len * len) < (res1 * res1) + (res2 * res2))
            return false;
        return true;
    }

    public static bool IsPointInsideArea(Vector3d point, MkdsArea area)
    {
        switch (area.Shape)
        {
            case MkdsAreaShapeType.Box:
                return IsPointInsideBoxArea(point, area);
            case MkdsAreaShapeType.Cylinder:
                return IsPointInsideCylinderArea(point, area);
        }

        return false;
    }
}
