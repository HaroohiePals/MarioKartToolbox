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
    private readonly IModalService _modalService = modalService;
    public ImGuiViewWindow(IModalService modalService) : this(ImGuiGameWindowSettings.Default, modalService) { }

    private MenuView _mainMenu = new();
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

        if (Content is not null)
            _mainMenu.Items.AddRange(_mainMenuItems.Merge(Content.MenuItems));
        else
            _mainMenu.Items.AddRange(_mainMenuItems);
    }

    protected sealed override void OnLoad()
    {
        base.OnLoad();
        RefreshMenu();
        LoadFinished?.Invoke();
    }

    protected override sealed void RenderLayout(FrameEventArgs args)
    {
        if (_mainMenu.Items.Count > 0)
            _mainMenu.Draw();

        foreach (var modal in _modalService.GetAllModals())
            modal.Draw();

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
        bool result = false;

        if (_prevContent != Content)
            result = true;

        _prevContent = Content;
        return result;
    }
}
