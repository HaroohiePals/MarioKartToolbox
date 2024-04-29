using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Linq;
using Area = HaroohiePals.NitroKart.MapData.Intermediate.Sections.MkdsArea;
using MkdsPath = HaroohiePals.NitroKart.MapData.Intermediate.Sections.MkdsPath;

namespace HaroohiePals.NitroKart.MapData.Intermediate;

public static class MkdsMapDataFactory
{
    public static MkdsMapData CreateFromXml(byte[] xmlData) => MkdsMapData.FromXml(xmlData);

    public static MkdsMapData CreateFromNkm(Nkmd nkm)
    {
        bool isMgStage = nkm.MgEnemyPoint is not null && nkm.MgEnemyPath is not null;
        var  mapData   = new MkdsMapData
        {
            Version          = nkm.Header.Version,
            IsMgStage        = isMgStage,
            MapObjects       = ToMapDataCollection(nkm.ObjectInformation, x => new MkdsMapObject(x)),
            Paths            = ConvertPathPoit(nkm.Path, nkm.Point) ?? new MapDataCollection<MkdsPath>(),
            StageInfo        = nkm.Stage is null ? null : new MkdsStageInfo(nkm.Stage),
            StartPoints      = ToMapDataCollection(nkm.KartPointStart, x => new MkdsStartPoint(x)),
            RespawnPoints    = ToMapDataCollection(nkm.KartPointJugem, x => new MkdsRespawnPoint(x, isMgStage)),
            KartPoint2D      = ToMapDataCollection(nkm.KartPoint2d, x => new MkdsKartPoint2d(x)),
            CannonPoints     = ToMapDataCollection(nkm.KartPointCannon, x => new MkdsCannonPoint(x)),
            KartPointMission = ToMapDataCollection(nkm.KartPointMission, x => new MkdsKartPointMission(x)),
            CheckPointPaths  = ConvertCpatCpoi(nkm.CheckPointPath, nkm.CheckPoint) ?? new MapDataCollection<MkdsCheckPointPath>(),
            ItemPaths        = ConvertIpatIpoi(nkm.ItemPath, nkm.ItemPoint) ?? new MapDataCollection<MkdsItemPath>(),
            EnemyPaths       = ConvertEpatEpoi(nkm.EnemyPath, nkm.EnemyPoint) ?? new MapDataCollection<MkdsEnemyPath>(),
            MgEnemyPaths     = ConvertMepaMepo(nkm.MgEnemyPath, nkm.MgEnemyPoint) ?? new MapDataCollection<MkdsMgEnemyPath>(),
            Areas            = ToMapDataCollection(nkm.Area, x => new Area(x, isMgStage)),
            Cameras          = ToMapDataCollection(nkm.Camera, x => new MkdsCamera(x))
        };

        if (isMgStage)
            mapData.EnemyPaths = null;
        else
            mapData.MgEnemyPaths = null;

        mapData.ResolveReferences();

        return mapData;
    }

    private static MapDataCollection<TTarget> ToMapDataCollection<TSource, TTarget>(NkmdSection<TSource> section,
        Func<TSource, TTarget> convertFunc)
        where TSource : NkmdSectionEntry
        where TTarget : IMapDataEntry
    {
        return section is null ? null : new MapDataCollection<TTarget>(section.Entries.Select(convertFunc));
    }

    private static MapDataCollection<MkdsPath> ConvertPathPoit(NkmdPath pathSection, NkmdPoit poitSection)
    {
        if (pathSection is null || poitSection is null)
            return null;

        try
        {
            var paths = new MapDataCollection<MkdsPath>();

            int poitIndex = 0;

            foreach (var pathEntry in pathSection.Entries)
            {
                var entry = new MkdsPath(pathEntry);

                for (var i = 0; i < pathEntry.NrPoit; i++)
                    entry.Points.Add(new MkdsPathPoint(poitSection[poitIndex++]));

                paths.Add(entry);
            }

            return paths;
        }
        catch
        {
            //todo: handle error
            return null;
        }
    }

    private static MapDataCollection<MkdsCheckPointPath> ConvertCpatCpoi(NkmdCpat cpatSection, NkmdCpoi cpoiSection)
    {
        if (cpatSection is null || cpoiSection is null)
            return null;

        try
        {
            var pathSection = new MapDataCollection<MkdsCheckPointPath>();
            for (int i = 0; i < cpatSection.Entries.Count; i++)
                pathSection.Add(new MkdsCheckPointPath(cpatSection[i], cpoiSection.Entries));

            return pathSection;
        }
        catch
        {
            //todo: handle error
            return null;
        }
    }

    private static MapDataCollection<MkdsItemPath> ConvertIpatIpoi(NkmdIpat ipatSection, NkmdIpoi ipoiSection)
    {
        if (ipatSection is null || ipoiSection is null)
            return null;

        try
        {
            var pathSection = new MapDataCollection<MkdsItemPath>();
            for (int i = 0; i < ipatSection.Entries.Count; i++)
                pathSection.Add(new MkdsItemPath(ipatSection[i], ipoiSection.Entries));

            return pathSection;
        }
        catch
        {
            //todo: handle error
            return null;
        }
    }

    private static MapDataCollection<MkdsEnemyPath> ConvertEpatEpoi(NkmdEpat epatSection, NkmdEpoi epoiSection)
    {
        if (epatSection is null || epoiSection is null)
            return null;

        try
        {
            var pathSection = new MapDataCollection<MkdsEnemyPath>();
            for (int i = 0; i < epatSection.Entries.Count; i++)
                pathSection.Add(new MkdsEnemyPath(epatSection[i], epoiSection.Entries));

            return pathSection;
        }
        catch (Exception ex)
        {
            //todo: handle error
            return null;
        }
    }

    private static MapDataCollection<MkdsMgEnemyPath> ConvertMepaMepo(NkmdMepa mepaSection, NkmdMepo mepoSection)
    {
        if (mepaSection is null || mepoSection is null)
            return null;

        try
        {
            var pathSection = new MapDataCollection<MkdsMgEnemyPath>();

            for (int i = 0; i < mepaSection.Entries.Count; i++)
                pathSection.Add(new MkdsMgEnemyPath(mepaSection[i], mepoSection.Entries));

            return pathSection;
        }
        catch
        {
            //todo: handle error
            return null;
        }
    }
}