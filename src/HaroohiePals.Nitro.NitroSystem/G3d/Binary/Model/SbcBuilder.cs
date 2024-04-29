using System;
using System.IO;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;

public sealed class SbcBuilder
{
    public enum MaterialHint : byte
    {
        UsedOnce = Sbc.SbcFlg000,
        UsedAgain = Sbc.SbcFlg001,
        LastUse = Sbc.SbcFlg010
    }

    public readonly record struct NodeMixMtx(byte SrcIdx, byte NodeId, byte Ratio);

    private readonly MemoryStream _stream = new();

    public void Nop()
    {
        _stream.WriteByte((byte)SbcCommand.Nop);
    }

    public void Return()
    {
        _stream.WriteByte((byte)SbcCommand.Return);
    }

    public void Node(byte nodeId, bool visible)
    {
        _stream.WriteByte((byte)SbcCommand.Node);
        _stream.WriteByte(nodeId);
        _stream.WriteByte((byte)(visible ? 1 : 0));
    }

    public void Matrix(byte idx)
    {
        _stream.WriteByte((byte)SbcCommand.Matrix);
        _stream.WriteByte(idx);
    }

    public void Material(byte matId, MaterialHint hint)
    {
        _stream.WriteByte((byte)((byte)SbcCommand.Material | (byte)hint));
        _stream.WriteByte(matId);
    }

    public void Shape(byte shpId)
    {
        _stream.WriteByte((byte)SbcCommand.Shape);
        _stream.WriteByte(shpId);
    }

    public void NodeDescription(byte nodeId, byte parentNodeId, bool applyMayaSsc, bool isMayaSscParent, int destIdx = -1,
        int srcIdx = -1)
    {
        byte opt = 0;
        if (destIdx != -1)
            opt |= Sbc.SbcFlg001;
        if (srcIdx != -1)
            opt |= Sbc.SbcFlg010;

        _stream.WriteByte((byte)((byte)SbcCommand.NodeDescription | opt));
        _stream.WriteByte(nodeId);
        _stream.WriteByte(parentNodeId);
        _stream.WriteByte((byte)((applyMayaSsc ? 1 : 0) | (isMayaSscParent ? 2 : 0)));
        if (destIdx != -1)
            _stream.WriteByte((byte)(destIdx & 0x1F));
        if (srcIdx != -1)
            _stream.WriteByte((byte)(srcIdx & 0x1F));
    }

    public void Billboard(byte nodeId, int destIdx = -1, int srcIdx = -1)
    {
        byte opt = 0;
        if (destIdx != -1)
            opt |= Sbc.SbcFlg001;
        if (srcIdx != -1)
            opt |= Sbc.SbcFlg010;

        _stream.WriteByte((byte)((byte)SbcCommand.Billboard | opt));
        _stream.WriteByte(nodeId);
        if (destIdx != -1)
            _stream.WriteByte((byte)(destIdx & 0x1F));
        if (srcIdx != -1)
            _stream.WriteByte((byte)(srcIdx & 0x1F));
    }

    public void BillboardY(byte nodeId, int destIdx = -1, int srcIdx = -1)
    {
        byte opt = 0;
        if (destIdx != -1)
            opt |= Sbc.SbcFlg001;
        if (srcIdx != -1)
            opt |= Sbc.SbcFlg010;

        _stream.WriteByte((byte)((byte)SbcCommand.BillboardY | opt));
        _stream.WriteByte(nodeId);
        if (destIdx != -1)
            _stream.WriteByte((byte)(destIdx & 0x1F));
        if (srcIdx != -1)
            _stream.WriteByte((byte)(srcIdx & 0x1F));
    }

    public void NodeMix(byte destIdx, ReadOnlySpan<NodeMixMtx> matrices)
    {
        _stream.WriteByte((byte)SbcCommand.NodeMix);
        _stream.WriteByte(destIdx);
        _stream.WriteByte((byte)matrices.Length);
        foreach (var mtx in matrices)
        {
            _stream.WriteByte(mtx.SrcIdx);
            _stream.WriteByte(mtx.NodeId);
            _stream.WriteByte(mtx.Ratio);
        }
    }

    public void CallDisplayList(ReadOnlySpan<byte> displayList)
    {
        throw new NotImplementedException();
    }

    public void PosScale(bool inverse)
    {
        if (inverse)
            _stream.WriteByte((byte)SbcCommand.PosScale | Sbc.SbcFlg001);
        else
            _stream.WriteByte((byte)SbcCommand.PosScale | Sbc.SbcFlg000);
    }

    public void EnvironmentMap(byte matId)
    {
        _stream.WriteByte((byte)SbcCommand.EnvironmentMap);
        _stream.WriteByte(matId);
        _stream.WriteByte(0);
    }

    public void ProjectionMap(byte matId)
    {
        _stream.WriteByte((byte)SbcCommand.ProjectionMap);
        _stream.WriteByte(matId);
        _stream.WriteByte(0);
    }

    public byte[] ToArray() => _stream.ToArray();
}