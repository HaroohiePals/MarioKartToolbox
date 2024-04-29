using HaroohiePals.Graphics;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdMaterial
{
    public enum STSource
    {
        Polygon,
        Material
    }

    public enum TexTiling
    {
        Clamp,
        Repeat,
        Flip
    }

    private static TexTiling ParseTexTiling(string value) => value switch
    {
        "clamp"  => TexTiling.Clamp,
        "repeat" => TexTiling.Repeat,
        "flip"   => TexTiling.Flip,
        _        => throw new ArgumentException(nameof(value))
    };

    public ImdMaterial(XmlElement element)
    {
        Index  = IntermediateUtil.GetIntAttribute(element, "index");
        Name   = IntermediateUtil.GetStringAttribute(element, "name");
        Light0 = IntermediateUtil.GetOnOffAttribute(element, "light0");
        Light1 = IntermediateUtil.GetOnOffAttribute(element, "light1");
        Light2 = IntermediateUtil.GetOnOffAttribute(element, "light2");
        Light3 = IntermediateUtil.GetOnOffAttribute(element, "light3");
        Face = element.GetAttribute("face") switch
        {
            "front" => GxCull.Back,
            "back"  => GxCull.Front,
            "both"  => GxCull.None,
            _       => throw new Exception()
        };
        Alpha = IntermediateUtil.GetIntAttribute(element, "alpha");
        if (Alpha < 0 || Alpha > 31)
            throw new Exception();
        WireMode = IntermediateUtil.GetOnOffAttribute(element, "wire_mode");
        PolygonMode = element.GetAttribute("polygon_mode") switch
        {
            "modulate"       => GxPolygonMode.Modulate,
            "decal"          => GxPolygonMode.Decal,
            "toon_highlight" => GxPolygonMode.ToonHighlight,
            "shadow"         => GxPolygonMode.Shadow,
            _                => throw new Exception()
        };
        PolygonId = IntermediateUtil.GetIntAttribute(element, "polygon_id");
        if (PolygonId < 0 || PolygonId > 63)
            throw new Exception();
        FogFlag = IntermediateUtil.GetOnOffAttribute(element, "fog_flag");
        if (element.HasAttribute("depth_test_decal"))
            DepthTestDecal = IntermediateUtil.GetOnOffAttribute(element, "depth_test_decal");
        if (element.HasAttribute("translucent_update_depth"))
            TranslucentUpdateDepth = IntermediateUtil.GetOnOffAttribute(element, "translucent_update_depth");
        if (element.HasAttribute("render_1_pixel"))
            Render1Pixel = IntermediateUtil.GetOnOffAttribute(element, "render_1_pixel");
        if (element.HasAttribute("far_clipping"))
            FarClipping = IntermediateUtil.GetOnOffAttribute(element, "far_clipping");
        Diffuse            = IntermediateUtil.GetRgb555Attribute(element, "diffuse");
        Ambient            = IntermediateUtil.GetRgb555Attribute(element, "ambient");
        Specular           = IntermediateUtil.GetRgb555Attribute(element, "specular");
        Emission           = IntermediateUtil.GetRgb555Attribute(element, "emission");
        ShininessTableFlag = IntermediateUtil.GetOnOffAttribute(element, "shininess_table_flag");
        TexImageIdx        = IntermediateUtil.GetIntAttribute(element, "tex_image_idx", -1);
        TexPaletteIdx      = IntermediateUtil.GetIntAttribute(element, "tex_palette_idx", -1);

        var texTiling = IntermediateUtil.GetArrayAttribute(element, "tex_tiling");
        if (texTiling != null)
        {
            if (texTiling.Length != 2)
                throw new Exception();

            TexTilingS = ParseTexTiling(texTiling[0]);
            TexTilingT = ParseTexTiling(texTiling[1]);
        }

        TexScale     = IntermediateUtil.GetVec2Attribute(element, "tex_scale");
        TexRotate    = IntermediateUtil.GetDoubleAttribute(element, "tex_rotate");
        TexTranslate = IntermediateUtil.GetVec2Attribute(element, "tex_translate");
        TexGenMode = element.GetAttribute("tex_gen_mode") switch
        {
            ""     => GxTexGen.None,
            "none" => GxTexGen.None,
            "tex"  => GxTexGen.TexCoord,
            "nrm"  => GxTexGen.Normal,
            "pos"  => GxTexGen.Vertex,
            _      => throw new Exception()
        };
        TexGenSTSrc = element.GetAttribute("tex_gen_st_src") switch
        {
            ""         => STSource.Polygon,
            "polygon"  => STSource.Polygon,
            "material" => STSource.Material,
            _          => throw new Exception()
        };
        TexGenST = IntermediateUtil.GetVec2Attribute(element, "tex_gen_st");
        if (element.HasAttribute("tex_effect_mtx"))
            TexEffectMtx = IntermediateUtil.GetMtx4Attribute(element, "tex_effect_mtx");
    }

    public int           Index;
    public string        Name;
    public bool          Light0;
    public bool          Light1;
    public bool          Light2;
    public bool          Light3;
    public GxCull        Face;
    public int           Alpha;
    public bool          WireMode;
    public GxPolygonMode PolygonMode;
    public int           PolygonId;
    public bool          FogFlag;
    public bool?         DepthTestDecal;
    public bool?         TranslucentUpdateDepth;
    public bool?         Render1Pixel;
    public bool?         FarClipping;
    public Rgb555        Diffuse;
    public Rgb555        Ambient;
    public Rgb555        Specular;
    public Rgb555        Emission;
    public bool          ShininessTableFlag;
    public int           TexImageIdx   = -1;
    public int           TexPaletteIdx = -1;
    public TexTiling     TexTilingS;
    public TexTiling     TexTilingT;
    public Vector2d      TexScale;
    public double        TexRotate;
    public Vector2d      TexTranslate;
    public GxTexGen      TexGenMode;
    public STSource      TexGenSTSrc;
    public Vector2d      TexGenST; //this has been replaced by TexEffectMtx in version 1.6.0 as it seems
    public Matrix4d?     TexEffectMtx;

    public G3dMaterial ToMaterial(Imd imd)
    {
        var result = new G3dMaterial();
        result.Flags = G3dMaterialFlags.Diffuse |
                       G3dMaterialFlags.Ambient |
                       G3dMaterialFlags.VtxColor |
                       G3dMaterialFlags.Specular |
                       G3dMaterialFlags.Emission |
                       G3dMaterialFlags.Shininess |
                       G3dMaterialFlags.TexPlttBase;
        result.PolygonAttributeMask      = 0x3F1F80FF;
        result.TexImageParamMask = 0xFFFFFFFF;
        result.MagW              = 1;
        result.MagH              = 1;
        if (TexImageIdx >= 0)
        {
            var texImage = imd.TexImageArray[TexImageIdx];
            result.OriginalWidth  = (ushort)texImage.OriginalWidth;
            result.OriginalHeight = (ushort)texImage.OriginalHeight;
        }

        result.SetLightEnableFlags(
            (Light0 ? 1u : 0) |
            (Light1 ? 2u : 0) |
            (Light2 ? 4u : 0) |
            (Light3 ? 8u : 0));
        result.SetCullMode(Face);
        result.SetAlpha((byte)Alpha);
        if (WireMode)
            result.Flags |= G3dMaterialFlags.Wireframe;
        result.SetPolygonMode(PolygonMode);
        result.SetPolygonId((byte)PolygonId);
        result.SetFogEnable(FogFlag);
        if (DepthTestDecal != null)
        {
            result.PolygonAttribute.DepthEquals =  DepthTestDecal.Value;
            result.PolygonAttributeMask         |= new GxPolygonAttr { DepthEquals = true };
        }

        if (TranslucentUpdateDepth != null)
        {
            result.PolygonAttribute.TranslucentDepthUpdate =  TranslucentUpdateDepth.Value;
            result.PolygonAttributeMask                    |= new GxPolygonAttr { TranslucentDepthUpdate = true };
        }

        if (Render1Pixel != null)
        {
            result.PolygonAttribute.Render1Dot =  Render1Pixel.Value;
            result.PolygonAttributeMask        |= new GxPolygonAttr { Render1Dot = true };
        }

        if (FarClipping != null)
        {
            result.PolygonAttribute.FarClip =  FarClipping.Value;
            result.PolygonAttributeMask     |= new GxPolygonAttr { FarClip = true };
        }

        result.SetDiffuse(Diffuse);
        result.SetAmbient(Ambient);
        result.SetSpecular(Specular);
        result.SetEmission(Emission);
        result.DiffuseAmbient |= 0x8000;
        if (ShininessTableFlag)
            result.SpecularEmission |= 0x8000;
        if (TexImageIdx >= 0)
        {
            result.ScaleS = TexScale.X;
            result.ScaleT = TexScale.Y;
            if (TexScale == Vector2d.One)
                result.Flags |= G3dMaterialFlags.TexMtxScaleOne;
            result.RotationSin = System.Math.Sin(MathHelper.DegreesToRadians(TexRotate));
            result.RotationCos = System.Math.Cos(MathHelper.DegreesToRadians(TexRotate));
            if (TexRotate == 0)
                result.Flags |= G3dMaterialFlags.TexMtxRotZero;
            result.TranslationS = TexTranslate.X;
            result.TranslationT = TexTranslate.Y;
            if (TexTranslate == Vector2d.Zero)
                result.Flags |= G3dMaterialFlags.TexMtxTransZero;
        }
        else
        {
            result.Flags |= G3dMaterialFlags.TexMtxScaleOne;
            result.Flags |= G3dMaterialFlags.TexMtxRotZero;
            result.Flags |= G3dMaterialFlags.TexMtxTransZero;
        }

        result.TexImageParam.TexGen  = TexGenMode;
        result.TexImageParam.RepeatS = TexTilingS != TexTiling.Clamp;
        result.TexImageParam.FlipS   = TexTilingS == TexTiling.Flip;
        result.TexImageParam.RepeatT = TexTilingT != TexTiling.Clamp;
        result.TexImageParam.FlipT   = TexTilingT == TexTiling.Flip;
        if (TexEffectMtx != null)
            result.EffectMtx = TexEffectMtx.Value;
        else
            result.EffectMtx = Matrix4d.Identity;

        if (TexImageIdx >= 0 && (TexGenMode == GxTexGen.Normal || TexGenMode == GxTexGen.Vertex))
            result.Flags |= G3dMaterialFlags.EffectMtx;

        if (TexImageIdx >= 0 && TexGenMode != GxTexGen.None)
            result.Flags |= G3dMaterialFlags.TexMtxUse;

        return result;
    }
}