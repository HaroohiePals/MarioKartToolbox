using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsCamera : IMapDataEntry, IReferenceable<MkdsCamera>, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    [Category("Viewpoints"), DisplayName("Target 1")]
    [ListViewColumn(2, "Target 1 X", "Target 1 Y", "Target 1 Z")]
    public Vector3d Target1 { get; set; }

    [Category("Viewpoints"), DisplayName("Target 2")]
    [ListViewColumn(3, "Target 2 X", "Target 2 Y", "Target 2 Z")]
    public Vector3d Target2 { get; set; }

    [Category("Field of View"), DisplayName("Begin Angle")]
    [ListViewColumn(4, "Begin Angle")]
    public ushort FovBegin { get; set; }

    [Category("Field of View"), DisplayName("End Angle")]
    [ListViewColumn(5, "End Angle")]
    public ushort FovEnd { get; set; }

    [Category("Field of View"), DisplayName("Speed")]
    [ListViewColumn(6, "FOV Speed")]
    public double FovSpeed { get; set; }

    [Category("Camera"), DisplayName("Type"), XmlIgnore]
    [ListViewColumn(7, "Type")]
    public MkdsCameraType Type { get; set; }

    [XmlAttribute("type"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public ushort TypeUshort
    {
        get => (ushort)Type;
        set => Type = (MkdsCameraType)value;
    }

    [Category("Object"), XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsPath>))]
    [ListViewColumn(8, "Path")]
    public Reference<MkdsPath> Path { get; set; }

    [Category("Camera"), DisplayName("Path Speed")]
    [ListViewColumn(9, "Path Speed")]
    public double PathSpeed { get; set; }

    [Category("Viewpoints"), DisplayName("Speed")]
    [ListViewColumn(10, "Target Speed")]
    public double TargetSpeed { get; set; }

    [Category("Camera")]
    [ListViewColumn(11, "Duration")]
    public short Duration { get; set; }

    [Category("Camera"), DisplayName("Next Camera"), XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsCamera>))]
    [ListViewColumn(12, "Next")]
    public Reference<MkdsCamera> NextCamera { get; set; }

    [Category("Camera"), DisplayName("First Intro Camera")]
    [Description(
        "Specifies if this CAME is the first camera to use for the course intro for the top or bottom screen.")]
    [ListViewColumn(13, "1st Intro Cam")]
    public MkdsCameIntroCamera FirstIntroCamera { get; set; }

    [Category("Camera"), DisplayName("Show Award Trail")]
    [ListViewColumn(14, "Award Trail")]
    public byte ShowAwardTrail { get; set; }

    [Browsable(false)]
    ReferenceHolder<MkdsCamera> IReferenceable<MkdsCamera>.ReferenceHolder { get; } = new();

    public string GetId(MkdsMapData mapData) => "" + mapData.Cameras.IndexOf(this);

    public MkdsCamera()
    {
        FovBegin = 30;
        FovEnd   = 30;
    }

    public MkdsCamera(NkmdCame.CameEntry cameEntry)
    {
        Position         = cameEntry.Position;
        Rotation         = cameEntry.Rotation;
        Target1          = cameEntry.Target1;
        Target2          = cameEntry.Target2;
        FovBegin         = cameEntry.FovBegin;
        FovEnd           = cameEntry.FovEnd;
        FovSpeed         = cameEntry.FovSpeed;
        Type             = cameEntry.Type;
        PathSpeed        = cameEntry.PathSpeed;
        TargetSpeed      = cameEntry.TargetSpeed;
        Duration         = cameEntry.Duration;
        FirstIntroCamera = cameEntry.FirstIntroCamera;
        ShowAwardTrail   = cameEntry.ShowAwardTrail;

        if (cameEntry.LinkedRoute >= 0)
            Path = new UnresolvedBinaryMapDataReference<MkdsPath>(cameEntry.LinkedRoute);
        if (cameEntry.NextCamera >= 0)
            NextCamera = new UnresolvedBinaryMapDataReference<MkdsCamera>(cameEntry.NextCamera);
    }

    public NkmdCame.CameEntry ToCameEntry(IReferenceSerializerCollection serializerCollection) => new()
    {
        Position         = Position,
        Rotation         = Rotation,
        Target1          = Target1,
        Target2          = Target2,
        FovBegin         = FovBegin,
        FovEnd           = FovEnd,
        FovSpeed         = FovSpeed,
        Type             = Type,
        PathSpeed        = PathSpeed,
        TargetSpeed      = TargetSpeed,
        Duration         = Duration,
        FirstIntroCamera = FirstIntroCamera,
        ShowAwardTrail   = ShowAwardTrail,
        LinkedRoute      = (short)serializerCollection.SerializeOrDefault(Path, -1),
        NextCamera       = (short)serializerCollection.SerializeOrDefault(NextCamera, -1)
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        if (Path?.IsResolved is false)
            Path = resolverCollection.Resolve(Path, _ => Path = null);

        if (NextCamera?.IsResolved is false)
            NextCamera = resolverCollection.Resolve(NextCamera, _ => NextCamera = null);
    }

    public void ReleaseReferences()
    {
        NextCamera?.Release();
        Path?.Release();

        this.ReleaseAllReferences();
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsCamera
        {
            Position = Position,
            Rotation = Rotation,
            Target1 = Target1,
            Target2 = Target2,
            FovBegin = FovBegin,
            FovEnd = FovEnd,
            FovSpeed = FovSpeed,
            Type = Type,
            PathSpeed = PathSpeed,
            TargetSpeed = TargetSpeed,
            Duration = Duration,
            FirstIntroCamera = FirstIntroCamera,
            ShowAwardTrail = ShowAwardTrail,
        };

        if (Path?.IsResolved is true)
            entry.Path = new WeakMapDataReference<MkdsPath>(Path.Target);
        else
            entry.Path = Path;

        if (NextCamera?.IsResolved is true)
            entry.NextCamera = new WeakMapDataReference<MkdsCamera>(NextCamera.Target);
        else
            entry.NextCamera = NextCamera;

        return entry;
    }
}