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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

class CollisionImportModalView : ModalView
{
    private enum CollisionImportSteps
    {
        Step1,
        Step2,
        Step3,

        Last = Step3
    }

    private CollisionImportSteps _curStep = CollisionImportSteps.Step1;

    private readonly ICourseEditorContext _context;

    private string _objFilePath = "";
    private Obj _objFile;

    private List<MaterialAttribute> _materialAttributes;
    private bool _selectionChanged = false;

    private ListBoxView _materialListBox;
    private PropertyGridView _propertyGridWidget = new();

    private KclOctreeGenerator.Params _octreeParams = MkdsKcl.DefaultOctreeParams;
    private KclOctree.CompressionMethod _compressionMethod = KclOctree.CompressionMethod.Merge;

    private Task _importTask;
    private LoadingModalView _loadingModal = new("Importing data. Please wait.");

    private CollisionCheatSheetModalView _cheatSheet = new();

    public CollisionImportModalView(ICourseEditorContext context) : base("Import Collision",
        new Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(500)))
    {
        _context = context;
        _propertyGridWidget.RegisterCollisionEditors(_context.Course.MapData);
    }

    protected override void DrawContent()
    {
        bool canContinue = false;
        if (ImGui.BeginChild("main_contents", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())))
        {
            ImGui.Text($"Step {(int)_curStep + 1}/{(int)CollisionImportSteps.Last + 1}");
            ImGui.Separator();
            if (ImGui.BeginChild("step_contents", new Vector2(0, 0)))
            {
                switch (_curStep)
                {
                    case CollisionImportSteps.Step1:
                        canContinue = DrawFileSelect();
                        break;
                    case CollisionImportSteps.Step2:
                        canContinue = DrawMaterialAttributeEditor();
                        break;
                    case CollisionImportSteps.Step3:
                        canContinue = DrawImportSettings();
                        break;
                }

                ImGui.EndChild();
            }

            ImGui.EndChild();
        }

        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 2 * ImGuiEx.CalcUiScaledValue(80) - ImGui.GetStyle().ItemSpacing.X);

        if (_curStep == CollisionImportSteps.Step1)
            ImGui.BeginDisabled();

        if (ImGui.Button("Previous", new Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            if (_curStep != CollisionImportSteps.Step1)
                _curStep--;
        }

        if (_curStep == CollisionImportSteps.Step1)
            ImGui.EndDisabled();

        ImGui.SameLine();

        if (!canContinue)
            ImGui.BeginDisabled();

        if (ImGui.Button(_curStep == CollisionImportSteps.Last ? "Finish" : "Next", new Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            if (_curStep != CollisionImportSteps.Last)
                _curStep++;
            else
                StartImportTask();
        }

        if (_importTask != null && _importTask.IsCompleted)
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
                //var prisms = KclPrismData.GenerateFx32()
                var materialAttributes = new Dictionary<string, ushort>();

                foreach (var m in _materialAttributes.Where(x => x.Enabled))
                    materialAttributes.Add(m.MaterialName, m.Attribute);

                var newCollision = MkdsKclConverter.FromObj(_objFile, materialAttributes, _octreeParams,
                    _compressionMethod);

                _context.ActionStack.Add(new SetMkdsCourseCollisionAction(_context.Course, newCollision));
            }
            catch { }
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

        if (ImGui.BeginChildFrame(ImGui.GetID("DrawPropertyGrid"), avail, ImGuiWindowFlags.NoBackground))
        {
            if (_selectionChanged)
            {
                _propertyGridWidget.Select(_materialAttributes.Where(x => _materialListBox.Selection.Contains(x)));
                _selectionChanged = false;
            }

            _propertyGridWidget.Draw();

            ImGui.EndChildFrame();
        }
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
        ImGui.Combo("Octree compression", ref compressionMethod, new[] { "Equal", "Merge" }, 2);
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

        MkdsCollisionType parseType(string part)
        {
            var types = Enum.GetNames(typeof(MkdsCollisionType)).Select(x => x.ToLower()).ToList();

            //Try exact match
            int typeIndex = types.IndexOf(types.FirstOrDefault(x => x == part));

            if (typeIndex != -1)
            {
                return (MkdsCollisionType)typeIndex;
            }
            else
            {
                //Try loose match
                typeIndex = types.IndexOf(types.FirstOrDefault(x => x.Contains(part)));

                if (typeIndex != -1)
                    return (MkdsCollisionType)typeIndex;
                else if (int.TryParse(part, out typeIndex))
                    return typeIndex < 23 ? (MkdsCollisionType)typeIndex : MkdsCollisionType.Road;
            }

            return MkdsCollisionType.Road;
        }

        MkdsCollisionVariant parseVariant(MkdsCollisionType type, string part)
        {
            var variants = MkdsCollisionConsts.GetVariants(type)
                .Select(x => x.ToLower().Replace(" ", "").Replace("(", "").Replace(")", "")).ToList();

            int variantIndex = variants.IndexOf(variants.FirstOrDefault(x => x.Contains(part)));

            if (variantIndex != -1)
                return (MkdsCollisionVariant)variantIndex;
            else if (int.TryParse(part, out variantIndex))
                return variantIndex < 8 ? (MkdsCollisionVariant)variantIndex : MkdsCollisionVariant.Variant0;

            return MkdsCollisionVariant.Variant0;
        }

        MkdsKclPrismAttribute attribute = 0;

        try
        {
            string[] parts = materialName.ToLower().Split('_');

            bool typeParsed = false;
            bool variantParsed = false;
            bool colorParsed = false;
            bool shadowParsed = false;

            var colorRegex = new Regex("color[0-9]|col[0-9]|c[0-9]");

            foreach (string part in parts)
            {
                if (!colorParsed && colorRegex.IsMatch(part) &&
                    int.TryParse(part[part.Length - 1].ToString(), out int lightId))
                {
                    attribute.LightId = lightId < 4 ? (MkdsCollisionLightId)lightId : MkdsCollisionLightId.Light0;
                    colorParsed = true;
                }
                else if (!shadowParsed && (part == "s" || part == "shd" || part == "shadow"))
                {
                    attribute.Map2dShadow = true;
                    shadowParsed = true;
                }
                else if (!typeParsed)
                {
                    attribute.Type = parseType(part);
                    typeParsed = true;
                }
                else if (!variantParsed)
                {
                    attribute.Variant = parseVariant(attribute.Type, part);
                    variantParsed = true;
                }
            }
        }
        catch { }

        return attribute;
    }

    private bool DrawMaterialAttributeEditor()
    {
        if (_objFile == null)
            return false;

        bool isFirstTime = false;

        if (_materialAttributes == null)
        {
            var materials = _objFile.Faces.Select(x => x.Material).OrderBy(x => x).Distinct();
            _materialAttributes = new();
            foreach (var material in materials)
            {
                _materialAttributes.Add(new MaterialAttribute(material, ParseAttribute(material)));
            }

            isFirstTime = true;

            _materialListBox = new("##MaterialListBox", _materialAttributes);
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
        var result = Nfd.OpenDialog(out string outPath, new Dictionary<string, string> { { "Wavefront OBJ", "obj" } });

        if (result == NfdStatus.Ok)
        {
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
}

public class MaterialAttribute
{
    public MkdsKclPrismAttribute Attribute;
    public string MaterialName;

    public MaterialAttribute(string materialName, MkdsKclPrismAttribute attribute)
    {
        MaterialName = materialName;
        Attribute = attribute;
        Type = attribute.Type; //update the implied flags
    }

    [Category("Settings")]
    public bool Enabled { get; set; } = true;

    [Category("Settings")]
    public MkdsCollisionType Type
    {
        get => Attribute.Type;
        set
        {
            Attribute.Type = value;
            var info = MkdsCollisionConsts.GetColTypeInfo(value);
            Attribute.IsFloor = info.IsFloor;
            Attribute.IsWall = info.IsWall;
            Attribute.IgnoreDrivers = info.IgnoreDrivers;
            Attribute.IgnoreItems = info.IgnoreItems;
        }
    }

    [Category("Settings")]
    public MkdsCollisionVariant Variant
    {
        get => Attribute.Variant;
        set => Attribute.Variant = value;
    }

    [Category("Flags"), DisplayName("Shadow on Bottom Map")]
    public bool Map2dShadow
    {
        get => Attribute.Map2dShadow;
        set => Attribute.Map2dShadow = value;
    }

    [Category("Settings"), DisplayName("Light"), Description("ID of the color inside Stag")]
    public MkdsCollisionLightId LightId
    {
        get => Attribute.LightId;
        set => Attribute.LightId = value;
    }

    public override string ToString()
    {
        string collisionType = Attribute.Type.ToString();
        string disabled = Enabled ? "" : "(X)";

        try
        {
            collisionType += $" - {MkdsCollisionConsts.GetVariantName(Attribute)}";
        }
        catch { }

        return $"{disabled}{MaterialName} ({collisionType})";
    }
}