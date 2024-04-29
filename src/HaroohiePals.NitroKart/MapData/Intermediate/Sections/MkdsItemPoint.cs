using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsItemPoint : IMapDataEntry, IReferenceable<MkdsItemPoint>, IRoutePoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Item Point"), XmlAttribute("radius")]
    [ListViewColumn(1, "Radius")]
    public double Radius { get; set; } = 50f;

    [Category("Item Point"), XmlAttribute("recalculationIndex"), DefaultValue((byte)0)]
    [Description("Only valid if Radius >= 1000")]
    [ListViewColumn(2, "Recalc. Idx")]
    public byte RecalculationIndex { get; set; } = 0;

    [Browsable(false)]
    ReferenceHolder<MkdsItemPoint> IReferenceable<MkdsItemPoint>.ReferenceHolder { get; } = new();

    public MkdsItemPoint() { }

    public MkdsItemPoint(NkmdIpoi.IpoiEntry ipoiEntry)
    {
        Position           = ipoiEntry.Position;
        Radius             = ipoiEntry.Radius;
        RecalculationIndex = ipoiEntry.RecalculationIndex;
    }

    public NkmdIpoi.IpoiEntry ToIpoiEntry() => new()
    {
        Position           = Position,
        Radius             = Radius,
        RecalculationIndex = RecalculationIndex
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() => this.ReleaseAllReferences();

    public IMapDataEntry Clone()
    {
        var entry = new MkdsItemPoint
        {
            Position = Position,
            Radius = Radius,
            RecalculationIndex = RecalculationIndex
        };

        return entry;
    }
}