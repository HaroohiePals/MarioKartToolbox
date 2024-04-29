using HaroohiePals.IO.Reference;
using System.Text;

namespace HaroohiePals.MarioKart.MapData;

public class MapDataXmlTextWriter : ReferenceXmlTextWriter
{
    private const string CANNOT_SERIALIZE_EXCEPTION_MESSAGE = "Cannot serializer from Reference<{0}> to string or int";

    public static readonly string XmlIdAttribName = "id";

    public MapDataXmlTextWriter(Stream w, Encoding encoding, IReferenceSerializerCollection serializerCollection)
        : base(w, encoding, serializerCollection) { }

    public MapDataXmlTextWriter(string filename, Encoding encoding, IReferenceSerializerCollection serializerCollection)
        : base(filename, encoding, serializerCollection) { }

    public MapDataXmlTextWriter(TextWriter w, IReferenceSerializerCollection serializerCollection)
        : base(w, serializerCollection) { }

    public override void WriteReference<T>(Reference<T> reference)
    {
        if (SerializerCollection.CanSerialize<T, string>())
            WriteAttributeString(XmlIdAttribName, SerializerCollection.Serialize<T, string>(reference));
        else if (SerializerCollection.CanSerialize<T, int>())
            WriteAttributeString(XmlIdAttribName, SerializerCollection.Serialize<T, int>(reference).ToString());
        else
            throw new ReferenceSerializationException(string.Format(CANNOT_SERIALIZE_EXCEPTION_MESSAGE, typeof(T)));
    }
}