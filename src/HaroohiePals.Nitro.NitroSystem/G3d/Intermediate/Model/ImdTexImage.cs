using HaroohiePals.IO;
using HaroohiePals.Nitro.Gx;
using System;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdTexImage
{
    public enum TexColor0Mode
    {
        Color,
        Transparency
    }

    public ImdTexImage(XmlElement element)
    {
        Index          = IntermediateUtil.GetIntAttribute(element, "index");
        Name           = IntermediateUtil.GetStringAttribute(element, "name");
        Width          = IntermediateUtil.GetIntAttribute(element, "width");
        Height         = IntermediateUtil.GetIntAttribute(element, "height");
        OriginalWidth  = IntermediateUtil.GetIntAttribute(element, "original_width");
        OriginalHeight = IntermediateUtil.GetIntAttribute(element, "original_height");
        Format = element.GetAttribute("format") switch
        {
            "palette4"   => ImageFormat.Pltt4,
            "palette16"  => ImageFormat.Pltt16,
            "palette256" => ImageFormat.Pltt256,
            "tex4x4"     => ImageFormat.Comp4x4,
            "a3i5"       => ImageFormat.A3I5,
            "a5i3"       => ImageFormat.A5I3,
            "direct"     => ImageFormat.Direct,
            _            => throw new Exception()
        };
        if (Format == ImageFormat.Pltt4 || Format == ImageFormat.Pltt16 || Format == ImageFormat.Pltt256)
        {
            Color0Mode = element.GetAttribute("color0_mode") switch
            {
                "color"        => TexColor0Mode.Color,
                "transparency" => TexColor0Mode.Transparency,
                _              => throw new Exception()
            };
        }

        PaletteName = IntermediateUtil.GetStringAttribute(element, "palette_name");
        Path        = IntermediateUtil.GetStringAttribute(element, "path");

        var bitmap = element["bitmap"];
        if (bitmap != null)
        {
            int size = IntermediateUtil.GetIntAttribute(bitmap, "size");
            if (Format == ImageFormat.Comp4x4)
            {
                Bitmap = new byte[size * 4];
                var words = IntermediateUtil.GetInnerTextParts(bitmap)
                    .Select(v => uint.Parse(v, NumberStyles.HexNumber))
                    .ToArray();
                if (words.Length != size)
                    throw new Exception();
                IOUtil.WriteU32Le(Bitmap, words);
            }
            else
            {
                Bitmap = new byte[size * 2];
                var words = IntermediateUtil.GetInnerTextParts(bitmap)
                    .Select(v => ushort.Parse(v, NumberStyles.HexNumber))
                    .ToArray();
                if (words.Length != size)
                    throw new Exception();
                IOUtil.WriteU16Le(Bitmap, words);
            }
        }

        if (Format == ImageFormat.Comp4x4)
        {
            var tex4x4PaletteIdx = element["tex4x4_palette_idx"];
            if (tex4x4PaletteIdx != null)
            {
                int size = IntermediateUtil.GetIntAttribute(tex4x4PaletteIdx, "size");
                Tex4x4PaletteIdx = new byte[size * 2];
                var words = IntermediateUtil.GetInnerTextParts(tex4x4PaletteIdx)
                    .Select(v => ushort.Parse(v, NumberStyles.HexNumber))
                    .ToArray();
                if (words.Length != size)
                    throw new Exception();
                IOUtil.WriteU16Le(Tex4x4PaletteIdx, words);
            }
        }
    }

    public int           Index;
    public string        Name;
    public int           Width;
    public int           Height;
    public int           OriginalWidth;
    public int           OriginalHeight;
    public ImageFormat   Format;
    public TexColor0Mode Color0Mode;
    public string        PaletteName;
    public string        Path;

    public byte[] Bitmap;
    public byte[] Tex4x4PaletteIdx;
}