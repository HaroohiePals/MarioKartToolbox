using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdSbcGenerator
{
    private record DisplayEx(ImdNode.Display Display, ImdPolygon Polygon, int VisNode, int MinRenderNodeOrder);

    private readonly Imd _imd;

    private readonly SbcBuilder _builder = new();

    private readonly int[] _mtxStackSlot;
    private readonly int[] _nodeStackSlot;
    private readonly int[] _stackSlots = new int[31];

    private readonly List<ImdMatrix> _remainingMixMatrices = new();

    private readonly int[] _nodeOrder;
    private readonly int[] _mtxMinNodeOrder;

    private readonly HashSet<DisplayEx> _remainingNoPrioDisplays = new();
    private readonly List<DisplayEx>    _remainingPrioDisplays   = new();

    private int FindFreeMtxStackSlot()
    {
        for (int i = 0; i < 31; i++)
        {
            if (_stackSlots[i] == -1)
                return i;
        }

        return -1;
    }

    private void GenerateNodeOrder()
    {
        var stack = new Stack<ImdNode>();
        stack.Push(_imd.NodeArray[0]);
        int order = 0;
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            _nodeOrder[node.Index] = order++;

            if (node.BrotherNext != -1)
                stack.Push(_imd.NodeArray[node.BrotherNext]);

            if (node.Child != -1)
                stack.Push(_imd.NodeArray[node.Child]);
        }
    }

    private void GenerateMtxOrder()
    {
        foreach (var mtx in _imd.MatrixArray)
            _mtxMinNodeOrder[mtx.Index] = mtx.GetUsedNodes(_imd).Select(node => _nodeOrder[node]).Max();
    }

    private ImdSbcGenerator(Imd imd)
    {
        _imd = imd;

        Array.Fill(_stackSlots, -1);
        _mtxStackSlot = new int[imd.MatrixArray.Length];
        Array.Fill(_mtxStackSlot, -1);
        _nodeStackSlot = new int[imd.NodeArray.Length];
        Array.Fill(_nodeStackSlot, -1);

        _remainingMixMatrices.AddRange(imd.MatrixArray.Where(mtx => mtx.MtxWeight > 1));

        _nodeOrder = new int[imd.NodeArray.Length];
        GenerateNodeOrder();

        _mtxMinNodeOrder = new int[imd.MatrixArray.Length];
        GenerateMtxOrder();

        foreach (var node in _imd.NodeArray)
        {
            foreach (var display in node.Displays)
            {
                var polygon = _imd.PolygonArray[display.Polygon];
                var dispEx = new DisplayEx(display, polygon, node.Index,
                    polygon.MtxPrims[0].MtxList.Select(mtx => _mtxMinNodeOrder[mtx]).Max());
                if (display.Priority == 0)
                    _remainingNoPrioDisplays.Add(dispEx);
                else
                    _remainingPrioDisplays.Add(dispEx);
            }
        }

        _remainingPrioDisplays.Sort((a, b) => a.Display.Priority.CompareTo(b.Display.Priority));
    }

    private DisplayEx[] TakeAvailableDisplays(int order)
    {
        var displaysTmp =
            new List<DisplayEx>(_remainingNoPrioDisplays.Where(d => d.MinRenderNodeOrder <= order));
        _remainingNoPrioDisplays.ExceptWith(displaysTmp);

        int lastPrio = -1;
        while (_remainingPrioDisplays.Count > 0)
        {
            int prio = _remainingPrioDisplays[0].Display.Priority;
            if (prio == lastPrio)
                break;

            var prioDisplays = _remainingPrioDisplays
                .Where(d => d.MinRenderNodeOrder <= order && d.Display.Priority == prio)
                .ToArray();

            foreach (var display in prioDisplays)
                _remainingPrioDisplays.Remove(display);

            displaysTmp.AddRange(prioDisplays);

            lastPrio = prio;
        }

        return displaysTmp.OrderBy(d => d.Display.Priority)
            .ThenBy(d => _imd.PolygonArray[d.Display.Polygon].PolygonSize).ToArray();
    }

    private ImdMatrix[] TakeAvailableNodeMixMatrices(int order)
    {
        var matrices = _remainingMixMatrices.Where(mtx => _mtxMinNodeOrder[mtx.Index] <= order).ToArray();
        foreach (var mtx in matrices)
            _remainingMixMatrices.Remove(mtx);
        return matrices;
    }

    private (byte[] sbc, int[] mtxStackSlot) GenerateSbc()
    {
        var matUsed = new bool[_imd.MaterialArray.Length];

        int  curNodeMtx = -1;
        int  curVisNode = -1;
        bool posScaleOn = false;

        var stack = new Stack<ImdNode>();
        stack.Push(_imd.NodeArray[0]);

        bool isNodeMtxStillUsed(int nodeId)
        {
            return _remainingNoPrioDisplays.Any(d =>
                       d.Polygon.MtxPrims[0].MtxList
                           .Any(mtx => _imd.MatrixArray[mtx].UsesNode(_imd, nodeId))) ||
                   _remainingPrioDisplays.Any(d =>
                       d.Polygon.MtxPrims[0].MtxList
                           .Any(mtx => _imd.MatrixArray[mtx].UsesNode(_imd, nodeId))) ||
                   stack.Any(n => n.Parent == nodeId && (n.BrotherPrev != -1 || n.BrotherNext != -1));
        }

        bool isMixMtxStillUsed(int mtxId)
        {
            return _remainingNoPrioDisplays.Any(d =>
                       d.Polygon.MtxPrims[0].MtxList.Contains(mtxId)) ||
                   _remainingPrioDisplays.Any(d =>
                       d.Polygon.MtxPrims[0].MtxList.Contains(mtxId));
        }

        void cleanMtxStack()
        {
            for (int i = 0; i < 31; i++)
            {
                if (_stackSlots[i] == -1)
                    continue;

                if ((_stackSlots[i] & 0x1000) != 0)
                {
                    if (!isMixMtxStillUsed(_stackSlots[i] & 0xFF))
                        _stackSlots[i] = -1;
                }
                else
                {
                    if (!isNodeMtxStillUsed(_stackSlots[i]))
                        _stackSlots[i] = -1;
                }
            }
        }

        while (stack.Count > 0)
        {
            var node = stack.Pop();

            int order = _nodeOrder[node.Index];

            bool stackStore = false;

            if (node.Child != -1 && _imd.NodeArray[node.Child].BrotherNext != -1)
                stackStore = true;

            var mixMatrices = TakeAvailableNodeMixMatrices(order);

            if (mixMatrices.Length > 0)
                stackStore = true;

            var displays = TakeAvailableDisplays(order);
            if (displays.Length > 1 || (displays.Length == 1 && displays[0].Polygon.MtxPrims[0].MtxList.Length > 1))
                stackStore = true;

            if (_remainingMixMatrices.Any(mtx => mtx.UsesNode(_imd, node.Index)))
                stackStore = true;

            if (_remainingNoPrioDisplays.Any(d =>
                    d.Polygon.MtxPrims[0].MtxList.Any(mtx => _imd.MatrixArray[mtx].UsesNode(_imd, node.Index))) ||
                _remainingPrioDisplays.Any(d =>
                    d.Polygon.MtxPrims[0].MtxList.Any(mtx => _imd.MatrixArray[mtx].UsesNode(_imd, node.Index))))
                stackStore = true;

            int restoreMtx = -1;
            if (node.Parent != -1 && curNodeMtx != node.Parent)
                restoreMtx = _nodeStackSlot[node.Parent];

            int storeMtx = -1;
            if (stackStore)
            {
                storeMtx                   = FindFreeMtxStackSlot();
                _stackSlots[storeMtx]      = node.Index;
                _nodeStackSlot[node.Index] = storeMtx;
                foreach (var mtx in _imd.MatrixArray)
                {
                    if (mtx.MtxWeight == 1 && mtx.NodeIdx == node.Index)
                        _mtxStackSlot[mtx.Index] = storeMtx;
                }
            }

            //todo: Can a ssc parent have more than one child, and could there then be one without scale compensate on?
            bool sscParent = node.Child != -1 && _imd.NodeArray[node.Child].ScaleCompensate;

            _builder.NodeDescription((byte)node.Index, (byte)(node.Parent == -1 ? 0 : node.Parent), node.ScaleCompensate,
                sscParent, storeMtx, restoreMtx);
            curNodeMtx = node.Index;

            foreach (var nodeMix in mixMatrices)
            {
                var nodes   = _imd.Envelope.NodeIdx.AsSpan(nodeMix.EnvelopeHead, nodeMix.MtxWeight);
                var weights = _imd.Envelope.Weight.AsSpan(nodeMix.EnvelopeHead, nodeMix.MtxWeight);

                var entries = new SbcBuilder.NodeMixMtx[nodeMix.MtxWeight];
                for (int i = 0; i < nodeMix.MtxWeight; i++)
                {
                    entries[i] = new SbcBuilder.NodeMixMtx((byte)_nodeStackSlot[nodes[i]], (byte)nodes[i],
                        (byte)System.Math.Clamp((int)System.Math.Round(weights[i] * 256d / 100d), 0, 255));
                }

                int slot = FindFreeMtxStackSlot();
                _mtxStackSlot[nodeMix.Index] = slot;
                _stackSlots[slot]            = 0x1000 | nodeMix.Index;
                _builder.NodeMix((byte)slot, entries);

                curNodeMtx = 0x1000 | nodeMix.Index;
            }

            for (int i = 0; i < displays.Length; i++)
            {
                var display = displays[i];
                if (curVisNode != display.VisNode)
                {
                    _builder.Node((byte)display.VisNode, _imd.NodeArray[display.VisNode].Visibility);
                    curVisNode = display.VisNode;
                }

                var shp = display.Polygon;
                if (shp.MtxPrims[0].PrimitiveArray[0].Elements[0] is ImdPrimitiveElement.Mtx mtxElement)
                {
                    var mtx = _imd.MatrixArray[shp.MtxPrims[0].MtxList[mtxElement.Idx]];

                    int mtxNode = mtx.NodeIdx;

                    if (node.Billboard == ImdNode.BillboardType.On)
                    {
                        _builder.Billboard((byte)mtxNode, -1, curNodeMtx != mtxNode ? _nodeStackSlot[mtxNode] : -1);
                        curNodeMtx = -1;
                        posScaleOn = false;
                    }
                    else if (node.Billboard == ImdNode.BillboardType.YOn)
                    {
                        _builder.BillboardY((byte)mtxNode, -1, curNodeMtx != mtxNode ? _nodeStackSlot[mtxNode] : -1);
                        curNodeMtx = -1;
                        posScaleOn = false;
                    }
                    else if (mtx.MtxWeight > 1 && curNodeMtx != (mtx.Index | 0x1000))
                    {
                        _builder.Matrix((byte)_mtxStackSlot[mtx.Index]);
                        curNodeMtx = mtx.Index | 0x1000;
                        posScaleOn = false;
                    }
                    else if (mtx.MtxWeight == 1 && curNodeMtx != mtxNode)
                    {
                        _builder.Matrix((byte)_nodeStackSlot[mtxNode]);
                        curNodeMtx = mtxNode;
                        posScaleOn = false;
                    }

                    if (_imd.ModelInfo.PosScale != 0 && !posScaleOn)
                    {
                        _builder.PosScale(false);
                        posScaleOn = true;
                    }
                }

                var mat = _imd.MaterialArray[display.Display.Material];

                bool matUsedAgain = false;
                for (int j = i + 1; j < displays.Length; j++)
                {
                    if (displays[j].Display.Material == display.Display.Material)
                        matUsedAgain = true;
                }

                if (!matUsedAgain)
                    matUsedAgain = _remainingNoPrioDisplays.Concat(_remainingPrioDisplays)
                        .Any(d => d.Display.Material == display.Display.Material);

                SbcBuilder.MaterialHint hint;
                if (matUsedAgain)
                    hint = SbcBuilder.MaterialHint.UsedAgain;
                else if (!matUsed[display.Display.Material])
                    hint = SbcBuilder.MaterialHint.UsedOnce;
                else
                    hint = SbcBuilder.MaterialHint.LastUse;

                _builder.Material((byte)display.Display.Material, hint);

                matUsed[display.Display.Material] = true;

                if (mat.TexGenMode == GxTexGen.Normal)
                    _builder.EnvironmentMap((byte)display.Display.Material);
                else if (mat.TexGenMode == GxTexGen.Vertex)
                    _builder.ProjectionMap((byte)display.Display.Material);
                _builder.Shape((byte)display.Display.Polygon);

                if (display.Polygon.MtxPrims[0].MtxList.Length > 1)
                {
                    curNodeMtx = -1;
                    posScaleOn = false;
                }
            }

            if (curNodeMtx == node.Index && posScaleOn)
                _builder.PosScale(true);

            if (node.BrotherNext != -1)
                stack.Push(_imd.NodeArray[node.BrotherNext]);

            if (node.Child != -1)
                stack.Push(_imd.NodeArray[node.Child]);

            cleanMtxStack();
        }

        _builder.Return();

        return (_builder.ToArray(), _mtxStackSlot);
    }


    public static (byte[] sbc, int[] mtxStackSlot) GenerateSbc(Imd imd)
    {
        return new ImdSbcGenerator(imd).GenerateSbc();
    }
}