using System;
using System.Collections.Generic;

namespace HaroohiePals.Gui.View.Tree;

public class TreeNode
{
    public TreeNode(string label, bool isLeaf)
    {
        Label = label;
        IsLeaf = isLeaf;
    }

    public bool IsLeaf { get; set; }
    public bool DefaultOpen { get; set; }
    public string Label { get; set; }
    public object Data { get; set; }
    public List<TreeNode> Children { get; } = new();

    public Action<TreeNode> Activate { get; set; }
}