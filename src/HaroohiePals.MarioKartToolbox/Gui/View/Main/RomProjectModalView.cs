using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.Nitro.Card;
using HaroohiePals.NitroKart.Rom;
using ImGuiNET;
using NativeFileDialogs.Net;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

public class RomProjectModalView() : ModalView(WINDOW_TITLE, WindowSize)
{
    private const string WINDOW_TITLE = "Create a new Nitro Rom Project";
    private static readonly Vector2 WindowSize = new(400, 0);
    
    private readonly LoadingModalView _loadingModal = new("Extracting ROM... Please wait.");
    private Task _createProjectTask;

    private string _romFilePath = "";
    private string _outputProjectFilePath = "";
    private bool _unpackArc = true;
    private string _errorMessage = "";
    private bool _createResult;
    
    protected override void DrawContent()
    {
        ImGui.BeginTable("##RomProjectModalTable", 2, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Input NDS ROM File");
        ImGui.TableNextColumn();
        ImGui.InputText("##RomFilePath", ref _romFilePath, 10000);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##RomFilePath"))
            SelectInputRomFilePath();

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.TextUnformatted("Output Project File");
        ImGui.TableNextColumn();
        ImGui.InputText("##OutputPath", ref _outputProjectFilePath, 10000);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##OutputPath"))
            SelectOutputProjectFilePath();

        ImGui.EndTable();

        bool isActionDisabled = string.IsNullOrEmpty(_romFilePath) || string.IsNullOrEmpty(_outputProjectFilePath);

        if (isActionDisabled)
            ImGui.BeginDisabled();
        if (ImGui.Button("Create Project"))
            PerformCreateProject();
        if (isActionDisabled)
            ImGui.EndDisabled();

        if (!_createResult && !string.IsNullOrEmpty(_errorMessage))
        {
            var errorColor = ImGuiEx.ColorConvertColorToFloat4(Color.Red);
            ImGui.SameLine();
            ImGui.TextColored(errorColor, _errorMessage);
        }
        
        if (_createProjectTask is { IsCompleted: true } && _createResult)
            EndCreateProjectTask();

        _loadingModal.Draw();
    }

    private void SelectInputRomFilePath()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
        {
            { "Nintendo DS ROM", "nds,srl" }
        });

        if (result == NfdStatus.Ok)
            _romFilePath = outPath;

        _errorMessage = "";
    }

    private void SelectOutputProjectFilePath()
    {
        var result = Nfd.SaveDialog(out string outPath, new Dictionary<string, string>
        {
            { "Nitro ROM Project", "json" }
        }, "RomProject.json");

        if (result == NfdStatus.Ok)
            _outputProjectFilePath = outPath;
        
        _errorMessage = "";
    }

    private void PerformCreateProject()
    {
        _loadingModal.Open();
        _createProjectTask = Task.Factory.StartNew(CreateProjectAsync);
    }
    
    private void EndCreateProjectTask()
    {
        _createProjectTask?.Dispose();
        _createProjectTask = null;
        _loadingModal.Close();
        Close();
    }
    
    private async Task CreateProjectAsync()
    {
        if (!File.Exists(_romFilePath))
        {
            _errorMessage = "The input file does not exist.";
            _createResult = false;
            _loadingModal.Close();
            return;
        }
        
        var outputFileInfo = new FileInfo(_outputProjectFilePath);
        string outputPath = outputFileInfo.DirectoryName; 

        if (Directory.Exists(outputPath) && Directory.EnumerateFiles(outputPath).Any())
        {
            _errorMessage = "The output folder is not empty.";
            _createResult = false;
            _loadingModal.Close();
            return;
        }

        string projectName = outputFileInfo.Name.Replace(outputFileInfo.Extension, "");
        
        _loadingModal.Open();

        try
        {
            byte[] romData = await File.ReadAllBytesAsync(_romFilePath);
            var rom = new NdsRom(romData);
            var projectFactory = new MkdsRomProjectFactory();
            await projectFactory.CreateAsync(rom, projectName, outputPath, _unpackArc);
        }
        catch
        {
            _errorMessage = "An unexpected error has occured.";
            _createResult = false;
            _loadingModal.Close();
            return;
        }
        
        _createResult = true;
    }
}