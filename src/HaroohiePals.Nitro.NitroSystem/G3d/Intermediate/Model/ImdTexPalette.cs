using HaroohiePals.Graphics;
using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdTexPalette
{
    public ImdTexPalette(XmlElement element)
    {
        Index     = IntermediateUtil.GetIntAttribute(element, "index");
        Name      = IntermediateUtil.GetStringAttribute(element, "name");
        ColorSize = IntermediateUtil.GetIntAttribute(element, "color_size");

        Colors = IntermediateUtil.GetInnerTextParts(element)
            .Select(v => (Rgb555)ushort.Parse(v, NumberStyles.HexNumber))
            .ToArray();
        if (Colors.Length != ColorSize)
            throw new Exception();
    }

    public int    Index;
    public string Name;
    public int    ColorSize;

    public Rgb555[] Colors;
}