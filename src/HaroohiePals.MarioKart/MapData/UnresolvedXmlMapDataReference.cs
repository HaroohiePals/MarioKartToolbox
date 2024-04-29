using HaroohiePals.IO.Reference;
using System.Xml;

namespace HaroohiePals.MarioKart.MapData;

public class UnresolvedXmlMapDataReference<T> : Reference<T>
    where T : IReferenceable<T>, IMapDataEntry
{
    public override bool IsResolved => false;

    public int UnresolvedId { get; private set; }

    public UnresolvedXmlMapDataReference() { }

    public UnresolvedXmlMapDataReference(int unresolvedId)
    {
        UnresolvedId = unresolvedId;
    }

    public override void ReadXml(XmlReader reader)
    {
        reader.MoveToContent();
        string value = reader.GetAttribute(MapDataXmlTextWriter.XmlIdAttribName);
        if (string.IsNullOrEmpty(value))
            UnresolvedId = -1;
        else
            UnresolvedId = int.Parse(value);
        bool isEmptyElement = reader.IsEmptyElement;
        reader.ReadStartElement();
        if (!isEmptyElement)
            reader.ReadEndElement();
    }

    public override void WriteXml(XmlWriter writer)
    {
        throw new NotSupportedException();
    }
}