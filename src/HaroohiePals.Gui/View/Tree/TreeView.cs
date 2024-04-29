using ImGuiNET;
using System;
using System.Collections.Generic;

namespace HaroohiePals.Gui.View.Tree;

public class TreeView : IView
{
    public string Id { get; }
    public List<TreeNode> Nodes { get; } = new();
    public TreeNode SelectedNode { get; set; }

    public event Action<TreeView> SelectionChanged;

    private TreeNode _newSelectedNode;

    public TreeView(string id)
    {
        Id = id;
    }

    public bool Draw()
    {
        ImGui.BeginChild(Id);
        {
            _newSelectedNode = SelectedNode;

            foreach (var node in Nodes)
                DrawNode(node);

            var oldSelection = SelectedNode;
            SelectedNode = _newSelectedNode;
            if (SelectedNode != oldSelection)
                SelectionChanged?.Invoke(this);
            ImGui.EndChild();
        }

        return true;
    }

    private void DrawNode(TreeNode node)
    {
        var flags = ImGuiTreeNodeFlags.SpanFullWidth;
        if (node.IsLeaf)
            flags |= ImGuiTreeNodeFlags.Leaf;
        if (SelectedNode == node)
            flags |= ImGuiTreeNodeFlags.Selected;
        if (node.DefaultOpen)
            flags |= ImGuiTreeNodeFlags.DefaultOpen;

        if (ImGui.TreeNodeEx(node.Label, flags))
        {
            if (ImGui.IsItemToggledOpen() || ImGui.IsItemClicked())
                _newSelectedNode = node;

            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                node.Activate?.Invoke(node);

            foreach (var child in node.Children)
                DrawNode(child);

            ImGui.TreePop();
        }
    }
}