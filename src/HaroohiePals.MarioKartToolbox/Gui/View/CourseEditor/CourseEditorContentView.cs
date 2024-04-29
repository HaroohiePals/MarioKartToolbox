using HaroohiePals.Gui;
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Menu;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.Gui.View.Toolbar;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData;
using ImGuiNET;
using System;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

sealed class CourseEditorContentView : WindowContentView, IDisposable
{
    private readonly IApplicationSettingsService _applicationSettings;
    private readonly IModalService _modalService;
    private readonly CourseEditorViewModel _viewModel;
    private readonly ICourseEditorViewFactory _subWindowFactory;

    private bool _showDemoWindow = false;
    private bool _debugAreaCheck = false;

    private DockSpaceView _dockSpaceView = new();
    private List<IView> _views = new();
    private PropertyGridPaneView _propertyGridPane;
    private PerspectiveViewportView _perspectiveViewPane;

    private readonly ToolbarView _mainToolbar = new("MainToolbar");
    private readonly ToolbarItem _undoButton;
    private readonly ToolbarItem _redoButton;
    private readonly ToolbarItem _copyButton;
    private readonly ToolbarItem _cutButton;
    private readonly ToolbarItem _pasteButton;
    private readonly ToolbarItem _addButton;
    private readonly ToolbarItem _deleteButton;

    private bool _shouldClose = false;

    public Action CloseCallback;

    public override IReadOnlyCollection<MenuItem> MenuItems =>
    [
        new("File")
        {
            Items = new()
            {
                new("Save", () => { _viewModel?.SaveFile(); }),
                new("Import")
                {
                    Items = new()
                    {
                        new("Import Collision", _viewModel.ShowImportCollision),
                        new("Import Course Model", _viewModel.ShowImportCourseModel),
                        new("Import Skybox Model", _viewModel.ShowImportCourseModelV)
                    }
                },
                new() { Separator = true },
                new("Close Editor", Close)
            }
        },
        new("Edit")
        {
            Items =
                {
                    new("Undo", PerformUndo, _applicationSettings.Settings.KeyBindings.Shortcuts.Undo),
                    new("Redo", PerformRedo, _applicationSettings.Settings.KeyBindings.Shortcuts.Redo),
                    new MenuItem { Separator = true },
                    new("Cut", _viewModel.Cut, _applicationSettings.Settings.KeyBindings.Shortcuts.Cut),
                    new("Copy", _viewModel.Copy, _applicationSettings.Settings.KeyBindings.Shortcuts.Copy),
                    new("Paste", _viewModel.Paste,  _applicationSettings.Settings.KeyBindings.Shortcuts.Paste),
                    new("Insert", _viewModel.Insert, _applicationSettings.Settings.KeyBindings.Shortcuts.Insert),
                    new("Delete", _viewModel.DeleteSelected, _applicationSettings.Settings.KeyBindings.Shortcuts.Delete),
                    new MenuItem { Separator = true },
                    new("Select All", _viewModel.SelectAll, _applicationSettings.Settings.KeyBindings.Shortcuts.SelectAll)
                }
        },
        new("Tools")
        {
            Items =
                {
                    new ("Map Data Generator (Experimental)", _viewModel.ShowMapDataGenerator)
                }
        }
    ];

    public CourseEditorContentView(CourseEditorViewModel viewModel, IApplicationSettingsService applicationSettings,
        IModalService modalService, ICourseEditorViewFactory subWindowFactory)
    {
        _applicationSettings = applicationSettings;
        _modalService = modalService;
        _viewModel = viewModel;
        _subWindowFactory = subWindowFactory;

        LoadCourse();

        _cutButton = new ToolbarItem(FontAwesome6.Scissors[0], "Cut", _viewModel.Cut);
        _copyButton = new ToolbarItem(FontAwesome6.Copy[0], "Copy", _viewModel.Copy);
        _pasteButton = new ToolbarItem(FontAwesome6.Paste[0], "Paste", _viewModel.Paste);
        _addButton = new ToolbarItem(FontAwesome6.CirclePlus[0], "Insert item", _viewModel.Insert);
        _deleteButton = new ToolbarItem(FontAwesome6.TrashCan[0], "Delete selected", _viewModel.DeleteSelected);

        _undoButton = new ToolbarItem(FontAwesome6.ArrowRotateLeft[0], "Undo", PerformUndo);
        _redoButton = new ToolbarItem(FontAwesome6.ArrowRotateRight[0], "Redo", PerformRedo);

        _mainToolbar.Items.Add(_cutButton);
        _mainToolbar.Items.Add(_copyButton);
        _mainToolbar.Items.Add(_pasteButton);
        _mainToolbar.Items.Add(_addButton);
        _mainToolbar.Items.Add(_deleteButton);

        _mainToolbar.Items.Add(_undoButton);
        _mainToolbar.Items.Add(_redoButton);
    }

