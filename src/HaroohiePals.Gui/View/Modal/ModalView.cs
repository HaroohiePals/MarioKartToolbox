using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.Gui.View.Modal;

public abstract class ModalView : IView
{
    private const bool DEFAULT_SHOW_CLOSE_BUTTON = true;

    private static readonly Vector2 DefaultSize = new Vector2(600, 400);

    private bool _open;
    private bool _close;

    public string Title { get; }
    public Vector2 Size { get; protected set; }
    public bool ShowCloseButton { get; protected set; }
    public bool IsOpen { get; private set; }

    public ModalView(string title) 
        : this(title, DefaultSize) { }

    public ModalView(string title, Vector2 size) 
        : this(title, size, DEFAULT_SHOW_CLOSE_BUTTON) { }

    public ModalView(string title, Vector2 size, bool showCloseButton)
    {
        Title = $"{title}##{GetHashCode()}";
        Size = size;
        ShowCloseButton = showCloseButton;
    }

    public void Open()
    {
        _open = true;
    }

    public void Close()
    {
        _close = true;
    }

    public bool Draw()
    {
        if (_open)
        {
            OnOpen();
            ImGui.OpenPopup(Title);
            _open = false;
        }

        bool open = true;

        if (ShowCloseButton && ImGui.BeginPopupModal(Title, ref open) || ImGui.BeginPopupModal(Title))
        {
            IsOpen = true;

            ImGui.SetWindowSize(Size, ImGuiCond.Once);

            DrawContent();

            if (_close)
            {
                ImGui.CloseCurrentPopup();
                _close = false;
                open = false;
            }

            ImGui.EndPopup();
        }

        if (!open)
        {
            IsOpen = false;
            OnClose();
        }

        return true;
    }

    protected abstract void DrawContent();

    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }
}