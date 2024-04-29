using HaroohiePals.Gui;
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Toolbar;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;
using Vector2 = System.Numerics.Vector2;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class ArchiveTreeView : IView
{
    private readonly ToolbarView _archiveToolbar = new("ArchiveToolbar");
    private readonly ToolbarItem _archiveImportButton;
    private readonly ToolbarItem _archiveExportButton;
    private readonly ToolbarItem _archiveDeleteButton;
    private readonly HaroohiePals.Gui.View.ArchiveTreeView _archiveTree = new("ArchiveTree", IconConsts.FileExtIcons);

    private readonly ArchiveTreeViewPaneViewModel _viewModel;

    public ArchiveTreeView(ArchiveTreeViewPaneViewModel viewModel)
    {
        _viewModel = viewModel;

        _archiveTree.Archive = _viewModel.Archive;

        _archiveImportButton = new ToolbarItem(FontAwesome6.FileImport[0], "Import", null);
        _archiveToolbar.Items.Add(_archiveImportButton);
        _archiveExportButton = new ToolbarItem(FontAwesome6.FileExport[0], "Export", () =>
        {
            if (_archiveTree.SelectedItem == null || _archiveTree.SelectedItem.IsDirectory)
                return;

            _viewModel.Export(_archiveTree.SelectedItem.Path);
        });
        _archiveToolbar.Items.Add(_archiveExportButton);
        _archiveDeleteButton = new ToolbarItem(FontAwesome6.TrashCan[0], "Delete", null);
        _archiveToolbar.Items.Add(_archiveDeleteButton);
    }

    public bool Draw()
    {
        _archiveImportButton.Enabled = false;
        _archiveExportButton.Enabled = _archiveTree.SelectedItem is { IsDirectory: false };
        _archiveDeleteButton.Enabled = false;
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4));
        if (ImGui.Begin("Archive"))
        {
            ImGui.SetWindowSize(new Vector2(400, 600), ImGuiCond.Once);

            _archiveToolbar.Draw();
            _archiveTree.Draw();

            ImGui.End();
        }

        ImGui.PopStyleVar();

        return true;
    }
}