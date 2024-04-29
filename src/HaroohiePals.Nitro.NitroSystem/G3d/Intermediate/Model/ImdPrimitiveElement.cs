using HaroohiePals.Graphics;
using HaroohiePals.Nitro.G3;
using OpenTK.Mathematics;
using System;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public abstract class ImdPrimitiveElement
{
    public abstract void WriteCommand(DisplayListBuilder builder);

    public class Mtx : ImdPrimitiveElement
    {
        public Mtx(XmlElement element)
            => Idx = IntermediateUtil.GetIntAttribute(element, "idx");

        public int Idx;

        public override void WriteCommand(DisplayListBuilder builder)
        {
            //needs special consideration
            throw new NotImplementedException();
        }
    }

    public class Tex : ImdPrimitiveElement
    {
        public Tex(XmlElement element)
            => St = IntermediateUtil.GetVec2Attribute(element, "st");

        public Vector2d St;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.TexCoord(St);
    }

    public class Nrm : ImdPrimitiveElement
    {
        public Nrm(XmlElement element)
            => Xyz = IntermediateUtil.GetVec3Attribute(element, "xyz");

        public Vector3d Xyz;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.Normal(Xyz);
    }

    public class Clr : ImdPrimitiveElement
    {
        public Clr(XmlElement element)
            => Rgb = IntermediateUtil.GetRgb555Attribute(element, "rgb");

        public Rgb555 Rgb;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.Color(Rgb);
    }

    public class PosS : ImdPrimitiveElement
    {
        public PosS(XmlElement element)
            => Xyz = IntermediateUtil.GetVec3Attribute(element, "xyz");

        public Vector3d Xyz;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.Vertex10(Xyz);
    }

    public class PosXy : ImdPrimitiveElement
    {
        public PosXy(XmlElement element)
            => Xy = IntermediateUtil.GetVec2Attribute(element, "xy");

        public Vector2d Xy;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.VertexXY(Xy);
    }

    public class PosXz : ImdPrimitiveElement
    {
        public PosXz(XmlElement element)
            => Xz = IntermediateUtil.GetVec2Attribute(element, "xz");

        public Vector2d Xz;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.VertexXZ(Xz);
    }

    public class PosYz : ImdPrimitiveElement
    {
        public PosYz(XmlElement element)
            => Yz = IntermediateUtil.GetVec2Attribute(element, "yz");

        public Vector2d Yz;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.VertexYZ(Yz);
    }

    public class PosXyz : ImdPrimitiveElement
    {
        public PosXyz(XmlElement element)
            => Xyz = IntermediateUtil.GetVec3Attribute(element, "xyz");

        public Vector3d Xyz;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.Vertex(Xyz);
    }

    public class PosDiff : ImdPrimitiveElement
    {
        public PosDiff(XmlElement element)
            => Xyz = IntermediateUtil.GetVec3Attribute(element, "xyz");

        public Vector3d Xyz;

        public override void WriteCommand(DisplayListBuilder builder)
            => builder.VertexDiff(Xyz);
    }

    public static ImdPrimitiveElement Parse(XmlElement element) => element.Name switch
    {
        "mtx"      => new Mtx(element),
        "tex"      => new Tex(element),
        "nrm"      => new Nrm(element),
        "clr"      => new Clr(element),
        "pos_s"    => new PosS(element),
        "pos_xy"   => new PosXy(element),
        "pos_yz"   => new PosYz(element),
        "pos_xz"   => new PosXz(element),
        "pos_xyz"  => new PosXyz(element),
        "pos_diff" => new PosDiff(element),
        "clr_idx"  => throw new NotImplementedException(),
        "pos_idx"  => throw new NotImplementedException(),
        _          => throw new Exception()
    };
}