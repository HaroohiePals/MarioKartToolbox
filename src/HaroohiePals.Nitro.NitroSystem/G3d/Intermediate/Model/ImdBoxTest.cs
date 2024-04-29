using OpenTK.Mathematics;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdBoxTest
{
    public ImdBoxTest(XmlElement element)
    {
        PosScale = IntermediateUtil.GetIntAttribute(element, "pos_scale");
        Xyz      = IntermediateUtil.GetVec3Attribute(element, "xyz");
        Whd      = IntermediateUtil.GetVec3Attribute(element, "whd");
    }

    public int      PosScale;
    public Vector3d Xyz;
    public Vector3d Whd;
}