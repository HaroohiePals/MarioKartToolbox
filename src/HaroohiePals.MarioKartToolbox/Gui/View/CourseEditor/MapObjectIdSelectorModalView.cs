using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.Gui.View.Tree;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapObj;
using ImGuiNET;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class MapObjectIdSelectorModalView : ModalView
{
    public bool Confirm = false;
    public MkdsMapObjectId MapObjectId = 0;

    private readonly IMkdsMapObjDatabase _mobjDatabase;

    private string[] _categories = new[] { "Ambient", "Common", "Obstacles", "Scenery", "Enemies", "Boss" };
    private TreeView _tree;
    private string _filter = "";

    public MapObjectIdSelectorModalView(IMkdsMapObjDatabase database)
        : base("Select a Map Object ID", new Vector2(ImGuiEx.CalcUiScaledValue(400)))
    {
        _mobjDatabase = database;
        Reset();
    }

    private string FormatMapObjectIdTreeNode(MkdsMapObjInfo mobjInfo)
        => $"{(ushort)mobjInfo.Id}: {mobjInfo.Name}";

    private void InitTree()
    {
        _tree = new("MapObjectIdSelectorTree");
        _tree.SelectionChanged += SelectionChanged;

        int categoryId = 0;

        foreach (var category in _categories)
        {
            var node = new TreeNode($"{FontAwesome6.Folder} {category}", false);

            var mapObjectInfos = _mobjDatabase.GetAll().Where(x => (ushort)x.Id / 100 == categoryId);

            if (!string.IsNullOrEmpty(_filter))
            {
                node.DefaultOpen = true;
                mapObjectInfos = mapObjectInfos.Where(x => x.Name.ToString().ToLower().Contains(_filter.ToLower()));
            }

            if (mapObjectInfos.Count() > 0)
            {
                node.Children.AddRange(mapObjectInfos.Select(x => new TreeNode(FormatMapObjectIdTreeNode(x), true)
                {
                    Data = x.Id,
                    Activate = ActivateNode
                }));

                var matchNode = node.Children.FirstOrDefault(x => (MkdsMapObjectId)x.Data == MapObjectId);
                if (matchNode != null)
                {
                    _tree.SelectedNode = matchNode;
                    node.DefaultOpen = true;
                }

                _tree.Nodes.Add(node);
            }

            categoryId++;
        }
    }

    private void ActivateNode(TreeNode node)
    {
        Confirm = true;
        MapObjectId = (MkdsMapObjectId)node.Data;
        Close();
    }

    private void SelectionChanged(TreeView obj)
    {
        if (obj.SelectedNode is not null && obj.SelectedNode.Data is not null && obj.SelectedNode.Data is MkdsMapObjectId mapObjectId)
            MapObjectId = mapObjectId;
    }

    protected override void DrawContent()
    {
        int integerMapObjectId = (ushort)MapObjectId;

        ImGui.Text("Filter: ");
        ImGui.SameLine();
        ImGui.PushItemWidth(ImGuiEx.CalcUiScaledValue(100));
        if (ImGui.InputText("##FilterMobjId", ref _filter, 20))
            InitTree();
        ImGui.PopItemWidth();

        ImGui.SameLine();

        ImGui.PushItemWidth(ImGuiEx.CalcUiScaledValue(100));
        if (ImGui.DragInt("##SelectedMobjId", ref integerMapObjectId, 1f, 0, 600))
        {
            MapObjectId = (MkdsMapObjectId)integerMapObjectId;
            InitTree();
        }
        ImGui.PopItemWidth();

        ImGui.SameLine();

        var mobjInfo = _mobjDatabase.GetById(MapObjectId);

        ImGui.Text(mobjInfo is not null ? mobjInfo.Name : "Unused");

        var avail = ImGui.GetContentRegionAvail();
        ImGui.BeginChild("MobjIdSelectTreeChild", new(avail.X, avail.Y - ImGuiEx.CalcUiScaledValue(25)));
        {
            _tree.Draw();
            ImGui.EndChild();
        }

        if (ImGui.Button("Confirm##ConfirmMobjIdSelection"))
        {
            Confirm = true;
            Close();
        }
    }

    public void Reset()
    {
        InitTree();
        Confirm = false;
    }
}
