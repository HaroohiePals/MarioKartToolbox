using HaroohiePals.IO.Reference;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System.Collections.Generic;
using System.Linq;
using NkmdArea = HaroohiePals.NitroKart.MapData.Binary.NkmdArea;
using NkmdPath = HaroohiePals.NitroKart.MapData.Binary.NkmdPath;

namespace HaroohiePals.NitroKart.MapData.Intermediate;

public static class NkmdFactory
{
    public static Nkmd FromMapData(MkdsMapData mapData)
    {
        var nkm = new Nkmd();
        nkm.Header.Version = mapData.Version;

        var serializerCollection = new ReferenceSerializerCollection();
        var referenceSerializer  = new MkdsMapDataReferenceSerializer(mapData);
        serializerCollection.RegisterSerializer<MkdsPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsRespawnPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsCheckPointPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsItemPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsItemPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsEnemyPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsEnemyPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsMgEnemyPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsCamera, int>(referenceSerializer);

        nkm.ObjectInformation                = ConvertMapObjects(mapData.MapObjects, serializerCollection);
        (nkm.Path, nkm.Point)                = ConvertPaths(mapData.Paths);
        nkm.Stage                            = mapData.StageInfo?.ToStag();
        nkm.KartPointStart                   = ConvertStartPoints(mapData.StartPoints);
        nkm.KartPointJugem                   = ConvertRespawnPoints(mapData, serializerCollection);
        nkm.KartPoint2d                      = ConvertKartPoint2d(mapData.KartPoint2D);
        nkm.KartPointCannon                  = ConvertCannonPoints(mapData.CannonPoints, serializerCollection);
        nkm.KartPointMission                 = ConvertKartPointMission(mapData.KartPointMission);
        (nkm.CheckPointPath, nkm.CheckPoint) = ConvertCheckPointPaths(mapData.CheckPointPaths, serializerCollection);
        (nkm.ItemPath, nkm.ItemPoint)        = ConvertItemPaths(mapData, serializerCollection);
        (nkm.EnemyPath, nkm.EnemyPoint)      = ConvertEnemyPaths(mapData.EnemyPaths, serializerCollection);
        (nkm.MgEnemyPath, nkm.MgEnemyPoint)  = ConvertMgEnemyPaths(mapData.MgEnemyPaths, serializerCollection);
        nkm.Area                             = ConvertAreas(mapData.Areas, serializerCollection);
        nkm.Camera                           = ConvertCameras(mapData.Cameras, serializerCollection);

        return nkm;
    }

    private static NkmdObji ConvertMapObjects(IEnumerable<MkdsMapObject> mapObjects,
        IReferenceSerializerCollection serializerCollection)
    {
        if (mapObjects is null)
            return null;

        var obji = new NkmdObji();
        obji.Entries.AddRange(mapObjects.Select(x => x.ToObjiEntry(serializerCollection)));
        return obji;
    }

    private static (NkmdPath, NkmdPoit) ConvertPaths(IEnumerable<Sections.MkdsPath> paths)
    {
        if (paths is null)
            return (null, null);

        try
        {
            var path = new NkmdPath();
            var poit = new NkmdPoit();

            byte pathIndex = 0;

            foreach (var pathEntry in paths)
            {
                var entry = pathEntry.ToPathEntry();

                entry.Index = pathIndex++;
                byte poitIndex = 0;

                foreach (var pathPointEntry in pathEntry.Points)
                {
                    var poitEntry = pathPointEntry.ToPoitEntry();
                    poitEntry.Index = poitIndex++;
                    poit.Entries.Add(poitEntry);
                }

                path.Entries.Add(entry);
            }

            return (path, poit);
        }
        catch
        {
            //todo: handle error
            return (null, null);
        }
    }

    private static NkmdKtps ConvertStartPoints(IEnumerable<MkdsStartPoint> startPoints)
    {
        if (startPoints is null)
            return null;

        var ktps = new NkmdKtps();
        ktps.Entries.AddRange(startPoints.Select(x => x.ToKtpsEntry()));
        return ktps;
    }

