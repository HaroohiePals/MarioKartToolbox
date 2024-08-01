using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsArea : IMapDataEntry, IPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Vectors"), DisplayName("Length Vector")]
    [ListViewColumn(1, "Len X", "Len Y", "Len Z")]
    public Vector3d LengthVector { get; set; }

    [Category("Vectors"), DisplayName("X Vector")]
    [ListViewColumn(2, "X Vec X", "X Vec Y", "X Vec Z")]
    public Vector3d XVector { get; set; }

    [Category("Vectors"), DisplayName("Y Vector")]
    [ListViewColumn(3, "Y Vec X", "Y Vec Y", "Y Vec Z")]
    public Vector3d YVector { get; set; }

    [Category("Vectors"), DisplayName("Z Vector")]
    [ListViewColumn(4, "Z Vec X", "Z Vec Y", "Z Vec Z")]
    public Vector3d ZVector { get; set; }

    [Description("ClipArea => Clip Area ID (Must be the same used in the OBJI entry)\n" +
                 "MissionEnd => 0 = win / 1 = lose\n" +
                 "MissionRivalPass => OBJI ID")]
    [ListViewColumn(5, "Param 0")]
    public short Param0 { get; set; }

    [Description("MissionRivalPass => Number of passes before losing")]
    [ListViewColumn(6, "Param 1")]
    public short Param1 { get; set; }

    [DisplayName("Enemy Point"), XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsEnemyPoint>))]
    [ListViewColumn(7, "Epoi")]
    public Reference<MkdsEnemyPoint> EnemyPoint { get; set; }

    [DisplayName("Battle Enemy Point")]
    [XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsMgEnemyPoint>))]
    [ListViewColumn(8, "Mepo")]
    public Reference<MkdsMgEnemyPoint> MgEnemyPoint { get; set; }

    [XmlAttribute("shape")]
    [ListViewColumn(9, "Shape")]
    public MkdsAreaShapeType Shape { get; set; }

    [XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsCamera>))]
    [ListViewColumn(10, "Camera")]
    public Reference<MkdsCamera> Camera { get; set; }

    [XmlIgnore]
    [ListViewColumn(11, "Type")]
    public MkdsAreaType AreaType { get; set; }

    [XmlAttribute("type"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public byte AreaTypeByte
    {
        get => (byte)AreaType;
        set => AreaType = (MkdsAreaType)value;
    }

    [ListViewColumn(12, "?")]
    public ushort Unknown10 { get; set; } //unverified

    [ListViewColumn(13, "?")]
    public byte Unknown11 { get; set; }

    public MkdsArea()
    {
        LengthVector = new(10, 10, 10);
        XVector = Vector3d.UnitX;
        YVector = Vector3d.UnitY;
        ZVector = Vector3d.UnitZ;
    }

    public MkdsArea(Binary.NkmdArea.AreaEntry areaEntry, bool isMgStage)
    {
        Position = areaEntry.Position;
        LengthVector = areaEntry.LengthVector;
        XVector = areaEntry.XVector;
        YVector = areaEntry.YVector;
        ZVector = areaEntry.ZVector;
        Param0 = areaEntry.Param0;
        Param1 = areaEntry.Param1;
        Shape = areaEntry.Shape;
        AreaType = areaEntry.AreaType;
        Unknown10 = areaEntry.Unknown10;
        Unknown11 = areaEntry.Unknown11;

        if (areaEntry.LinkedCame >= 0)
            Camera = new UnresolvedBinaryMapDataReference<MkdsCamera>(areaEntry.LinkedCame);

        if (areaEntry.AreaType == MkdsAreaType.EnemyRecalculation && areaEntry.EnemyPointId >= 0)
        {
            if (isMgStage)
                MgEnemyPoint = new UnresolvedBinaryMapDataReference<MkdsMgEnemyPoint>(areaEntry.EnemyPointId);
            else
                EnemyPoint = new UnresolvedBinaryMapDataReference<MkdsEnemyPoint>(areaEntry.EnemyPointId);
        }
    }

    public Binary.NkmdArea.AreaEntry ToAreaEntry(IReferenceSerializerCollection serializerCollection)
    {
        var entry = new Binary.NkmdArea.AreaEntry
        {
            Position = Position,
            LengthVector = LengthVector,
            XVector = XVector,
            YVector = YVector,
            ZVector = ZVector,
            Param0 = Param0,
            Param1 = Param1,
            Shape = Shape,
            AreaType = AreaType,
            Unknown10 = Unknown10,
            Unknown11 = Unknown11,
            LinkedCame = (sbyte)serializerCollection.SerializeOrDefault(Camera, -1)
        };
        if (AreaType == MkdsAreaType.EnemyRecalculation)
        {
            if (EnemyPoint == null)
                entry.EnemyPointId = (short)serializerCollection.SerializeOrDefault(MgEnemyPoint, -1);
            else
                entry.EnemyPointId = (short)serializerCollection.SerializeOrDefault(EnemyPoint, -1);
        }
        else
            entry.EnemyPointId = 0;

        return entry;
    }

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        if (Camera?.IsResolved is false)
            Camera = resolverCollection.Resolve(Camera, _ => Camera = null);

        if (AreaType == MkdsAreaType.EnemyRecalculation)
        {
            if (EnemyPoint?.IsResolved is false)
                EnemyPoint = resolverCollection.Resolve(EnemyPoint, _ => EnemyPoint = null);

            if (MgEnemyPoint?.IsResolved is false)
                MgEnemyPoint = resolverCollection.Resolve(MgEnemyPoint, _ => MgEnemyPoint = null);
        }
    }

    public void ReleaseReferences()
    {
        Camera?.Release();
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsArea
        {
            Position = Position,
            LengthVector = LengthVector,
            XVector = XVector,
            YVector = YVector,
            ZVector = ZVector,
            Param0 = Param0,
            Param1 = Param1,
            Shape = Shape,
            AreaType = AreaType,
            Unknown10 = Unknown10,
            Unknown11 = Unknown11,
        };

        if (Camera?.IsResolved is true)
            entry.Camera = new WeakMapDataReference<MkdsCamera>(Camera.Target);
        else
            entry.Camera = Camera;

        if (EnemyPoint?.IsResolved is true)
            entry.EnemyPoint = new WeakMapDataReference<MkdsEnemyPoint>(EnemyPoint.Target);
        else
            entry.EnemyPoint = EnemyPoint;

        if (MgEnemyPoint?.IsResolved is true)
            entry.MgEnemyPoint = new WeakMapDataReference<MkdsMgEnemyPoint>(MgEnemyPoint.Target);
        else
            entry.MgEnemyPoint = MgEnemyPoint;

        return entry;
    }
}