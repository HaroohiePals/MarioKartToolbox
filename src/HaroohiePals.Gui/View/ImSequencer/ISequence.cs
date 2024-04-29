using ImGuiNET;

namespace HaroohiePals.Gui.View.ImSequencer;

// Ported from https://github.com/CedricGuillemet/ImGuizmo/blob/master/ImSequencer.h

public interface ISequence
{
    bool Focused { get; set; }

    int GetFrameMin() => 0;
    int GetFrameMax() => 0;
    int GetItemCount() => 0;

    void BeginEdit(int index) { }
    void Edit(int index, int start, int end) { }
    void EndEdit() { }
    int GetItemTypeCount() => 0;
    string GetItemTypeName(int typeIndex) => "";
    string GetItemLabel(int index) => "";
    string GetCollapseFmt() => "{0} Frames / {1} entries";

    void Get(int index, out int start, out int end, out int type, out uint color);
    void Add(int type) { }
    void Del(int index) { }
    void Duplicate(int index) { }

    void Copy() { }
    void Paste() { }

    int GetCustomHeight(int index) => 0;
    void DoubleClick(int index) { }
    void CustomDraw(int index, ImDrawListPtr drawList, ImRect rc, ImRect legendRect, ImRect clippingRect, ImRect legendClippingRect) { }
    void CustomDrawCompact(int index, ImDrawListPtr drawList, ImRect rc, ImRect clippingRect) { }
}