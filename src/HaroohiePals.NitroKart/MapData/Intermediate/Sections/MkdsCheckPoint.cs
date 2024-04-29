using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsCheckPoint : IMapDataEntry
{
    [Category("Points"), DisplayName("Point 1")]
    [ListViewColumn(0, "X1", "Z1")]
    public Vector2d Point1 { get; set; }

    [Category("Points"), DisplayName("Point 2")]
    [ListViewColumn(1, "X2", "X2")]
    public Vector2d Point2 { get; set; }

    [Category("Sections"), DisplayName("Goto Section"), DefaultValue((short)-1)]
    [Description("Specifies if the next checkpoint is in a new section. Use -1 otherwise.")]
    [XmlAttribute("gotoSection")]
    [ListViewColumn(2, "Goto Sect.")]
    public short GotoSection { get; set; } = -1;

    [Category("Sections"), DisplayName("Start Section"), DefaultValue((short)-1)]
    [Description("Specifies if a new section is started (including this checkpoint). Use -1 otherwise.")]
    [XmlAttribute("startSection")]
    [ListViewColumn(3, "Start Sect.")]
    public short StartSection { get; set; } = -1;

    [Category("Checkpoint"), DisplayName("Key Point"), DefaultValue((short)-1)]
    [XmlAttribute("keyPointId")]
    [ListViewColumn(4, "Key ID")]
    public short KeyPointId { get; set; } = -1;

    [Category("Checkpoint"), DisplayName("Respawn"), XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsRespawnPoint>))]
    [ListViewColumn(5, "Respawn")]
    public Reference<MkdsRespawnPoint> Respawn { get; set; }

    [Category("Checkpoint"), DisplayName("Freeze Place"), XmlAttribute("freezePlace"), DefaultValue(false)]
    [Description("Disables place updates inside the checkpoint, used in rainbow road for the loop")]
    [ListViewColumn(6, "Freeze Place")]
    public bool FreezePlace { get; set; } = false;

    public MkdsCheckPoint() { }

    public MkdsCheckPoint(NkmdCpoi.CpoiEntry cpoiEntry)
    {
        Point1       = cpoiEntry.Point1;
        Point2       = cpoiEntry.Point2;
        GotoSection  = cpoiEntry.GotoSection;
        StartSection = cpoiEntry.StartSection;
        KeyPointId   = cpoiEntry.KeyPointId;
        FreezePlace  = (cpoiEntry.Flags & 1) != 0;

        if (cpoiEntry.RespawnId != 0xFF)
            Respawn = new UnresolvedBinaryMapDataReference<MkdsRespawnPoint>(cpoiEntry.RespawnId);
    }

    public NkmdCpoi.CpoiEntry ToCpoiEntry(IReferenceSerializerCollection serializerCollection) => new()
    {
        Point1       = Point1,
        Point2       = Point2,
        GotoSection  = GotoSection,
        StartSection = StartSection,
        KeyPointId   = KeyPointId,
        RespawnId    = (byte)serializerCollection.SerializeOrDefault(Respawn, 0xFF),
        Flags        = (byte)(FreezePlace ? 1 : 0)
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        if (Respawn?.IsResolved is false)
            Respawn = resolverCollection.Resolve(Respawn, _ => Respawn = null);
    }

    public void ReleaseReferences()
    {
        Respawn?.Release();
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsCheckPoint
        {
            Point1 = Point1,
            Point2 = Point2,
            GotoSection = GotoSection,
            StartSection = StartSection,
            KeyPointId = KeyPointId,
            FreezePlace = FreezePlace
        };

        if (Respawn?.IsResolved is true)
            entry.Respawn = new WeakMapDataReference<MkdsRespawnPoint>(Respawn.Target);
        else
            entry.Respawn = Respawn;

        return entry;
    }
}