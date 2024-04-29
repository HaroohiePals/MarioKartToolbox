using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdOutputInfo
{
    public ImdOutputInfo(XmlElement element)
    {
        VertexSize   = IntermediateUtil.GetIntAttribute(element, "vertex_size");
        PolygonSize  = IntermediateUtil.GetIntAttribute(element, "polygon_size");
        TriangleSize = IntermediateUtil.GetIntAttribute(element, "triangle_size");
        QuadSize     = IntermediateUtil.GetIntAttribute(element, "quad_size");
    }

    public int VertexSize;
    public int PolygonSize;
    public int TriangleSize;
    public int QuadSize;
}