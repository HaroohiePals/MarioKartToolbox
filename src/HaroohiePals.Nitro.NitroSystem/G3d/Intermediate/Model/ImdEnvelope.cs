using System;
using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdEnvelope
{
    public ImdEnvelope(XmlElement element)
    {
        var weight  = element["weight"];
        var nodeIdx = element["node_idx"];
        if (weight == null || nodeIdx == null)
            throw new Exception();

        int weightSize  = IntermediateUtil.GetIntAttribute(weight, "size");
        int nodeIdxSize = IntermediateUtil.GetIntAttribute(nodeIdx, "size");
        if (weightSize != nodeIdxSize)
            throw new Exception();

        Weight  = IntermediateUtil.GetInnerTextParts(weight).Select(int.Parse).ToArray();
        NodeIdx = IntermediateUtil.GetInnerTextParts(nodeIdx).Select(int.Parse).ToArray();

        if (Weight.Length != weightSize)
            throw new Exception();

        if (NodeIdx.Length != nodeIdxSize)
            throw new Exception();
    }

    public int[] Weight;
    public int[] NodeIdx;
}