using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("Point")]
public sealed class MkdsPathPoint : IMapDataEntry, IPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    //[Category("Point")]
    //public byte Index { get; set; }
    [Category("Point")]
    [ListViewColumn(1, "?")]
    public byte Unknown1 { get; set; }

    [Category("Point")]
    [ListViewColumn(2, "Duration")]
    public short Duration { get; set; }

    [Category("Point")]
    // [TypeConverter(typeof(HexTypeConverter)), HexReversed]
    [ListViewColumn(3, "?")]
    public uint Unknown2 { get; set; }

    public MkdsPathPoint() { }

    public MkdsPathPoint(NkmdPoit.PoitEntry poitEntry)
    {
        Position = poitEntry.Position;
        Unknown1 = poitEntry.Unknown1;
        Duration = poitEntry.Duration;
        Unknown2 = poitEntry.Unknown2;
    }

    public NkmdPoit.PoitEntry ToPoitEntry() => new()
    {
        Position = Position,
        Unknown1 = Unknown1,
        Duration = Duration,
        Unknown2 = Unknown2
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() { }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsPathPoint
        {
            Position = Position,
            Unknown1 = Unknown1,
            Duration = Duration,
            Unknown2 = Unknown2
        };

        return entry;
    }
}