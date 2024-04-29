using HaroohiePals.Gui;
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Toolbar;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class MapDataExplorerView : IView
{
    private ToolbarView _toolbar = new("MapDataToolbar");
    private ToolbarItem _moveUpButton;
    private ToolbarItem _moveDownButton;

    private MapDataExplorerViewModel _viewModel;

    private string _lastSelectedNodeId;

    private List<MapDataTreeNode> _nodes;
    private MapDataTreeNode[] _selectedNodes => GetSelectedNodes(_nodes);

    /// <summary>
    /// Used for shift selection
    /// </summary>
    private List<int> _indicesToSelect = new();

    private MapDataTreeNode _lastSelectedNode
    {
        get
        {
            var selectedNodes = _selectedNodes;

            if (selectedNodes.Count() == 0)
                return null;

            var found = selectedNodes.FirstOrDefault(x => x.UniqueId == _lastSelectedNodeId);

            if (found == null)
            {
                found = selectedNodes[^1];
                _lastSelectedNodeId = found.UniqueId;
            }

            return found;
        }
    }

    /// <summary>
    /// Disable selection clicks until the next frame (Used for drag and drop)
    /// </summary>
    private bool _suspendSelectionClick = false;

    private MapDataTreeNode _sourceDragNode;

    public MapDataExplorerView(MapDataExplorerViewModel viewModel)
    {
        _viewModel = viewModel;

        _moveUpButton = new ToolbarItem(FontAwesome6.ArrowUp[0], "Move up", null /*PerformMoveUp*/);
        _moveDownButton = new ToolbarItem(FontAwesome6.ArrowDown[0], "Move down", null /*PerformMoveDown*/);

        _toolbar.Items.Add(_moveUpButton);
        _toolbar.Items.Add(_moveDownButton);
    }

    private MapDataTreeNode CreateFromMapDataCollection<T>(MapDataCollection<T> collection, string name,
        string childrenName = null, object data = null, MapDataTreeNode parent = null)
        where T : IMapDataEntry
    {
        if (data == null)
            data = collection;

        if (collection == null)
            return new MapDataTreeNode("", FontAwesome6.Folder, name, false) { Data = name };

        var root = new MapDataTreeNode($"##{collection.GetHashCode()}", FontAwesome6.Folder, name, false)
        {
            Data = data,
            ShowItemCount = true,
            Parent = parent,
            Collection = collection
        };

        int i = 0;
        foreach (var entry in collection)
        {
            MapDataTreeNode child;

            switch (entry)
            {
                case MkdsMapObject mobj:
                    child = new MapDataTreeNode($"##{entry.GetHashCode()}", FontAwesome6.Cube,
                        $"{mobj.GetDisplayName()} #{i}", true)
                    {
                        Data = entry,
                        ShowVisibilityButton = false
                    };
                    break;
                case MkdsItemPath ipat:
                    child = CreateFromMapDataCollection(ipat.Points, $"Path #{i}", "Point", ipat);
                    child.Collection = ipat.Points;
                    break;
                case MkdsEnemyPath epat:
                    child = CreateFromMapDataCollection(epat.Points, $"Path #{i}", "Point", epat);
                    child.Collection = epat.Points;
                    break;
                case MkdsMgEnemyPath mepa:
                    child = CreateFromMapDataCollection(mepa.Points, $"Path #{i}", "Point", mepa);
                    child.Collection = mepa.Points;
                    break;
                case MkdsCheckPointPath cpat:
                    child = CreateFromMapDataCollection(cpat.Points, $"Path #{i}", "Point", cpat);
                    child.Collection = cpat.Points;
                    break;
                case MkdsPath path:
                    child = CreateFromMapDataCollection(path.Points, $"Path #{i}", "Point", path);
                    child.Collection = path.Points;
                    break;
                default:
                    child = new MapDataTreeNode($"##{entry.GetHashCode()}", FontAwesome6.Cube,
                        $"{childrenName ?? entry.GetDisplayName()} #{i}", true)
                    {
                        Data = entry,
                        ShowVisibilityButton = false
                    };
                    break;
            }

            if (child != null)
            {
                child.Parent = root;
                child.CanDrag = true;
                child.Index = i;
                child.Activate = _ => { _viewModel.Context.PerformFrameSelection(); };
                root.Children.Add(child);
            }

            i++;
        }

        return root;
    }

    private List<MapDataTreeNode> CreateNodes()
    {
        List<MapDataTreeNode> nodes = new();

        nodes.Add(new MapDataTreeNode($"##{_viewModel.Context.Course.MapData.StageInfo.GetHashCode()}", FontAwesome6.Info,
            "Stage Information", true)
        { Data = _viewModel.Context.Course.MapData.StageInfo });

        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.MapObjects, "Map Objects"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.Paths, "Paths"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.StartPoints, "Start Points"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.RespawnPoints, "Respawn Points"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.KartPoint2D, "Kart Point 2D"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.CannonPoints, "Cannon Points"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.KartPointMission, "Mission Points"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.CheckPointPaths, "Checkpoint Paths"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.ItemPaths, "Item Paths"));
        if (_viewModel.Context.Course.MapData.IsMgStage)
            nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.MgEnemyPaths, "Battle Enemy Paths"));
        else
            nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.EnemyPaths, "Enemy Paths"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.Areas, "Areas"));
        nodes.Add(CreateFromMapDataCollection(_viewModel.Context.Course.MapData.Cameras, "Cameras"));

        return nodes;
    }

    //private List<MapDataTreeNode> GetAdjacentSelectedNodes()
    //{
    //    var result = new List<MapDataTreeNode>();

    //    var selectedNodes = _selectedNodes;

    //    if (_lastSelectedNode == null)
    //    {
    //        if (selectedNodes != null && selectedNodes.Count > 0)
    //            _lastSelectedNodeId = _selectedNodes[^1].UniqueId;
    //        else
    //            return result;
    //    }

    //    var lastSelectedNode = _lastSelectedNode;

    //    var candidates = selectedNodes.Where(x => x.Parent?.Collection == lastSelectedNode.Parent?.Collection)
    //        .ToList();

    //    result.Add(lastSelectedNode);

    //    // Traverse down
    //    for (int i = lastSelectedNode.Index + 1; ; i++)
    //    {
    //        var candidate = candidates.FirstOrDefault(x => x.Index == i);

    //        if (candidate == null)
    //            break;

    //        result.Add(candidate);
    //    }

    //    if (result.Count <= 1)
    //    {
    //        // Traverse up
    //        for (int i = lastSelectedNode.Index - 1; ; i--)
    //        {
    //            var candidate = candidates.FirstOrDefault(x => x.Index == i);

    //            if (candidate == null)
    //                break;

    //            result.Add(candidate);
    //        }
    //    }

    //    return result;
    //}

    private MapDataTreeNode[] GetSelectedNodes(IEnumerable<MapDataTreeNode> nodes)
    {
        var selected = new List<MapDataTreeNode>();

        if (nodes == null || nodes.Count() == 0)
            return selected.ToArray();

        foreach (var node in nodes)
        {
            if (_viewModel.Context.SceneObjectHolder.IsSelected(node.Data))
                selected.Add(node);
            selected.AddRange(GetSelectedNodes(node.Children.Cast<MapDataTreeNode>()));
        }

        return selected.ToArray();
    }

    public bool Draw()
    {
        if (_suspendSelectionClick)
            _suspendSelectionClick = false;

        //_moveDownButton.Enabled = CanMoveDown();
        //_moveUpButton.Enabled = CanMoveUp();

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4));
        if (ImGui.Begin("Map Data Explorer"))
        {
            ImGui.SetWindowSize(new Vector2(400, 600), ImGuiCond.Once);

            _toolbar.Draw();
            DrawTree();

            HandlePaste();
            HandleInsert();
            HandleSelectAll();

            ImGui.End();
        }

        ImGui.PopStyleVar();

        if (_sourceDragNode != null && !ImGui.IsMouseDragging(ImGuiMouseButton.Left))
            ClearDragDrop();

        _viewModel.ExpandSelection = false;

        return true;
    }

    private void DrawTree()
    {
        _nodes = CreateNodes();

        ImGui.BeginChild("Map Data Tree");
        {
            foreach (var node in _nodes)
                DrawNode(node);
            ImGui.EndChild();
        }
    }

    private void DrawNodeLabel(string icon, string text, bool transparent = false)
    {
        var textColor = ImGui.GetColorU32(ImGuiCol.Text);

        if (!string.IsNullOrEmpty(icon))
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, transparent ? 0x77FFFFFF : 0xFFFFFFFF);
            ImGui.Text(icon);
            ImGui.PopStyleColor();
        }

        ImGui.SameLine();

        if (!string.IsNullOrEmpty(icon))
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 4);

        ImGui.PushStyleColor(ImGuiCol.Text, transparent ? textColor & 0x00FFFFFF | 0x77000000 : textColor);
        ImGui.Text(text);
        ImGui.PopStyleColor();
    }

    private void HandleClick(MapDataTreeNode node, bool leftClick, bool rightClick)
    {
        if (_suspendSelectionClick)
            return;

        if (ImGui.IsItemToggledOpen() && leftClick || leftClick || rightClick)
        {
            if (ImGui.GetIO().KeyShift &&
                _lastSelectedNode != null &&
                _lastSelectedNode.Data.GetType() == node.Data.GetType() &&
                _lastSelectedNode.Parent?.Collection == node.Parent?.Collection)
            {
                int minIndex = Math.Min(_lastSelectedNode.Index, node.Index);
                int maxIndex = Math.Max(_lastSelectedNode.Index, node.Index);

                _indicesToSelect.Clear();

                for (int i = minIndex; i <= maxIndex; i++)
                    _indicesToSelect.Add(i);
            }
            else if (ImGui.GetIO().KeyCtrl || ImGui.GetIO().KeyShift)
            {
                PerformSelection(node);
            }
            else
            {
                PerformSelection(node, true);
            }
        }

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            node.Activate?.Invoke(node);
    }

    private void DrawNode(MapDataTreeNode node)
    {
        var flags = ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.OpenOnArrow |
                    ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth;
        if (node.IsLeaf)
            flags |= ImGuiTreeNodeFlags.Leaf;

        bool selected = _viewModel.Context.SceneObjectHolder.IsSelected(node.Data);
        if (selected)
            flags |= ImGuiTreeNodeFlags.Selected;

        if (_viewModel.ExpandSelection)
        {
            if (node.Children.Any(x =>
                    _viewModel.Context.SceneObjectHolder.IsSelected(x.Data) || x.Children != null &&
                    x.Children.Any(y => _viewModel.Context.SceneObjectHolder.IsSelected(y.Data))))
                ImGui.SetNextItemOpen(true);
            if (selected)
                ImGui.SetScrollHereY();
        }

        bool open = ImGui.TreeNodeEx(node.UniqueId, flags);

        bool itemHovered = ImGui.IsItemHovered();
        bool leftClick = itemHovered && ImGui.IsMouseReleased(ImGuiMouseButton.Left);
        bool rightClick = ImGui.IsItemClicked(ImGuiMouseButton.Right);

        HandleDragDrop(node);

        HandleShiftSelection(node);

        HandleClick(node, leftClick, rightClick);

        DrawNodeLabel(node.Icon, node.Label);

        // Show collection's item count
        if (node.ShowItemCount)
        {
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() - 4);
            ImGui.TextDisabled($"({node.Children.Count})");
        }

        // Show context menu
        DrawContextMenu(node);

        if (open)
        {
            if (!node.Visible)
                ImGui.BeginDisabled();

            // Children below
            foreach (var child in node.Children)
                DrawNode((MapDataTreeNode)child);

            if (!node.Visible)
                ImGui.EndDisabled();

            ImGui.TreePop();
        }
    }

    private void DrawContextMenu(MapDataTreeNode node)
    {
        // Disabled for now

        //if (!node.CanShowContextMenu) return;

        //string contextMenuLabel = $"Context Menu ##{node.Data.GetHashCode()}_ContextMenu";

        //if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        //    ImGui.OpenPopup(contextMenuLabel);

        //if (ImGui.BeginPopupContextItem(contextMenuLabel))
        //{
        //    if (ImGui.Selectable("Add"))
        //    {
        //        PerformAdd();
        //    }

        //    if (ImGui.Selectable("Delete"))
        //    {
        //        PerformDelete();
        //    }

        //    ImGui.EndPopup();
        //}
    }

    private void HandleShiftSelection(MapDataTreeNode node)
    {
        if (_indicesToSelect.Count > 0 && node.Parent?.Collection == _lastSelectedNode?.Parent?.Collection &&
            _indicesToSelect.Contains(node.Index))
        {
            PerformSelection(node, false, false);
            _indicesToSelect.Remove(node.Index);
        }
    }

    private void HandleDrag(MapDataTreeNode node, string nodePayloadType)
    {
        // Drag
        if (node.CanDrag && ImGui.BeginDragDropSource())
        {
            if (!_selectedNodes.Any(x => x.UniqueId == node.UniqueId))
            {
                // It's possible that the drag action starts from an unselected node.
                // In order to make things right, I will fix the selection.
                PerformSelection(node, true, false);
            }

            _sourceDragNode = node;

            ImGui.SetDragDropPayload(nodePayloadType, nint.Zero, 0);
            ImGui.Text(_viewModel.Context.SceneObjectHolder.SelectionSize == 1 ? node.Label
                : $"({_viewModel.Context.SceneObjectHolder.SelectionSize} items)");
            ImGui.EndDragDropSource();
        }
    }

    private void HandleDrop(MapDataTreeNode dropNode, string nodePayloadType)
    {
        if (_sourceDragNode == null || _sourceDragNode.Data is not IMapDataEntry dragEntry || dropNode.Data is not IMapDataEntry dropEntry)
            return;

        _suspendSelectionClick = true;

        // Drop
        if (_viewModel.CanDrop(dragEntry, dropEntry) && ImGui.BeginDragDropTarget())
        {
            bool dropInside = dropNode.Data.GetType() == _sourceDragNode.Data.GetType();

            var style = ImGui.GetStyle();
            var dropTargetOriginalColor = style.Colors[(int)ImGuiCol.DragDropTarget];

            var payload = ImGui.AcceptDragDropPayload(nodePayloadType,
                dropInside ? ImGuiDragDropFlags.AcceptNoDrawDefaultRect : ImGuiDragDropFlags.None);

            if (!payload.Equals(nint.Zero))
            {
                bool moveAfter = false;

                if (dropInside)
                {
                    var pos = ImGui.GetItemRectMin();
                    var mousePos = ImGui.GetMousePos();

                    var rectSize = ImGui.GetItemRectSize();

                    float sizeY = rectSize.Y + 2;
                    float sizeX = rectSize.X + ImGui.GetContentRegionAvail().X;
                    float deltaY = (mousePos - pos).Y;

                    moveAfter = deltaY > sizeY / 2;

                    uint color = (uint)(dropTargetOriginalColor.X * 255) |
                                 (uint)(dropTargetOriginalColor.Y * 255) << 8 |
                                 (uint)(dropTargetOriginalColor.Z * 255) << 16 |
                                 (uint)(dropTargetOriginalColor.W * 255) << 24;

                    //Draw custom line
                    ImGui.GetWindowDrawList().AddLine(
                        new Vector2(pos.X, pos.Y + (moveAfter ? sizeY : -2)),
                        new Vector2(pos.X + sizeX, pos.Y + (moveAfter ? sizeY : -2)), color, 2f);
                }

                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    if (dropInside && dropNode.Data is IMapDataEntry entry)
                        _viewModel.DropSelected(entry, moveAfter);
                    else
                        _viewModel.DropSelected(dropNode.Collection);

                    ClearDragDrop();
                }
            }

            ImGui.EndDragDropTarget();
        }
    }

    private void HandleDragDrop(MapDataTreeNode node)
    {
        string nodePayloadType = $"NKM_NODE";

        HandleDrag(node, nodePayloadType);

        HandleDrop(node, nodePayloadType);
    }

    private void ClearDragDrop()
    {
        _sourceDragNode = null;
    }

    private void PerformSelection(MapDataTreeNode node, bool resetSelection = false, bool allowUnselect = true)
    {
        _lastSelectedNodeId = node.UniqueId;

        if (_viewModel.HandleSelect(node.Data, resetSelection, allowUnselect))
            _lastSelectedNodeId = null;
    }

    private void HandlePaste()
    {
        if (/*!ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) || */_lastSelectedNode == null)
            return;

        if (_lastSelectedNode.Data is IMapDataEntry entry)
            _viewModel.HandlePaste(entry);
        else
            _viewModel.HandlePaste(_lastSelectedNode.Collection);
    }

    private void HandleInsert()
    {
        if (/*!ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) || */_lastSelectedNode == null)
            return;

        if (_lastSelectedNode.Data is IMapDataEntry entry)
            _viewModel.HandleInsert(entry);
        else
            _viewModel.HandleInsert(_lastSelectedNode.Collection);
    }

    private void HandleSelectAll()
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) || !_viewModel.Context.IsActionRequested(CourseEditorRequestableAction.SelectAll))
            return;

        _viewModel.Context.SceneObjectHolder.ClearSelection();
        SelectNodeLeaves(_nodes);

        _viewModel.Context.ClearActionRequest();
    }

    private void SelectNodeLeaves(IEnumerable<MapDataTreeNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.Children.Count > 0)
                SelectNodeLeaves(node.Children.Cast<MapDataTreeNode>());

            if (node.IsLeaf && node.Data is IMapDataEntry)
                PerformSelection(node, false, false);
        }
    }
}