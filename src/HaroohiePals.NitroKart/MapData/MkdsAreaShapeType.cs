using System.Xml.Serialization;

namespace HaroohiePals.NitroKart.MapData;

public enum MkdsAreaShapeType : byte
{
    [XmlEnum("box")]
    Box = 0,

    [XmlEnum("cylinder")]
    Cylinder = 1
}