    public void Close()
    {
        _shouldClose = true;
    }

    private void LoadCourse()
    {
        if (_applicationSettings.Settings.CourseEditor.AutoFixInvalidNkmReferences)
            _viewModel.FixFatalErrors();

        _propertyGridPane = _subWindowFactory.CreatePropertyGridPaneView(_viewModel.Context);
        _views.Add(_propertyGridPane);

        if (_viewModel.Context.Course is MkdsBinaryCourse binCourse)
            _views.Add(_subWindowFactory.CreateArchiveTreeView(binCourse.MainArchive));
        _views.Add(_subWindowFactory.CreateMapDataExplorerView(_viewModel.Context));
        _views.Add(_subWindowFactory.CreateCameraPreviewView(_viewModel.Context));
        _views.Add(_subWindowFactory.CreateTopDownViewportView(_viewModel.Context));
        _perspectiveViewPane = _subWindowFactory.CreatePerspectiveViewportView(_viewModel.Context);
        _views.Add(_perspectiveViewPane);
        _views.Add(_subWindowFactory.CreateTimelinePaneView(_viewModel.Context));
        _views.Add(_subWindowFactory.CreateValidationPaneView(_viewModel.Context));
    }

    private void PerformRedo()
    {
        if (!_viewModel.PerformRedo())
            return;

        _propertyGridPane?.Refresh();
    }

    private void PerformUndo()
    {
        if (!_viewModel.PerformUndo(out bool selectionCleared))
            return;

        if (selectionCleared)
            _propertyGridPane?.Clear();
        else
            _propertyGridPane?.Refresh();
    }

    public override void Update(UpdateArgs args)
    {
        base.Update(args);

        _viewModel.Update(args.Time);

        _views.ForEach(view => view.Update(args));

        if (_shouldClose)
        {
            CloseCallback?.Invoke();
            // todo: Close editor through state machine
            //_modalService.CloseWindow(this);
            //Dispose();
        }
    }

    public override bool Draw()
    {
        _undoButton.Enabled = _viewModel.CanUndo;
        _redoButton.Enabled = _viewModel.CanRedo;

        _addButton.Enabled = _viewModel.CanAdd;
        _deleteButton.Enabled = _viewModel.CanDelete;
        _copyButton.Enabled = _viewModel.CanCopy;
        _cutButton.Enabled = _viewModel.CanCut;

        _mainToolbar.Draw();
        _dockSpaceView.Draw();

        //Test Window
        if (ImGui.Begin("Test Window"))
        {
            if (ImGui.Button("Show/hide demo window"))
                _showDemoWindow = !_showDemoWindow;

            if (ImGui.Button("Toggle check MapObject #0 inside Area #0"))
                _debugAreaCheck = !_debugAreaCheck;

            //Area test
            try
            {
                if (_debugAreaCheck)
                {
                    var area = _viewModel.Context.Course.MapData.Areas[0];
                    var mobj = _viewModel.Context.Course.MapData.MapObjects[0];

                    ImGui.Text(MkdsMapDataUtil.IsPointInsideArea(mobj.Position, area) ? "Inside area" : "Outside area");
                }
            }
            catch
            {
                ImGui.Text("Error during area check");
            }


            //if (ImGui.Button("Ct Wizard"))
            //    _windowManager.ShowModal(new CtWizardWindow(_course));

            if (ImGui.BeginListBox("Action stack"))
            {
                var actions = _viewModel.Context.ActionStack.Peek();

                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    var action = actions[i];

                    if (i > _viewModel.Context.ActionStack.UndoActionsCount - 1)
                        ImGui.BeginDisabled();

                    ImGui.BulletText($"{action}");

                    if (i > _viewModel.Context.ActionStack.UndoActionsCount - 1)
                        ImGui.EndDisabled();
                }

                ImGui.EndListBox();
            }

            ImGui.Text($"Size: {_viewModel.Context.ActionStack.Peek().Count}/{_viewModel.Context.ActionStack.Capacity}");
            ImGui.SameLine();
            ImGui.Text($"Next: {_viewModel.Context.ActionStack.RedoActionsCount}");
            ImGui.SameLine();
            ImGui.Text($"Prev: {_viewModel.Context.ActionStack.UndoActionsCount}");

            if (ImGui.Button("Toggle Edit Mode"))
                _perspectiveViewPane?.ToggleEditMode();

            ImGui.End();
        }

        if (_showDemoWindow)
            ImGui.ShowDemoWindow();

        _views.ForEach(x => x.Draw());

        _viewModel.ClearRequests();

        return true;
    }

    public void Dispose()
    {
        foreach (var view in _views)
            if (view is IDisposable d)
                d.Dispose();
    }
}