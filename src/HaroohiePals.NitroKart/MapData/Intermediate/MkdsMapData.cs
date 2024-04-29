using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate;

public class MkdsMapData : IMapData
{
    private static readonly XmlSerializer Serializer = new(typeof(MkdsMapData),
        new OverrideXml()
            .Override<Vector3d>()
            .Member(nameof(Vector3d.X)).XmlAttribute("x")
            .Member(nameof(Vector3d.Y)).XmlAttribute("y")
            .Member(nameof(Vector3d.Z)).XmlAttribute("z")
            .Override<Vector2d>()
            .Member(nameof(Vector2d.X)).XmlAttribute("x")
            .Member(nameof(Vector2d.Y)).XmlAttribute("y")
            .Commit());

    [XmlAttribute("version")]
    public ushort Version { get; set; } = 37;

    public MapDataCollection<MkdsMapObject>        MapObjects;
    public MapDataCollection<MkdsPath>             Paths;
    public MkdsStageInfo                           StageInfo;
    public MapDataCollection<MkdsStartPoint>       StartPoints;
    public MapDataCollection<MkdsRespawnPoint>     RespawnPoints;
    public MapDataCollection<MkdsKartPoint2d>      KartPoint2D;
    public MapDataCollection<MkdsCannonPoint>      CannonPoints;
    public MapDataCollection<MkdsKartPointMission> KartPointMission;
    public MapDataCollection<MkdsCheckPointPath>   CheckPointPaths;
    public MapDataCollection<MkdsItemPath>         ItemPaths;
    public MapDataCollection<MkdsEnemyPath>        EnemyPaths;
    public MapDataCollection<MkdsMgEnemyPath>      MgEnemyPaths;
    public MapDataCollection<MkdsArea>             Areas;
    public MapDataCollection<MkdsCamera>           Cameras;

    [XmlAttribute("isMgStage"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DefaultValue(false)]
    public bool IsMgStage { get; set; }

    public MkdsMapData() { }

    public MkdsMapData(bool isMgStage)
    {
        IsMgStage = isMgStage;

        MapObjects = new();
        Paths      = new();
        StageInfo  = new();

        StageInfo.NrLaps = (short)(isMgStage ? 100 : 3);

        StartPoints      = new();
        RespawnPoints    = new();
        KartPoint2D      = new();
        CannonPoints     = new();
        KartPointMission = new();
        CheckPointPaths  = new();
        ItemPaths        = new();
        if (!isMgStage)
            EnemyPaths = new();
        else
            MgEnemyPaths = new();
        Areas   = new();
        Cameras = new();
    }

    public byte[] WriteXml()
    {
        var m   = new MemoryStream();
        var xns = new XmlSerializerNamespaces();
        xns.Add(String.Empty, String.Empty);

        var serializerCollection = new ReferenceSerializerCollection();
        var referenceSerializer  = new MkdsMapDataReferenceSerializer(this);
        serializerCollection.RegisterSerializer<Sections.MkdsPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsRespawnPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsCheckPointPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsItemPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsItemPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsEnemyPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsEnemyPath, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsMgEnemyPoint, int>(referenceSerializer);
        serializerCollection.RegisterSerializer<MkdsCamera, int>(referenceSerializer);

        using (var writer = new MapDataXmlTextWriter(new StreamWriter(m), serializerCollection))
        {
            writer.Formatting = Formatting.Indented;
            Serializer.Serialize(writer, this, xns);
            writer.Close();
        }

        return m.ToArray();
    }

    public static MkdsMapData FromXml(byte[] data)
    {
        return FromXml(new MemoryStream(data));
    }

    public static MkdsMapData FromXml(MemoryStream m)
    {
        var     deserializer = Serializer;
        MkdsMapData data;
        using (var reader = new StreamReader(m))
            data = (MkdsMapData)deserializer.Deserialize(reader);

        if (!data.IsMgStage)
            data.MgEnemyPaths = null;
        else
            data.EnemyPaths = null;

        if (data.Version < 32)
            data.CannonPoints = null;

        if (data.Version < 36)
            data.KartPointMission = null;

        data.ResolveReferences();
        return data;
    }

    public void ResolveReferences()
    {
        var resolverCollection = new ReferenceResolverCollection();
        var referenceResolver  = new MkdsMapDataReferenceResolver(this);
        resolverCollection.RegisterResolver<MkdsPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsRespawnPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsCheckPointPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsItemPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsItemPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsEnemyPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsEnemyPath>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsMgEnemyPoint>(referenceResolver);
        resolverCollection.RegisterResolver<MkdsCamera>(referenceResolver);

        MapObjects?.ResolveReferences(resolverCollection);
        Paths?.ResolveReferences(resolverCollection);
        RespawnPoints?.ResolveReferences(resolverCollection);
        CannonPoints?.ResolveReferences(resolverCollection);
        CheckPointPaths?.ResolveReferences(resolverCollection);
        ItemPaths?.ResolveReferences(resolverCollection);
        EnemyPaths?.ResolveReferences(resolverCollection);
        MgEnemyPaths?.ResolveReferences(resolverCollection);
        Areas?.ResolveReferences(resolverCollection);
        Cameras?.ResolveReferences(resolverCollection);
    }
}