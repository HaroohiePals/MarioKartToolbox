#nullable enable

using System;
using System.Numerics;
using ImGuiNET;

namespace HaroohiePals.Gui.View.Modal;

public class MessageModalView(
    string title, string message,
    MessageModalButtonsType buttonsType = MessageModalButtonsType.Ok,
    Action<bool>? closeCallback = null) 
    : ModalView(title, Vector2.Zero)
{
    private bool _closeCallbackValue;
    
    protected override void DrawContent()
    {
        ImGui.TextUnformatted(message);

        switch (buttonsType)
        {
            case MessageModalButtonsType.None:
                ShowCloseButton = true;
                _closeCallbackValue = false;
                break;
            case MessageModalButtonsType.Ok:
                ShowCloseButton = false;
                if (DrawAlignedButton("Ok", 0.5f))
                    OnConfirmButtonClick();
                break;
            case MessageModalButtonsType.YesNo:
                ShowCloseButton = false;
                if (DrawAlignedButton("Yes", 0.0f))
                    OnConfirmButtonClick();
                if (DrawAlignedButton("No", 1.0f))
                    OnCancelButtonClick();
                break;
        }
    }

    protected override void OnClose()
    {
        base.OnClose();
        closeCallback?.Invoke(_closeCallbackValue);
    }

    private bool DrawAlignedButton(string label, float alignment)
    {
        var style = ImGui.GetStyle();

        float size = ImGui.CalcTextSize(label).X + style.FramePadding.X * 2.0f;
        float avail = ImGui.GetContentRegionAvail().X;

        float off = (avail - size) * alignment;
        if (off > 0.0f)
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + off);

        return ImGui.Button(label);
    }

    private void OnConfirmButtonClick()
    {
        _closeCallbackValue = true;
        Close();
    }
    
    private void OnCancelButtonClick()
    {
        _closeCallbackValue = false;
        Close();
    }
}