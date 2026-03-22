using HaroohiePals.Graphics3d;
using HaroohiePals.Gui;
using HaroohiePals.Gui.View;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.KCollision;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.NitroKart.Actions;
using ImGuiNET;
using NativeFileDialogs.Net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class CollisionImportModalView : ModalView
{
    private CollisionImportStep _curStep = CollisionImportStep.SelectFile;

    private readonly ICourseEditorContext _context;
    private readonly KclOctreeGenerator.Params _octreeParams = MkdsKcl.DefaultOctreeParams;
    private readonly PropertyGridView _propertyGridWidget = new();
    private readonly LoadingModalView _loadingModal = new("Importing data. Please wait.");
    private readonly CollisionCheatSheetModalView _cheatSheet = new();

    private string _objFilePath = "";
    private Obj _objFile;

    private List<CollisionImportMaterialAttribute> _materialAttributes;
    private bool _selectionChanged;
    private ListBoxView _materialListBox;
    private KclOctree.CompressionMethod _compressionMethod = KclOctree.CompressionMethod.Merge;
    private Task _importTask;

    public CollisionImportModalView(ICourseEditorContext context) : base("Import Collision",
        new Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(500)))
    {
        _context = context;
        _propertyGridWidget.RegisterCollisionEditors(_context.Course.MapData);
    }

    protected override void DrawContent()
    {
        var lastStep = _curStep;
        bool canContinue = false;
        if (ImGui.BeginChild("main_contents", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())))
        {
            ImGui.Text($"Step {(int)lastStep + 1}/{(int)CollisionImportStep.Last + 1}");
            ImGui.Separator();
            if (ImGui.BeginChild("step_contents", new Vector2(0, 0)))
            {
                canContinue = lastStep switch
                {
                    CollisionImportStep.SelectFile => DrawFileSelect(),
                    CollisionImportStep.AdjustAttributes => DrawMaterialAttributeEditor(),
                    CollisionImportStep.AdjustSettings => DrawImportSettings(),
                    _ => false
                };
            }

            ImGui.EndChild();
        }

        ImGui.EndChild();

        var contentRegionMax = ImGui.GetContentRegionAvail() + ImGui.GetCursorScreenPos() - ImGui.GetWindowPos();
        ImGui.SetCursorPosX(contentRegionMax.X - 2 * ImGuiEx.CalcUiScaledValue(80) - ImGui.GetStyle().ItemSpacing.X);

        if (lastStep == CollisionImportStep.SelectFile)
            ImGui.BeginDisabled();

        if (ImGui.Button("Previous", new Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            if (lastStep != CollisionImportStep.SelectFile)
                _curStep--;
        }

        if (lastStep == CollisionImportStep.SelectFile)
            ImGui.EndDisabled();

        ImGui.SameLine();

        if (!canContinue)
            ImGui.BeginDisabled();

        if (ImGui.Button(lastStep == CollisionImportStep.Last ? "Finish" : "Next",
                new Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            if (lastStep != CollisionImportStep.Last)
                _curStep++;
            else
                StartImportTask();
        }

        if (_importTask is { IsCompleted: true })
            EndImportTask();

        if (!canContinue)
            ImGui.EndDisabled();
    }

    private void StartImportTask()
    {
        _loadingModal.Open();

        _importTask = Task.Factory.StartNew(() =>
        {
            try
            {
                var materialAttributes = _materialAttributes
                    .Where(x => x.Enabled)
                    .ToDictionary<CollisionImportMaterialAttribute, string, ushort>(
                        m => m.MaterialName, m => m.Attribute);

                var newCollision = MkdsKclConverter.FromObj(_objFile, materialAttributes, _octreeParams,
                    _compressionMethod);

                _context.ActionStack.Add(new SetMkdsCourseCollisionAction(_context.Course, newCollision));
            }
            catch
            {
                // ignored
            }
        });
    }

    private void EndImportTask()
    {
        _importTask?.Dispose();
        _importTask = null;
        _loadingModal.Close();
        Close();
    }

    private bool DrawFileSelect()
    {
        string text = "Select an OBJ file to import...";

        ImGui.Text("Some good practices and tips:");
        ImGui.BulletText("Make sure your mesh is triangulated");
        ImGui.BulletText("In order for the game to detect geometry, any vertex must be above Y = 0");
        ImGui.BulletText("Avoid weird and long triangle shapes for better generation");
        ImGui.BulletText("This tool is able to detect attributes based on the materials' name.");
        if (ImGui.Button("Consult the naming cheat sheet"))
        {
            _cheatSheet.Open();
        }

        ImGui.Columns(2, "FileSelect");
        ImGui.Text(text);
        ImGui.NextColumn();
        string romPath = _objFilePath;
        ImGui.InputText("##ObjFilePath", ref romPath, 10000);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##ObjFilePath"))
        {
            SelectObjFilePath();
        }

        ImGui.NextColumn();

        ImGui.Columns(1);

        _cheatSheet.Draw();

        return _objFile != null;
    }

    private void DrawMaterialListBox()
    {
        if (_materialListBox == null)
            return;

        ImGui.Text("Select Materials:");

        _materialListBox.Size = ImGui.GetContentRegionAvail();

        if (_materialListBox.Draw())
        {
            _selectionChanged = true;
        }
    }

    private void DrawPropertyGrid()
    {
        var avail = ImGui.GetContentRegionAvail();

        ImGui.PushStyleColor(ImGuiCol.ChildBg, 0);
        if (ImGui.BeginChild(ImGui.GetID("DrawPropertyGrid"), avail))
        {
            if (_selectionChanged)
            {
                _propertyGridWidget.Select(_materialAttributes.Where(x => _materialListBox.Selection.Contains(x)));
                _selectionChanged = false;
            }

            _propertyGridWidget.Draw();
        }

        ImGui.EndChild();
        ImGui.PopStyleColor();
    }

    private bool DrawImportSettings()
    {
        if (_materialAttributes == null)
            return false;

        ImGui.Text("Import settings:");

        ImGui.Checkbox("Round cube prism test (slower)", ref _octreeParams.UseRoundCubePrismTest);
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(
                "The round cube prism test accurately determines if a prism should be included in an octree node. This can slightly decrease the amount of prisms per leaf and as such slightly improve performance. The test is a lot slower than the normal triangle cube test however.");
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }

        //ImGui.SameLine();

        ImGui.Checkbox("Smart depth compression", ref _octreeParams.UseSmartDepthCompression);
        ImGui.SameLine();
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(
                "Smart depth compression tries to reduce the depth of the octree by checking if splitting a node significantly reduces the number of prisms per leaf. This leads to a significant decrease in file size, but can also decrease performance slightly.");
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }

        //ImGui.SameLine();

        if (!_octreeParams.UseSmartDepthCompression)
            ImGui.BeginDisabled();

        ImGui.PushItemWidth(100);
        ImGui.SliderInt("Smart delta", ref _octreeParams.SmartCompressionDelta, 1, 20);
        ImGui.PopItemWidth();

        if (!_octreeParams.UseSmartDepthCompression)
            ImGui.EndDisabled();

        //ImGui.SameLine();

        ImGui.PushItemWidth(100);
        int compressionMethod = (int)_compressionMethod;
        ImGui.Combo("Octree compression", ref compressionMethod, ["Equal", "Merge"], 2);
        _compressionMethod = (KclOctree.CompressionMethod)compressionMethod;
        ImGui.PopItemWidth();

        _loadingModal.Draw();

        return true;
    }

    private MkdsKclPrismAttribute ParseAttribute(string materialName)
    {
        if (string.IsNullOrEmpty(materialName))
            return 0;

        // Parse raw hex value
        if (int.TryParse(materialName.Split('_')[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture,
                out int parsedHex))
            return (MkdsKclPrismAttribute)parsedHex;

        MkdsKclPrismAttribute attribute = 0;

        try
        {
            string[] parts = materialName.ToLower().Split('_');

            bool typeParsed = false;
            bool variantParsed = false;
            bool colorParsed = false;
            bool shadowParsed = false;
            bool unusedParsed = false;

            var colorRegex = new Regex("color[0-9]|col[0-9]|c[0-9]");

            foreach (string part in parts)
            {
                if (!colorParsed && colorRegex.IsMatch(part) &&
                    int.TryParse(part[^1].ToString(), out int lightId))
                {
                    attribute.LightId = lightId < 4 ? (MkdsCollisionLightId)lightId : MkdsCollisionLightId.Light0;
                    colorParsed = true;
                }
                else if (!shadowParsed && part is "s" or "shd" or "shadow")
                {
                    attribute.Map2dShadow = true;
                    shadowParsed = true;
                }
                else if (!unusedParsed && part is "u" or "uf" or "unused")
                {
                    attribute.UnusedFlag = true;
                    unusedParsed = true;
                }
                else if (!typeParsed)
                {
                    attribute.Type = ParseType(part);
                    typeParsed = true;
                }
                else if (!variantParsed)
                {
                    attribute.Variant = ParseVariant(attribute.Type, part);
                    variantParsed = true;
                }
            }
        }
        catch
        {
            // ignored
        }

        return attribute;
    }

    private MkdsCollisionType ParseType(string part)
    {
        var types = Enum.GetNames(typeof(MkdsCollisionType)).Select(x => x.ToLower()).ToList();

        //Try exact match
        int typeIndex = types.IndexOf(types.FirstOrDefault(x => x == part));

        if (typeIndex != -1)
        {
            return (MkdsCollisionType)typeIndex;
        }

        //Try loose match
        typeIndex = types.IndexOf(types.FirstOrDefault(x => x.Contains(part)));

        if (typeIndex != -1)
            return (MkdsCollisionType)typeIndex;
        if (int.TryParse(part, out typeIndex))
            return typeIndex < 23 ? (MkdsCollisionType)typeIndex : MkdsCollisionType.Road;

        return MkdsCollisionType.Road;
    }

    private MkdsCollisionVariant ParseVariant(MkdsCollisionType type, string part)
    {
        var variants = MkdsCollisionConsts.GetVariants(type)
            .Select(x => x.ToLower().Replace(" ", "").Replace("(", "").Replace(")", "")).ToList();

        int variantIndex = variants.IndexOf(variants.FirstOrDefault(x => x.Contains(part)));

        if (variantIndex != -1)
            return (MkdsCollisionVariant)variantIndex;
        if (int.TryParse(part, out variantIndex))
            return variantIndex < 8 ? (MkdsCollisionVariant)variantIndex : MkdsCollisionVariant.Variant0;

        return MkdsCollisionVariant.Variant0;
    }

    private bool DrawMaterialAttributeEditor()
    {
        if (_objFile == null)
            return false;

        bool isFirstTime = false;

        if (_materialAttributes == null)
        {
            var materials = _objFile.Faces.Select(x => x.Material)
                .OrderBy(x => x).Distinct();
            _materialAttributes = [];
            foreach (string material in materials)
            {
                _materialAttributes.Add(new CollisionImportMaterialAttribute(material, ParseAttribute(material)));
            }

            isFirstTime = true;

            _materialListBox = new ListBoxView("##MaterialListBox", _materialAttributes);
        }

        ImGui.Columns(2, "MaterialList");

        if (isFirstTime)
            ImGui.SetColumnWidth(0, Size.X * 0.5f);
        
        DrawMaterialListBox();

        ImGui.NextColumn();

        DrawPropertyGrid();

        ImGui.NextColumn();
        ImGui.Columns(1);

        return true;
    }

    private void SelectObjFilePath()
    {
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string>
        {
            { "Wavefront OBJ", "obj" }
        });

        if (result != NfdStatus.Ok || string.IsNullOrEmpty(outPath))
            return;

        _objFilePath = outPath;

        try
        {
            _objFile = new Obj(File.ReadAllBytes(_objFilePath));
            _curStep++;
        }
        catch
        {
            _objFile = null;
        }

        _materialAttributes = null;
    }
}