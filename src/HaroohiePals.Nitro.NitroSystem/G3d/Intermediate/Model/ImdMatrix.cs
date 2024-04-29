using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdMatrix
{
    public ImdMatrix(XmlElement element)
    {
        Index        = IntermediateUtil.GetIntAttribute(element, "index");
        MtxWeight    = IntermediateUtil.GetIntAttribute(element, "mtx_weight");
        EnvelopeHead = IntermediateUtil.GetIntAttribute(element, "envelope_head");
        NodeIdx      = IntermediateUtil.GetIntAttribute(element, "node_idx");
    }

    public int Index;
    public int MtxWeight;
    public int EnvelopeHead;
    public int NodeIdx;

    public bool UsesNode(Imd imd, int node) 
        => GetUsedNodes(imd).Contains(node);

    public int[] GetUsedNodes(Imd imd)
    {
        if (MtxWeight < 1)
            return new int[0];

        if (MtxWeight == 1)
            return new[] { NodeIdx };

        return imd.Envelope.NodeIdx[EnvelopeHead .. (EnvelopeHead + MtxWeight)];
    }
}