using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class ViewportCameraToolInfo
{
    private readonly IApplicationSettingsService _applicationSettings;

    public ViewportCameraToolInfo(IApplicationSettingsService applicationSettings)
    {
        _applicationSettings = applicationSettings;
    }

    public void Draw(ViewportContext context, Gizmo gizmo, bool isTopDown = false)
    {
        if (!ImGui.IsWindowFocused() || !_applicationSettings.Settings.Viewport.ShowToolCameraHint)
            return;

        string info = "Camera | ";

        if (isTopDown)
        {
            info += "Drag Right Click: Move Camera";
        }
        else
        {
            info += "Right Click: Enable Free View";

            if (ImGui.IsMouseDown(ImGuiMouseButton.Right))
            {
                ref readonly var viewportKeyBindings = ref _applicationSettings.Settings.KeyBindings.Viewport;
                info +=
                    $"\nMove Camera | [{viewportKeyBindings.Forward}][{viewportKeyBindings.Left}][{viewportKeyBindings.Backward}][{viewportKeyBindings.Right}]: Forward/Left/Backward/Right | " +
                    $"[{viewportKeyBindings.Up}][{viewportKeyBindings.Down}] Up/Down";
            }
        }

        info += $"\nTool | {gizmo.Tool}";

        switch (gizmo.Tool)
        {
            case GizmoTool.Draw:
                if (gizmo.DrawTool == null)
                {
                    info += " | Select a path or point to draw";
                }
                else
                {
                    info += $" | {gizmo.DrawTool}";

                    switch (gizmo.DrawTool)
                    {
                        case ConnectedPathDrawTool<MkdsEnemyPath, MkdsEnemyPoint>:
                        case ConnectedPathDrawTool<MkdsItemPath, MkdsItemPoint>:
                        case CheckPointPathDrawTool:
                            info += !ImGui.GetIO().KeyShift ? " | [Shift]: Start or connect a split path"
                                : " | Creating or connecting split path";
                            break;
                    }
                }

                break;
            default:
                ref readonly var gizmoKeyBinding = ref _applicationSettings.Settings.KeyBindings.Gizmo;

                if (gizmo.Started && !isTopDown)
                {
                    if (gizmoKeyBinding.SnapToCollision.IsDown())
                        info += $" | Collision snapping active";
                    else
                        info += $" | [{gizmoKeyBinding.SnapToCollision}]: Activate collision snapping";
                }
                break;
        }

        var textHeight = ImGui.CalcTextSize(info).Y;
        var pos = new Vector2(8, ImGui.GetContentRegionMax().Y - textHeight - 8);

        //Outline hack
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFF000000);

        ImGui.SetCursorPos(pos - new Vector2(-1, -1));
        ImGui.Text(info);
        ImGui.SetCursorPos(pos - new Vector2(1, -1));
        ImGui.Text(info);
        ImGui.SetCursorPos(pos - new Vector2(1, 1));
        ImGui.Text(info);
        ImGui.SetCursorPos(pos - new Vector2(-1, 1));
        ImGui.Text(info);
        ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFFFFFF);

        ImGui.SetCursorPos(pos);
        ImGui.Text(info);
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();
    }
}