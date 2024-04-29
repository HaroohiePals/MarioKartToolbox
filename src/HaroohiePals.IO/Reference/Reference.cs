using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace HaroohiePals.IO.Reference;

public class Reference<T> : IXmlSerializable where T : IReferenceable<T>
{
    private const string NOT_REFERENCE_XML_TEXT_WRITER_EXCEPTION_MESSAGE
        = "References can only be serialized with a ReferenceXmlTextWriter";

    public delegate void RemoveReferenceFunc(Reference<T> reference);

    private readonly RemoveReferenceFunc _removeFunc;

    public virtual bool IsResolved => true;

    public T Target { get; }

    protected Reference() { }

    public Reference(T target, RemoveReferenceFunc removeFunc)
    {
        Target      = target;
        _removeFunc = removeFunc;
    }

    public void InvokeRemove() => _removeFunc?.Invoke(this);

    public void Release() => Target?.ReleaseReference(this);

    public override string ToString() => Target?.ToString();

    public virtual XmlSchema GetSchema() => null;

    public virtual void ReadXml(XmlReader reader)
    {
        throw new NotSupportedException();
    }

    public virtual void WriteXml(XmlWriter writer)
    {
        if (!(writer is ReferenceXmlTextWriter refWriter))
            throw new ReferenceSerializationException(NOT_REFERENCE_XML_TEXT_WRITER_EXCEPTION_MESSAGE);
        refWriter.WriteReference(this);
    }
}