using HaroohiePals.IO.Reference;
using HaroohiePals.NitroKart.MapData.Binary;
using System.ComponentModel;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class MkdsMobjSettings : IXmlSerializable
{
    public MkdsMobjSettings() { }

    public MkdsMobjSettings(MkdsMobjSettings settings)
    {
        settings?.Settings.CopyTo(Settings, 0);
    }

    public short[] Settings { get; } = new short[7];

    public virtual void ToNkmEntry(IReferenceSerializerCollection serializerCollection, NkmdObji.ObjiEntry objiEntry)
    {
        Settings.CopyTo(objiEntry.Settings, 0);
    }

    public virtual void ResolveReferences(IReferenceResolverCollection resolverCollection) { }
    public virtual void ReleaseReferences() { }

    public XmlSchema GetSchema() => null;

    public virtual void ReadXml(XmlReader reader)
    {
        reader.MoveToContent();
        bool isEmptyElement = reader.IsEmptyElement;
        reader.ReadStartElement();
        if (!isEmptyElement)
        {
            int i = 0;
            do
            {
                if (i < 7)
                    Settings[i++] = short.Parse(reader.GetAttribute("value"));
                reader.Read();
            } while (reader.NodeType == XmlNodeType.Element);

            reader.ReadEndElement();
        }
    }

    public virtual void WriteXml(XmlWriter writer)
    {
        foreach (var setting in Settings)
        {
            writer.WriteStartElement("Setting");
            writer.WriteAttributeString("value", "" + setting);
            writer.WriteEndElement();
        }
    }
}