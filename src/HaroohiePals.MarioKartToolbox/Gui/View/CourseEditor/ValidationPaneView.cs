using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Pane;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.Validation;
using ImGuiNET;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class ValidationPaneView : PaneView
{
    private readonly ValidationPaneViewModel _viewModel;

    public ValidationPaneView(ValidationPaneViewModel viewModel) : base("Error List")
    {
        _viewModel = viewModel;
    }

    public override void Update(UpdateArgs args)
    {
        _viewModel.Update();
    }

    private void DrawToolbar()
    {
        //int      current = 0;
        //string[] items   = new[] { "Map Data" };
        //ImGui.SetNextItemWidth(100);
        //ImGui.Combo("##ErrorListEntity", ref current, items, items.Length);

        //ImGui.SameLine();
        if (ImGui.Button("Fix All"))
            _viewModel.FixAllErrors();

        //ImGui.SameLine();
        //if (ImGui.Button("Check"))
        //    _viewModel.UpdateErrors();

        var selectedColor = ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive];

        ImGui.SameLine();
        bool selected = _viewModel.ShowFatal;
        if (selected)
            ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
        if (ImGui.Button($"{_viewModel.Errors.Count(x => x.Level == ErrorLevel.Fatal)} Fatal Errors"))
            _viewModel.ShowFatal = !_viewModel.ShowFatal;
        if (selected)
            ImGui.PopStyleColor();

        ImGui.SameLine();
        selected = _viewModel.ShowErrors;
        if (selected)
            ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
        if (ImGui.Button($"{_viewModel.Errors.Count(x => x.Level == ErrorLevel.Error)} Errors"))
            _viewModel.ShowErrors = !_viewModel.ShowErrors;
        if (selected)
            ImGui.PopStyleColor();

        ImGui.SameLine();
        selected = _viewModel.ShowWarning;
        if (selected)
            ImGui.PushStyleColor(ImGuiCol.Button, selectedColor);
        if (ImGui.Button($"{_viewModel.Errors.Count(x => x.Level == ErrorLevel.Warning)} Warnings"))
            _viewModel.ShowWarning = !_viewModel.ShowWarning;
        if (selected)
            ImGui.PopStyleColor();
    }

    private void DrawTable()
    {
        var avail = ImGui.GetContentRegionAvail();

        if (ImGui.BeginChildFrame(ImGui.GetID("##ErrorListTableContainer"), avail))
        {
            ImGuiTableFlags tableFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.SizingFixedFit;

            if (ImGui.BeginTable("ErrorListTable", 5, tableFlags))
            {
                ImGuiTableColumnFlags columnFlags = ImGuiTableColumnFlags.NoResize;

                ImGui.TableSetupColumn("#", columnFlags);
                ImGui.TableSetupColumn("Type", columnFlags);
                ImGui.TableSetupColumn("Message", columnFlags);
                ImGui.TableSetupColumn("Level", columnFlags);
                ImGui.TableSetupColumn("Where", columnFlags);
                ImGui.TableHeadersRow();

                foreach (var error in _viewModel.Errors)
                {
                    if (error.Level == ErrorLevel.Fatal && !_viewModel.ShowFatal ||
                        error.Level == ErrorLevel.Error && !_viewModel.ShowErrors ||
                        error.Level == ErrorLevel.Warning && !_viewModel.ShowWarning)
                        continue;

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    if (error.IsFixable)
                    {
                        if (ImGui.Button($"Fix##{error.GetHashCode()}"))
                            _viewModel.FixSingleError(error);
                    }

                    ImGui.TableSetColumnIndex(1);
                    ImGui.Text(error.Rule.Name);

                    ImGui.TableSetColumnIndex(2);
                    ImGui.Text(error.Message);

                    ImGui.TableSetColumnIndex(3);
                    ImGui.Text(error.Level.ToString());

                    ImGui.TableSetColumnIndex(4);
                    if (error.Source is not null && error.Source is IMapDataEntry entry && ImGui.Button($"{entry.GetDisplayName()}##{error.GetHashCode()}"))
                        _viewModel.SelectMapDataEntry(entry);
                }

                ImGui.EndTable();
            }

            ImGui.EndChildFrame();
        }
    }

    public override void DrawContent()
    {
        if (_viewModel.Errors is null)
            _viewModel.UpdateErrors();

        DrawToolbar();

        DrawTable();
    }
}