using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsStartPoint : IMapDataEntry, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    [ListViewColumn(2, "Idx")]
    public short Index { get; set; }

    public MkdsStartPoint() { }

    public MkdsStartPoint(NkmdKtps.KtpsEntry ktpsEntry)
    {
        Position = ktpsEntry.Position;
        Rotation = ktpsEntry.Rotation;
        Index    = ktpsEntry.Index;
    }

    public NkmdKtps.KtpsEntry ToKtpsEntry() => new()
    {
        Position = Position,
        Rotation = Rotation,
        Index    = Index
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() { }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsStartPoint
        {
            Position = Position,
            Rotation = Rotation,
            Index = Index
        };

        return entry;
    }
}