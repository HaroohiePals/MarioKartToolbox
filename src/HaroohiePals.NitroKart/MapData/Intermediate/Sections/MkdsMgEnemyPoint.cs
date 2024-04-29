using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Linq;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsMgEnemyPoint : IMapDataEntry, IReferenceable<MkdsMgEnemyPoint>, IRoutePoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Enemy Point")]
    [ListViewColumn(1, "Radius")]
    public double Radius { get; set; } = 50f;

    [ListViewColumn(2, "?")]
    public ushort Unknown0 { get; set; }

    [ListViewColumn(3, "?")]
    public ushort Unknown1 { get; set; }

    [ListViewColumn(4, "?")]
    public uint Unknown2 { get; set; }

    [Browsable(false)]
    ReferenceHolder<MkdsMgEnemyPoint> IReferenceable<MkdsMgEnemyPoint>.ReferenceHolder { get; } = new();

    public string GetId(MkdsMapData mapData)
        => "" + mapData.MgEnemyPaths.SelectMany(x => x.Points).ToList().IndexOf(this);

    public MkdsMgEnemyPoint() { }

    public MkdsMgEnemyPoint(NkmdMepo.MepoEntry mepoEntry)
    {
        Position = mepoEntry.Position;
        Radius   = mepoEntry.Radius;
        Unknown0 = mepoEntry.Unknown0;
        Unknown1 = mepoEntry.Unknown1;
        Unknown2 = mepoEntry.Unknown2;
    }

    public NkmdMepo.MepoEntry ToMepoEntry() => new()
    {
        Position = Position,
        Radius   = Radius,
        Unknown0 = Unknown0,
        Unknown1 = Unknown1,
        Unknown2 = Unknown2
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() => this.ReleaseAllReferences();

    public IMapDataEntry Clone()
    {
        var entry = new MkdsMgEnemyPoint
        {
            Position = Position,
            Radius = Radius,
            Unknown0 = Unknown0,
            Unknown1 = Unknown1,
            Unknown2 = Unknown2
        };

        return entry;
    }
}