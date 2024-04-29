using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsEnemyPoint : IMapDataEntry, IReferenceable<MkdsEnemyPoint>, IRoutePoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Enemy Point"), XmlAttribute("radius")]
    [ListViewColumn(1, "Radius")]
    public double Radius { get; set; } = 50f;

    [Category("Enemy Point")]
    [ListViewColumn(2, "Drift")]
    public NkmdEpoi.EpoiEntry.DriftValue Drifting { get; set; }

    [ListViewColumn(3, "?")]
    public ushort Unknown0 { get; set; }

    // [TypeConverter(typeof(HexTypeConverter)), HexReversed]
    [ListViewColumn(4, "?")]
    public uint Unknown1 { get; set; }

    [Browsable(false)]
    ReferenceHolder<MkdsEnemyPoint> IReferenceable<MkdsEnemyPoint>.ReferenceHolder { get; } = new();

    public MkdsEnemyPoint() { }

    public MkdsEnemyPoint(NkmdEpoi.EpoiEntry epoiEntry)
    {
        Position = epoiEntry.Position;
        Radius   = epoiEntry.Radius;
        Drifting = epoiEntry.Drifting;
        Unknown0 = epoiEntry.Unknown0;
        Unknown1 = epoiEntry.Unknown1;
    }

    public NkmdEpoi.EpoiEntry ToEpoiEntry() => new()
    {
        Position = Position,
        Radius   = Radius,
        Drifting = Drifting,
        Unknown0 = Unknown0,
        Unknown1 = Unknown1
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() => this.ReleaseAllReferences();


    public IMapDataEntry Clone()
    {
        var entry = new MkdsEnemyPoint
        {
            Position = Position,
            Radius = Radius,
            Drifting = Drifting,
            Unknown0 = Unknown0,
            Unknown1 = Unknown1
        };

        return entry;
    }
}