using HaroohiePals.Gui.View.Tree;
using HaroohiePals.MarioKart.MapData;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class MapDataTreeNode : TreeNode
{
    public MapDataTreeNode(string uniqueId, string icon, string label, bool isLeaf) : base(label, isLeaf)
    {
        UniqueId = uniqueId;
        Icon = icon;
    }

    public string UniqueId;
    public string Icon;
    public int Index;

    public MapDataTreeNode Parent;

    public bool Visible = true;
    public bool ShowVisibilityButton = true;
    public bool ShowItemCount = false;
    public bool CanShowContextMenu = true;
    public bool CanDrag = false;

    public IMapDataCollection Collection;
}