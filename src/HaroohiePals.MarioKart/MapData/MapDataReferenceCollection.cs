using HaroohiePals.IO.Reference;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HaroohiePals.MarioKart.MapData;

public class MapDataReferenceCollection<T> : List<Reference<T>>, IXmlSerializable
    where T : IReferenceable<T>, IMapDataEntry
{
    private const string NOT_REFERENCE_XML_TEXT_WRITER_EXCEPTION_MESSAGE
        = "References can only be serialized with a ReferenceXmlTextWriter";

    private const string REFERENCE_XML_ELEMENT_NAME = "Reference";

    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        reader.MoveToContent();
        bool isEmptyElement = reader.IsEmptyElement;
        reader.ReadStartElement();
        if (isEmptyElement)
            return;
        do
        {
            string idValue = reader.GetAttribute(MapDataXmlTextWriter.XmlIdAttribName);
            if (!int.TryParse(idValue, out int id))
                id = -1;
            Add(new UnresolvedXmlMapDataReference<T>(id));
            reader.Read();
        } while (reader.NodeType == XmlNodeType.Element);

        reader.ReadEndElement();
    }

    public void WriteXml(XmlWriter writer)
    {
        if (writer is not ReferenceXmlTextWriter refWriter)
            throw new ReferenceSerializationException(NOT_REFERENCE_XML_TEXT_WRITER_EXCEPTION_MESSAGE);

        foreach (var reference in this)
        {
            writer.WriteStartElement(REFERENCE_XML_ELEMENT_NAME);
            refWriter.WriteReference(reference);
            writer.WriteEndElement();
        }
    }
}