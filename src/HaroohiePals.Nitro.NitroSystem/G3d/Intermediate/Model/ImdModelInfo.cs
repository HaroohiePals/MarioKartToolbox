using System;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdModelInfo
{
    public enum ModelScalingRule
    {
        Standard,
        Maya,
        Si3d
    }

    public enum ModelVertexStyle
    {
        Direct,
        Index
    }

    public enum ModelTexMatrixMode
    {
        Maya,
        Si3d,
        Xsi,
        _3dsmax
    }

    public enum ModelCompressNode
    {
        None,
        Cull,
        Merge,
        Unite,
        UniteCombine
    }

    public enum ModelOutputTexture
    {
        Used,
        All
    }

    public ImdModelInfo(XmlElement element)
    {
        PosScale = IntermediateUtil.GetIntAttribute(element, "pos_scale");
        ScalingRule = element.GetAttribute("scaling_rule") switch
        {
            "standard" => ModelScalingRule.Standard,
            "maya"     => ModelScalingRule.Maya,
            "si3d"     => ModelScalingRule.Si3d,
            _          => throw new Exception()
        };
        VertexStyle = element.GetAttribute("vertex_style") switch
        {
            "direct" => ModelVertexStyle.Direct,
            "index"  => ModelVertexStyle.Index,
            _        => throw new Exception()
        };
        Magnify        = IntermediateUtil.GetDoubleAttribute(element, "magnify");
        ToolStartFrame = IntermediateUtil.GetIntAttribute(element, "tool_start_frame");
        TexMatrixMode = element.GetAttribute("tex_matrix_mode") switch
        {
            "maya"   => ModelTexMatrixMode.Maya,
            "si3d"   => ModelTexMatrixMode.Si3d,
            "xsi"    => ModelTexMatrixMode.Xsi,
            "3dsmax" => ModelTexMatrixMode._3dsmax,
            _        => throw new Exception()
        };
        CompressNode = element.GetAttribute("compress_node") switch
        {
            "none"          => ModelCompressNode.None,
            "cull"          => ModelCompressNode.Cull,
            "merge"         => ModelCompressNode.Merge,
            "unite"         => ModelCompressNode.Unite,
            "unite_combine" => ModelCompressNode.UniteCombine,
            _               => throw new Exception()
        };
        var nodeSize = IntermediateUtil.GetArrayAttribute(element, "node_size");
        if (nodeSize != null)
        {
            if (nodeSize.Length != 2)
                throw new Exception();

            NodeSizeOriginal   = int.Parse(nodeSize[0]);
            NodeSizeCompressed = int.Parse(nodeSize[1]);
        }

        CompressMaterial = IntermediateUtil.GetOnOffAttribute(element, "compress_material");

        var materialSize = IntermediateUtil.GetArrayAttribute(element, "material_size");
        if (materialSize != null)
        {
            if (materialSize.Length != 2)
                throw new Exception();

            MaterialSizeOriginal   = int.Parse(materialSize[0]);
            MaterialSizeCompressed = int.Parse(materialSize[1]);
        }

        OutputTexture = element.GetAttribute("output_texture") switch
        {
            "used" => ModelOutputTexture.Used,
            "all"  => ModelOutputTexture.All,
            _      => throw new Exception()
        };
        ForceFullWeight   = IntermediateUtil.GetOnOffAttribute(element, "force_full_weight");
        UsePrimitiveStrip = IntermediateUtil.GetOnOffAttribute(element, "use_primitive_strip");
    }

    public int                PosScale;
    public ModelScalingRule   ScalingRule;
    public ModelVertexStyle   VertexStyle;
    public double             Magnify;
    public int                ToolStartFrame;
    public ModelTexMatrixMode TexMatrixMode;
    public ModelCompressNode  CompressNode;
    public int                NodeSizeOriginal;
    public int                NodeSizeCompressed;
    public bool               CompressMaterial;
    public int                MaterialSizeOriginal;
    public int                MaterialSizeCompressed;
    public ModelOutputTexture OutputTexture;
    public bool               ForceFullWeight;
    public bool               UsePrimitiveStrip;
}