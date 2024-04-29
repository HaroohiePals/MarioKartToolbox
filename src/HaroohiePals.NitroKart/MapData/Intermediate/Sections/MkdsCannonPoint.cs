using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsCannonPoint : IMapDataEntry, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    [Category("Warp"), DisplayName("Next Battle Enemy Point")]
    [XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsMgEnemyPoint>))]
    [ListViewColumn(2, "Mepo")]
    public Reference<MkdsMgEnemyPoint> MgEnemyPoint { get; set; }

    [Category("Warp"), Description("Index used for KCL flags")]
    [ListViewColumn(3, "Idx")]
    public short Index { get; set; }

    public MkdsCannonPoint()
    {
    }

    public MkdsCannonPoint(NkmdKtpc.KtpcEntry ktpcEntry)
    {
        Position = ktpcEntry.Position;
        Rotation = ktpcEntry.Rotation;
        Index    = ktpcEntry.Index;

        if (ktpcEntry.NextMEPO >= 0)
            MgEnemyPoint = new UnresolvedBinaryMapDataReference<MkdsMgEnemyPoint>(ktpcEntry.NextMEPO);
    }

    public NkmdKtpc.KtpcEntry ToKtpcEntry(IReferenceSerializerCollection serializerCollection)
    {
        var entry = new NkmdKtpc.KtpcEntry()
        {
            Position = Position,
            Rotation = Rotation,
            Index    = Index
        };

        //if (mapData.Version > 34)
        //	entry.Index = (short)mapData.CannonPoints.Entries.IndexOf(this);
        //else
        //	entry.Index = -1;

        entry.NextMEPO = (short)serializerCollection.SerializeOrDefault(MgEnemyPoint, -1);

        return entry;
    }

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        if (MgEnemyPoint?.IsResolved is false)
            MgEnemyPoint = resolverCollection.Resolve(MgEnemyPoint, _ => MgEnemyPoint = null);
    }

    public void ReleaseReferences()
    {
        MgEnemyPoint?.Release();
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsCannonPoint
        {
            Position = Position,
            Rotation = Rotation,
            Index = Index
        };

        if (MgEnemyPoint?.IsResolved is true)
            entry.MgEnemyPoint = new WeakMapDataReference<MkdsMgEnemyPoint>(MgEnemyPoint.Target);
        else
            entry.MgEnemyPoint = MgEnemyPoint;

        return entry;
    }
}