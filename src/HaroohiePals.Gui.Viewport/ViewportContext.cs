using HaroohiePals.Actions;
using ImGuiNET;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public class ViewportContext
{
    public const uint InvalidPickingId = 0xFF000000;

    public bool ForceCustomProjectionMatrix = false;
    public Matrix4 CustomProjectionMatrix { get; set; } = Matrix4.Identity;

    public Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;
    public Matrix4 ViewMatrix       { get; set; } = Matrix4.Identity;

    public Vector2i ViewportSize { get; set; }

    public bool TranslucentPass { get; set; }

    public ActionStack       ActionStack       { get; set; }
    public SceneObjectHolder SceneObjectHolder { get; set; } = new();
    public SelectionHandle   HoverObject       { get; set; }

    public PickingResult PickingResult { get; set; }

    private readonly Dictionary<PickingResult, uint> _pickingResultPickingIdMap = new();
    private readonly Dictionary<uint, PickingResult> _pickingIdPickingResultMap = new();
    private uint _lastPickingId = InvalidPickingId + 1;

    private string _message;
    private double _messageExpiry;

    public void ShowMessage(string message, double seconds = 2.5)
    {
        _message = message;
        _messageExpiry = ImGui.GetTime() + seconds;
    }

    public bool TryGetMessage(out string message)
    {
        message = !string.IsNullOrEmpty(_message) && ImGui.GetTime() < _messageExpiry ? _message : null;
        return message != null;
    }

    public bool IsSelected(object obj, int subIndex = -1)
        => SceneObjectHolder.IsSubIndexSelected(obj, subIndex);

    public bool IsHovered(object obj, int subIndex = -1)
        => HoverObject != null && HoverObject.Object == obj && (subIndex == -1 || HoverObject.SubIndex == subIndex);

    public uint GetPickingId(int groupId, int index, int subIndex = -1)
    {
        if (_lastPickingId < InvalidPickingId)
            return InvalidPickingId;

        var pickingResult = new PickingResult(groupId, index, subIndex);
        if (_pickingResultPickingIdMap.ContainsKey(pickingResult))
        {
            return _pickingResultPickingIdMap[pickingResult];
        }
        _pickingResultPickingIdMap.Add(pickingResult, _lastPickingId);
        _pickingIdPickingResultMap.Add(_lastPickingId, pickingResult);
        return _lastPickingId++;
    }

    public PickingResult GetPickingResult(uint pickingId)
    {
        if (_pickingIdPickingResultMap.ContainsKey(pickingId))
        {
            return _pickingIdPickingResultMap[pickingId];
        }
        return PickingResult.Invalid;
    }
}