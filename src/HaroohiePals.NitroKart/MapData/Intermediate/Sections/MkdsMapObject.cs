using HaroohiePals.Core.ComponentModel;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsMapObject : IMapDataEntry, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    [Category("Transformation")]
    [ListViewColumn(2, "Scale X", "Scale Y", "Scale Z")]
    public Vector3d Scale { get; set; } = new(1);

    private MkdsMapObjectId _objectId;

    [Category("Object"), DisplayName("Map Object ID"), XmlIgnore]
    [ListViewColumn(3, "Object ID")]
    public MkdsMapObjectId ObjectId
    {
        get => _objectId;
        set
        {
            _objectId = value;
            Settings  = MkdsMobjSettingsFactory.Construct(_objectId, Settings);
        }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlAttribute("objectId")]
    public ushort ObjectIdUshort
    {
        get => (ushort)ObjectId;
        set => ObjectId = (MkdsMapObjectId)value;
    }

    [Category("Object"), XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsPath>))]
    [ListViewColumn(4, "Path")]
    public Reference<MkdsPath> Path { get; set; }


    private MkdsMobjSettings _settings = new MkdsMobjSettings();

    [Nested(NestType.Category),
     ListViewColumn(5, "Setting 0", "Setting 1", "Setting 2", "Setting 3", "Setting 4", "Setting 5", "Setting 6")]
    public MkdsMobjSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            if (_settings is MkdsMobjSettings)
                _settings = MkdsMobjSettingsFactory.Construct(_objectId, _settings);
        }
    }

    [Category("Object"), DisplayName("Enable Inactive Period")]
    [ListViewColumn(6, "Inactive Period ON")]
    public bool EnableInactivePeriod { get; set; }

    [Category("Object"), DisplayName("Clip Area ID"), Range(0, 9)]
    [ListViewColumn(7, "Clip Area 0", "Clip Area 1", "Clip Area 2", "Clip Area 3")]
    public ushort[] ClipAreaIds { get; set; } = new ushort[4];

    [Category("Object"), DisplayName("TT Visible")]
    [Description("Specifies whether the object is visible in Time Trials mode or not.")]
    [XmlAttribute("ttVisible")]
    [ListViewColumn(8, "TT Visible")]
    public bool TTVisible { get; set; }

    public MkdsMapObject()
    {
        _objectId = MkdsMapObjectId.Itembox;
    }

    public MkdsMapObject(NkmdObji.ObjiEntry objiEntry)
    {
        Position             = objiEntry.Position;
        Rotation             = objiEntry.Rotation;
        Scale                = objiEntry.Scale;
        EnableInactivePeriod = objiEntry.EnableInactivePeriod;
        ClipAreaIds          = objiEntry.ClipAreaIds;
        TTVisible            = objiEntry.TTVisible;

        if (objiEntry.PathId >= 0)
            Path = new UnresolvedBinaryMapDataReference<MkdsPath>(objiEntry.PathId);

        objiEntry.ClipAreaIds.CopyTo(ClipAreaIds, 0);

        objiEntry.Settings.CopyTo(Settings.Settings, 0);
        ObjectId = (MkdsMapObjectId)objiEntry.ObjectId;
    }

    public NkmdObji.ObjiEntry ToObjiEntry(IReferenceSerializerCollection serializerCollection)
    {
        var objiEntry = new NkmdObji.ObjiEntry
        {
            Position             = Position,
            Rotation             = Rotation,
            Scale                = Scale,
            EnableInactivePeriod = EnableInactivePeriod,
            ClipAreaIds          = ClipAreaIds,
            TTVisible            = TTVisible,
            PathId               = (short)serializerCollection.SerializeOrDefault(Path, -1),
            ObjectId             = (ushort)ObjectId
        };

        ClipAreaIds.CopyTo(objiEntry.ClipAreaIds, 0);
        Settings.ToNkmEntry(serializerCollection, objiEntry);

        return objiEntry;
    }

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        if (Path?.IsResolved is false)
            Path = resolverCollection.Resolve(Path, _ => Path = null);

        Settings.ResolveReferences(resolverCollection);
    }

    public void ReleaseReferences()
    {
        Path?.Release();
        Settings?.ReleaseReferences();
    }

    public override string ToString() => ObjectId.ToString();

    public IMapDataEntry Clone()
    {
        var entry = new MkdsMapObject
        {
            Position = Position,
            Rotation = Rotation,
            Scale = Scale,
            EnableInactivePeriod = EnableInactivePeriod,
            ClipAreaIds = (ushort[])ClipAreaIds.Clone(),
            TTVisible = TTVisible,
            ObjectId = ObjectId,
            Settings = new MkdsMobjSettings(Settings)
        };

        if (Path?.IsResolved is true)
            entry.Path = new WeakMapDataReference<MkdsPath>(Path.Target);
        else
            entry.Path = Path;

        return entry;
    }
}