    private static NkmdKtpj ConvertRespawnPoints(MkdsMapData mapData, IReferenceSerializerCollection serializerCollection)
    {
        if (mapData.RespawnPoints is null)
            return null;

        var ktpj = new NkmdKtpj(mapData.Version);
        ktpj.Entries.AddRange(mapData.RespawnPoints.Select(x => x.ToKtpjEntry(serializerCollection)));
        return ktpj;
    }

    private static NkmdKtp2 ConvertKartPoint2d(IEnumerable<MkdsKartPoint2d> kartPoint2d)
    {
        if (kartPoint2d is null)
            return null;

        var ktp2 = new NkmdKtp2();
        ktp2.Entries.AddRange(kartPoint2d.Select(x => x.ToKtp2Entry()));
        return ktp2;
    }

    private static NkmdKtpc ConvertCannonPoints(IEnumerable<MkdsCannonPoint> cannonPoints,
        IReferenceSerializerCollection serializerCollection)
    {
        if (cannonPoints is null)
            return null;

        var ktpc = new NkmdKtpc();
        ktpc.Entries.AddRange(cannonPoints.Select(x => x.ToKtpcEntry(serializerCollection)));
        return ktpc;
    }

    private static NkmdKtpm ConvertKartPointMission(IEnumerable<MkdsKartPointMission> kartPointMission)
    {
        if (kartPointMission is null)
            return null;

        var ktpm = new NkmdKtpm();
        ktpm.Entries.AddRange(kartPointMission.Select(x => x.ToKtpmEntry()));
        return ktpm;
    }

    private static (NkmdCpat, NkmdCpoi) ConvertCheckPointPaths(IEnumerable<MkdsCheckPointPath> checkPointPaths,
        IReferenceSerializerCollection serializerCollection)
    {
        if (checkPointPaths is null)
            return (null, null);

        var points = new NkmdCpoi();
        var paths  = new NkmdCpat();

        var newPoints = new List<NkmdCpoi.CpoiEntry>();
        foreach (var path in checkPointPaths)
        {
            var cpatEntry = new NkmdCpat.CpatEntry
            {
                StartIndex = (short)newPoints.Count,
                Length     = (short)path.Points.Count
            };
            for (int j = 0; j < 3; j++)
            {
                if (j < path.Previous.Count)
                    cpatEntry.Previous[j] =
                        (byte)serializerCollection.Serialize<MkdsCheckPointPath, int>(path.Previous[j]);
                else
                    cpatEntry.Previous[j] = 0xFF;
                if (j < path.Next.Count)
                    cpatEntry.Next[j] =
                        (byte)serializerCollection.Serialize<MkdsCheckPointPath, int>(path.Next[j]);
                else
                    cpatEntry.Next[j] = 0xFF;
            }

            newPoints.AddRange(path.Points.Select(x => x.ToCpoiEntry(serializerCollection)));
            paths.Entries.Add(cpatEntry);
        }

        points.Entries.AddRange(newPoints);

        return (paths, points);
    }

    private static (NkmdIpat, NkmdIpoi) ConvertItemPaths(MkdsMapData mapData, IReferenceSerializerCollection serializerCollection)
    {
        if (mapData.ItemPaths is null)
            return (null, null);

        var points = new NkmdIpoi(mapData.Version);
        var paths  = new NkmdIpat();

        var newPoints = new List<NkmdIpoi.IpoiEntry>();
        foreach (var path in mapData.ItemPaths)
        {
            var ipatEntry = new NkmdIpatEpatEntry
            {
                StartIndex = (short)newPoints.Count,
                Length     = (short)path.Points.Count
            };
            for (int j = 0; j < 3; j++)
            {
                if (j < path.Previous.Count)
                    ipatEntry.Previous[j] =
                        (byte)serializerCollection.Serialize<MkdsItemPath, int>(path.Previous[j]);
                else
                    ipatEntry.Previous[j] = 0xFF;
                if (j < path.Next.Count)
                    ipatEntry.Next[j] = (byte)serializerCollection.Serialize<MkdsItemPath, int>(path.Next[j]);
                else
                    ipatEntry.Next[j] = 0xFF;
            }

            newPoints.AddRange(path.Points.Select(x => x.ToIpoiEntry()));
            paths.Entries.Add(ipatEntry);
        }

        points.Entries.AddRange(newPoints);

        return (paths, points);
    }

