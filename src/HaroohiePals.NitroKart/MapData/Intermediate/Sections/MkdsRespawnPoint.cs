using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Binary;
using HaroohiePals.NitroKart.MapData.Intermediate.ComponentModel;
using OpenTK.Mathematics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

public sealed class MkdsRespawnPoint : IMapDataEntry, IReferenceable<MkdsRespawnPoint>, IRotatedPoint
{
    [Category("Transformation")]
    [ListViewColumn(0, "X", "Y", "Z")]
    public Vector3d Position { get; set; }

    [Category("Transformation")]
    [ListViewColumn(1, "Rot X", "Rot Y", "Rot Z")]
    public Vector3d Rotation { get; set; }

    [Category("Respawn"), DisplayName("Enemy Point"),
     XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsEnemyPoint>))]
    [ListViewColumn(2, "Epoi")]
    public Reference<MkdsEnemyPoint> EnemyPoint { get; set; }

    [Category("Respawn"), DisplayName("Battle Enemy Point")]
    [XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsMgEnemyPoint>))]
    [ListViewColumn(3, "Mepo")]
    public Reference<MkdsMgEnemyPoint> MgEnemyPoint { get; set; }


    [Category("Respawn"), DisplayName("Item Point"), XmlElement(typeof(UnresolvedXmlMapDataReference<MkdsItemPoint>))]
    [ListViewColumn(4, "Ipoi")]
    public Reference<MkdsItemPoint> ItemPoint { get; set; }

    [Browsable(false)]
    ReferenceHolder<MkdsRespawnPoint> IReferenceable<MkdsRespawnPoint>.ReferenceHolder { get; } = new();

    [Browsable(false)]
    [XmlIgnore]
    public int OriginalIndex { get; } = -1;

    public MkdsRespawnPoint() { }

    public MkdsRespawnPoint(NkmdKtpj.KtpjEntry ktpjEntry, bool isMgStage)
    {
        Position = ktpjEntry.Position;
        Rotation = ktpjEntry.Rotation;

        OriginalIndex = ktpjEntry.Index;

        if (ktpjEntry.EnemyPointID >= 0)
        {
            if (isMgStage)
                MgEnemyPoint = new UnresolvedBinaryMapDataReference<MkdsMgEnemyPoint>(ktpjEntry.EnemyPointID);
            else
                EnemyPoint = new UnresolvedBinaryMapDataReference<MkdsEnemyPoint>(ktpjEntry.EnemyPointID);
        }

        if (ktpjEntry.ItemPointID >= 0)
            ItemPoint = new UnresolvedBinaryMapDataReference<MkdsItemPoint>(ktpjEntry.ItemPointID);
    }

    public NkmdKtpj.KtpjEntry ToKtpjEntry(IReferenceSerializerCollection serializerCollection)
    {
        var entry = new NkmdKtpj.KtpjEntry
        {
            Position = Position,
            Rotation = Rotation,
            Index    = serializerCollection.Serialize<MkdsRespawnPoint, int>(new Reference<MkdsRespawnPoint>(this, null))
        };

        if (MgEnemyPoint is not null)
            entry.EnemyPointID = (short)serializerCollection.SerializeOrDefault(MgEnemyPoint, -1);
        else
            entry.EnemyPointID = (short)serializerCollection.SerializeOrDefault(EnemyPoint, -1);

        entry.ItemPointID = (short)serializerCollection.SerializeOrDefault(ItemPoint, -1);

        return entry;
    }

    public void ResolveReferences(IReferenceResolverCollection resolverCollection)
    {
        if (EnemyPoint?.IsResolved is false)
            EnemyPoint = resolverCollection.Resolve(EnemyPoint, _ => EnemyPoint = null);

        if (MgEnemyPoint?.IsResolved is false)
            MgEnemyPoint = resolverCollection.Resolve(MgEnemyPoint, _ => MgEnemyPoint = null);

        if (ItemPoint?.IsResolved is false)
            ItemPoint = resolverCollection.Resolve(ItemPoint, _ => ItemPoint = null);
    }

    public void ReleaseReferences()
    {
        EnemyPoint?.Release();
        MgEnemyPoint?.Release();
        ItemPoint?.Release();

        this.ReleaseAllReferences();
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsRespawnPoint
        {
            Position = Position,
            Rotation = Rotation
        };

        if (ItemPoint?.IsResolved is true)
            entry.ItemPoint = new WeakMapDataReference<MkdsItemPoint>(ItemPoint.Target);
        else
            entry.ItemPoint = ItemPoint;

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