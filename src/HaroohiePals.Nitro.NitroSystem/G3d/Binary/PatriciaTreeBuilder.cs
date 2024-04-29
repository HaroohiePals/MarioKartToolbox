#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary;

public sealed class PatriciaTreeBuilder
{
    private readonly List<PatriciaTreeNode> _nodes;

    private PatriciaTreeBuilder()
    {
        var rootNode = new PatriciaTreeNode("\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0", 0, 127);
        _nodes = [ rootNode ];
    }

    private void AddEntry(string name, int entryIndex)
    {
        name = name.PadRight(16, '\0');
        var curNode = _nodes[0];
        var leftNode = curNode.LeftNode;
        int bit = 127;
        while (curNode.ReferenceBit > leftNode.ReferenceBit)
        {
            curNode = leftNode;
            leftNode = GetStringBit(name, leftNode.ReferenceBit) ? leftNode.RightNode : leftNode.LeftNode;
        }

        while (GetStringBit(leftNode.Name, bit) == GetStringBit(name, bit))
        {
            bit--;
        }

        curNode = _nodes[0];
        leftNode = curNode.LeftNode;
        while (curNode.ReferenceBit > leftNode.ReferenceBit && leftNode.ReferenceBit > bit)
        {
            curNode = leftNode;
            leftNode = GetStringBit(name, leftNode.ReferenceBit) ? leftNode.RightNode : leftNode.LeftNode;
        }

        var node = new PatriciaTreeNode(name, entryIndex, bit);
        node.LeftNode = GetStringBit(name, node.ReferenceBit) ? leftNode : node;
        node.RightNode = GetStringBit(name, node.ReferenceBit) ? node : leftNode;
        if (GetStringBit(name, curNode.ReferenceBit))
            curNode.RightNode = node;
        else
            curNode.LeftNode = node;
        _nodes.Add(node);
    }

    private static bool GetStringBit(string name, int bit)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(bit / 8, name.Length, nameof(bit));

        return (name[bit / 8] >> (bit & 7) & 1) == 1;
    }

    private void SortNodes()
    {
        //sort the nodes depth-first
        var visited = new HashSet<PatriciaTreeNode>();
        var stack = new Stack<PatriciaTreeNode>();
        stack.Push(_nodes[0]);
        _nodes.Clear();
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (visited.Contains(node))
                continue;
            visited.Add(node);
            _nodes.Add(node);
            stack.Push(node.RightNode);
            stack.Push(node.LeftNode);
        }
    }

    private G3dDictionaryPatriciaTreeNode[] ToG3dDictionaryNodes() => _nodes.Select(n => new G3dDictionaryPatriciaTreeNode
    {
        ReferenceBit = (byte)n.ReferenceBit,
        LeftNodeIndex = (byte)_nodes.IndexOf(n.LeftNode),
        RightNodeIndex = (byte)_nodes.IndexOf(n.RightNode),
        EntryIndex = (byte)n.EntryIndex
    }).ToArray();

    public static G3dDictionaryPatriciaTreeNode[] BuildFrom(IEnumerable<string> names)
    {
        var builder = new PatriciaTreeBuilder();
        int index = 0;
        foreach (string name in names)
        {
            builder.AddEntry(name, index++);
        }

        builder.SortNodes();
        return builder.ToG3dDictionaryNodes();
    }

    private class PatriciaTreeNode
    {
        public int ReferenceBit { get; }
        public PatriciaTreeNode LeftNode { get; set; }
        public PatriciaTreeNode RightNode { get; set; }
        public int EntryIndex { get; }
        public string Name { get; }

        public PatriciaTreeNode(string name, int entryIndex, int referenceBit)
        {
            Name = name;
            EntryIndex = entryIndex;
            ReferenceBit = referenceBit;
            LeftNode = this;
            RightNode = this;
        }

        public override string ToString() => Name.TrimEnd('\0');
    }
}