    private static (NkmdEpat, NkmdEpoi) ConvertEnemyPaths(IEnumerable<MkdsEnemyPath> enemyPaths,
        IReferenceSerializerCollection serializerCollection)
    {
        if (enemyPaths is null)
            return (null, null);

        var points = new NkmdEpoi();
        var paths  = new NkmdEpat();

        var newPoints = new List<NkmdEpoi.EpoiEntry>();
        foreach (var path in enemyPaths)
        {
            var epatEntry = new NkmdIpatEpatEntry
            {
                StartIndex = (short)newPoints.Count,
                Length     = (short)path.Points.Count
            };
            for (int j = 0; j < 3; j++)
            {
                if (j < path.Previous.Count)
                    epatEntry.Previous[j] =
                        (byte)serializerCollection.Serialize<MkdsEnemyPath, int>(path.Previous[j]);
                else
                    epatEntry.Previous[j] = 0xFF;
                if (j < path.Next.Count)
                    epatEntry.Next[j] = (byte)serializerCollection.Serialize<MkdsEnemyPath, int>(path.Next[j]);
                else
                    epatEntry.Next[j] = 0xFF;
            }

            newPoints.AddRange(path.Points.Select(x => x.ToEpoiEntry()));
            paths.Entries.Add(epatEntry);
        }

        points.Entries.AddRange(newPoints);

        return (paths, points);
    }

    private static (NkmdMepa, NkmdMepo) ConvertMgEnemyPaths(IEnumerable<MkdsMgEnemyPath> mgEnemyPaths,
        IReferenceSerializerCollection serializerCollection)
    {
        if (mgEnemyPaths is null)
            return (null, null);

        var points = new NkmdMepo();
        var paths  = new NkmdMepa();

        var newPoints = new List<NkmdMepo.MepoEntry>();

        foreach (var path in mgEnemyPaths)
        {
            var mepaEntry = new NkmdMepa.MepaEntry();
            mepaEntry.StartIndex = (short)newPoints.Count;
            mepaEntry.Length     = (short)path.Points.Count;
            for (int j = 0; j < 8; j++)
            {
                if (j < path.Previous.Count)
                {
                    mepaEntry.Previous[j] =
                        (byte)serializerCollection.Serialize<MkdsMgEnemyPoint, int>(path.Previous[j]);
                }
                else
                    mepaEntry.Previous[j] = 0xFF;

                if (j < path.Next.Count)
                {
                    mepaEntry.Next[j] =
                        (byte)serializerCollection.Serialize<MkdsMgEnemyPoint, int>(path.Next[j]);
                }
                else
                    mepaEntry.Next[j] = 0xFF;
            }

            newPoints.AddRange(path.Points.Select(x => x.ToMepoEntry()));
            paths.Entries.Add(mepaEntry);
        }

        points.Entries.AddRange(newPoints);

        return (paths, points);
    }

    private static NkmdArea ConvertAreas(IEnumerable<Sections.MkdsArea> areas,
        IReferenceSerializerCollection serializerCollection)
    {
        if (areas is null)
            return null;

        var area = new NkmdArea();
        area.Entries.AddRange(areas.Select(x => x.ToAreaEntry(serializerCollection)));
        return area;
    }

    private static NkmdCame ConvertCameras(IEnumerable<MkdsCamera> cameras, IReferenceSerializerCollection serializerCollection)
    {
        if (cameras is null)
            return null;

        var came = new NkmdCame();
        came.Entries.AddRange(cameras.Select(x => x.ToCameEntry(serializerCollection)));
        return came;
    }
}