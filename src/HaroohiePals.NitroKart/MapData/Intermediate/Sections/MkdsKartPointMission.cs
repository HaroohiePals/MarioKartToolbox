using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("MissionPoint")]
public sealed class MkdsKartPointMission : IMapDataEntry, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    [Category("Mission Point")]
    // [TypeConverter(typeof(HexTypeConverter)), HexReversed]
    [ListViewColumn(2, "?")]
    public ushort Unknown { get; set; }

    [Category("Mission Point")]
    [ListViewColumn(3, "Idx")]
    public short Index { get; set; }

    public MkdsKartPointMission() { }

    public MkdsKartPointMission(NkmdKtpm.KtpmEntry ktpmEntry)
    {
        Position = ktpmEntry.Position;
        Rotation = ktpmEntry.Rotation;
        Unknown  = ktpmEntry.Unknown;
        Index    = ktpmEntry.Index;
    }

    public NkmdKtpm.KtpmEntry ToKtpmEntry() => new()
    {
        Position = Position,
        Rotation = Rotation,
        Unknown  = Unknown,
        Index    = Index
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() { }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsKartPointMission
        {
            Position = Position,
            Rotation = Rotation,
            Unknown = Unknown,
            Index = Index
        };

        return entry;
    }
}