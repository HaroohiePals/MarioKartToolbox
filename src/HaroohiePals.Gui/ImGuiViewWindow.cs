#nullable enable
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Menu;
using HaroohiePals.Gui.View.Modal;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;

namespace HaroohiePals.Gui;

public class ImGuiViewWindow(ImGuiGameWindowSettings settings, IModalService modalService) : ImGuiGameWindow(settings)
{
    private readonly MenuView _mainMenu = new();
    private IReadOnlyCollection<MenuItem> _mainMenuItems = [];

    /// <summary>
    /// Base menu entries
    /// </summary>
    public IReadOnlyCollection<MenuItem> MainMenuItems
    {
        get => _mainMenuItems;
        set
        {
            _mainMenuItems = value;
            RefreshMenu();
        }
    }

    private WindowContentView? _prevContent;
    public WindowContentView? Content;

    public event Action? LoadFinished;

    private void RefreshMenu()
    {
        _mainMenu.Items.Clear();

        _mainMenu.Items.AddRange(Content is not null ? _mainMenuItems.Merge(Content.MenuItems) : _mainMenuItems);
    }

    protected sealed override void OnLoad()
    {
        base.OnLoad();
        RefreshMenu();
        LoadFinished?.Invoke();
    }

    protected sealed override void RenderLayout(FrameEventArgs args)
    {
        if (_mainMenu.Items.Count > 0)
            _mainMenu.Draw();

        foreach (var modal in modalService.GetAllModals())
            modal.Draw();
        modalService.Cleanup();

        Content?.Draw();
    }

    protected sealed override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (HasContentChanged())
            RefreshMenu();

        Content?.Update(new UpdateArgs(args.Time));
    }

    private bool HasContentChanged()
    {
        bool result = _prevContent != Content;

        _prevContent = Content;
        return result;
    }
}
