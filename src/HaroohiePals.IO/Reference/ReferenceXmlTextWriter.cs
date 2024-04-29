using System.IO;
using System.Text;
using System.Xml;

namespace HaroohiePals.IO.Reference;

public abstract class ReferenceXmlTextWriter : XmlTextWriter
{
    protected readonly IReferenceSerializerCollection SerializerCollection;

    protected ReferenceXmlTextWriter(Stream w, Encoding encoding, IReferenceSerializerCollection serializerCollection)
        : base(w, encoding)
    {
        SerializerCollection = serializerCollection;
    }

    protected ReferenceXmlTextWriter(string filename, Encoding encoding,
        IReferenceSerializerCollection serializerCollection)
        : base(filename, encoding)
    {
        SerializerCollection = serializerCollection;
    }

    protected ReferenceXmlTextWriter(TextWriter w, IReferenceSerializerCollection serializerCollection)
        : base(w)
    {
        SerializerCollection = serializerCollection;
    }

    public abstract void WriteReference<T>(Reference<T> reference) where T : IReferenceable<T>;
}