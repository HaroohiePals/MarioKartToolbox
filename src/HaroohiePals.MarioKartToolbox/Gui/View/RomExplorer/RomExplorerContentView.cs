#nullable enable
using System;
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Menu;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;
using HaroohiePals.IO.Archive;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.RomExplorer;

namespace HaroohiePals.MarioKartToolbox.Gui.View.RomExplorer;

class RomExplorerContentView(RomExplorerViewModel viewModel): WindowContentView
{
    public Action? CloseCallback;

    private ArchiveTreeView? _tree;

    public override IReadOnlyCollection<MenuItem> MenuItems
    {
        get
        {
            var fileMenu = new MenuItem("File")
            {
                Items =
                [
                    new() { Separator = true },
                    new("Close ROM", CloseCallback)
                ]
            };

            if (viewModel.IsProjectLoaded())
            {
                fileMenu.Items.Insert(0, new("Save As...")
                {
                    Items =
                    [
                        new("Nintendo DS ROM", viewModel.BuildRom)
                    ]
                });
            }

            return
            [
                fileMenu
            ];
        }
    }

    public override void Update(UpdateArgs args)
    {
        if (_tree is not null)
            return;
        
        _tree = new("RomTree", IconConsts.FileExtIcons)
        {
            Archive = viewModel.RomArchive
        };
        _tree.Activate += (_, path, _) => viewModel.ActivateItem(path);
    }

    public override bool Draw()
    {
        if (ImGui.Begin($"Rom Explorer ({viewModel.GetTitle()})"))
        {
            ImGui.SetWindowSize(new Vector2(600, 800), ImGuiCond.Once);

            if (ImGui.BeginTabBar("##Tabs_RomExplorer"))
            {
                if (ImGui.BeginTabItem("File System"))
                {
                    _tree?.Draw();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        ImGui.End();

        return true;
    }

    public void SetFileActivationCallbacks(Action<string, Archive>? onCarcOpen = null, Action<string>? onNkmOpen = null)
    {
        viewModel.OnCarcOpen = onCarcOpen;
        viewModel.OnNkmOpen = onNkmOpen;
    }
}