using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;


namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections;

[XmlType("Path")]
public sealed class MkdsPath : IReferenceable<MkdsPath>, IMapDataEntry
{
    [XmlElement("Point")]
    public MapDataCollection<MkdsPathPoint> Points { get; } = new();

    [XmlAttribute("loop"), DefaultValue(false)]
    public bool Loop { get; set; } = false;

    public MkdsPath() { }

    public MkdsPath(Binary.NkmdPath.PathEntry pathEntry)
    {
        Loop = pathEntry.Loop;
    }

    [Browsable(false)]
    ReferenceHolder<MkdsPath> IReferenceable<MkdsPath>.ReferenceHolder { get; } = new();

    public void ResolveReferences(IReferenceResolverCollection resolverCollection) { }

    public void ReleaseReferences() => this.ReleaseAllReferences();

    public Binary.NkmdPath.PathEntry ToPathEntry()
    {
        return new Binary.NkmdPath.PathEntry { Loop = Loop, NrPoit = (short)Points.Count };
    }

    public IMapDataEntry Clone()
    {
        var entry = new MkdsPath
        {
            Loop = Loop
        };

        entry.Points.AddRange(Points.Select(x => (MkdsPathPoint)x.Clone()));

        return entry;
    }
}