using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.Gui.View.Modal;

public abstract class ModalView : IView
{
    private const bool DEFAULT_SHOW_CLOSE_BUTTON = true;

    private static readonly Vector2 DefaultSize = new Vector2(600, 400);

    private bool _shouldOpen;
    private bool _shouldClose;

    public string Title { get; }
    public Vector2 Size { get; protected set; }
    public bool ShowCloseButton { get; protected set; }
    public bool IsOpen { get; private set; }
    public bool IsOpening => _shouldOpen;

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
        _shouldOpen = true;
    }

    public void Close()
    {
        _shouldClose = true;
    }

    public bool Draw()
    {
        if (_shouldOpen)
        {
            OnOpen();
            ImGui.OpenPopup(Title);
            _shouldOpen = false;
        }

        bool open = true;

        if (ShowCloseButton && ImGui.BeginPopupModal(Title, ref open, ImGuiWindowFlags.NoSavedSettings) || ImGui.BeginPopupModal(Title))
        {
            IsOpen = true;

            ImGui.SetWindowSize(Size, ImGuiCond.Once);

            DrawContent();

            if (_shouldClose)
            {
                ImGui.CloseCurrentPopup();
                _shouldClose = false;
                open = false;
            }

            ImGui.EndPopup();
        }

        if (IsOpen && !open)
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