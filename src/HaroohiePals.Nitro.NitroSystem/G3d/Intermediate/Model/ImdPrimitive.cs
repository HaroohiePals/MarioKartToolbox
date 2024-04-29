using HaroohiePals.Nitro.G3;
using System;
using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdPrimitive
{
    public ImdPrimitive(XmlElement element)
    {
        Index = IntermediateUtil.GetIntAttribute(element, "index");
        Type = element.GetAttribute("type") switch
        {
            "triangles"      => GxBegin.Triangles,
            "quads"          => GxBegin.Quads,
            "triangle_strip" => GxBegin.TriangleStrip,
            "quad_strip"     => GxBegin.QuadStrip,
            _                => throw new Exception()
        };
        VertexSize = IntermediateUtil.GetIntAttribute(element, "vertex_size");

        var xmlPrimitiveElements = element.ChildNodes.OfType<XmlElement>().ToArray();
        Elements = new ImdPrimitiveElement[xmlPrimitiveElements.Length];
        for (int i = 0; i < xmlPrimitiveElements.Length; i++)
            Elements[i] = ImdPrimitiveElement.Parse(xmlPrimitiveElements[i]);
    }

    public int     Index;
    public GxBegin Type;
    public int     VertexSize;

    public ImdPrimitiveElement[] Elements;
}