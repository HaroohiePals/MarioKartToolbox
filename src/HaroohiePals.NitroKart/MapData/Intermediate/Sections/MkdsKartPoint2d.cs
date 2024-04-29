using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsKartPoint2d : IMapDataEntry, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    public MkdsKartPoint2d() { }

    public MkdsKartPoint2d(NkmdKtp2.Ktp2Entry ktp2Entry)
    {
        Position = ktp2Entry.Position;
        Rotation = ktp2Entry.Rotation;
    }

    public NkmdKtp2.Ktp2Entry ToKtp2Entry() => new()
    {
        Position = Position,
        Rotation = Rotation
    };

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() { }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsKartPoint2d
        {
            Position = Position,
            Rotation = Rotation
        };

        return entry;
    }
}