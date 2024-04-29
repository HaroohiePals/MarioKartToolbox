using HaroohiePals.Gui;
using HaroohiePals.Gui.Input;
using HaroohiePals.Gui.Themes;
using HaroohiePals.Gui.View.Modal;
using HaroohiePals.MarioKartToolbox.Application.Settings;
using ImGuiNET;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

internal class PreferencesModalView : ModalView
{
    internal record PreferencesTab(string Label, Action Draw);

    private readonly IApplicationSettingsService _applicationSettings;

    private PreferencesTab[] _tabs;

    private ApplicationSettings _prefs;
    private bool _isDirty = false;
    private bool _hasSaved = false;

    private bool _showStyleEditor = false;

    public Action OnCloseAfterSaved;

    public PreferencesModalView(IApplicationSettingsService applicationSettings) : base("Preferences",
        new Vector2(ImGuiEx.CalcUiScaledValue(600), ImGuiEx.CalcUiScaledValue(400)))
    {
        _applicationSettings = applicationSettings;
        _tabs = new PreferencesTab[]
        {
            new("General", DrawGeneral),
            new("Viewport", DrawViewport),
            new("Key Bindings", DrawKeyBindings),
        };
    }

    protected override void OnOpen()
    {
        _prefs = _applicationSettings.Settings;
        _keyBindingCurrentLabel = null;
        _hasSaved = false;
    }

    protected override void OnClose()
    {
        if (_hasSaved)
            OnCloseAfterSaved?.Invoke();
    }

    protected override void DrawContent()
    {
        if (_showStyleEditor)
        {
            if (ImGui.Begin("Theme Editor"))
            {
                ImGui.ShowStyleEditor();

                ImGui.End();
            }
        }

        PreferencesTab selectedTab = null;

        if (ImGui.BeginTabBar("##Tabs_Preferences"))
        {
            foreach (var tab in _tabs)
            {
                if (ImGui.BeginTabItem(tab.Label))
                {
                    selectedTab = tab;
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        var region = ImGui.GetContentRegionAvail();

        if (ImGui.BeginChild("##TabContent_Preferences", new Vector2(region.X, region.Y - ImGuiEx.CalcUiScaledValue(30))))
        {
            selectedTab?.Draw();

            ImGui.EndChild();
        }

        ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - 1 * ImGuiEx.CalcUiScaledValue(80) - ImGui.GetStyle().ItemSpacing.X);

        if (!_isDirty)
            ImGui.BeginDisabled();

        if (ImGui.Button("Apply", new Vector2(ImGuiEx.CalcUiScaledValue(80), 0)))
        {
            Console.WriteLine("Saving preferences");
            _applicationSettings.Set((ref ApplicationSettings settings) => settings = _prefs);
            _prefs = _applicationSettings.Settings;
            _hasSaved = true;

            ImGuiThemeManager.Apply(_prefs.Appearance.Theme);

            _isDirty = false;
        }

        if (!_isDirty)
            ImGui.EndDisabled();
    }

    private void DrawGeneral()
    {
        if (ImGui.CollapsingHeader("Course Editor##General_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2, "##Columns_CourseEditor_General_Preferences");

            //Autofix Invalid Map Data References
            {
                ImGui.Text("Autofix Invalid Map Data References");
                ImGui.SameLine();
                ImGui.TextDisabled("(?)");
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                    ImGui.TextUnformatted(
                        "Upon opening the course editor, a fix to all fatal errors that would prevent saving\n" +
                        "is applied.\n" +
                        "These errors should only show up on files created with older NKM editors.\n" +
                        "There are a few instances of official mission NKM files containing invalid references,\n" +
                        "probably due to leftover data.\n" +
                        "Mario Kart Toolbox performs a high level abstraction of all references, for consistency\n" +
                        "sake those invalid references are kept as pure indices, however when saving all references\n" +
                        "must be valid to prevent issues.\n" +
                        "Disable this flag only for documentation purposes or if you know what you are doing.");
                    ImGui.PopTextWrapPos();
                    ImGui.EndTooltip();
                }
                ImGui.NextColumn();
                if (ImGui.Checkbox("##AutofixInvalidNkmRef", ref _prefs.CourseEditor.AutoFixInvalidNkmReferences))
                {
                    _isDirty = true;
                }
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        if (ImGui.CollapsingHeader("Appearance##General_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2, "##Columns_Appearance_General_Preferences");

            //Themes
            {
                string[] themes = ImGuiThemeManager.Themes.Select(x => x.Name).ToArray();

                ImGui.Text("Theme");

                ImGui.NextColumn();
                int index = Array.IndexOf(themes, _prefs.Appearance.Theme);
                ImGui.PushItemWidth(ImGuiEx.CalcUiScaledValue(200));
                if (ImGui.Combo("##Theme", ref index, themes, themes.Length))
                {
                    _prefs.Appearance.Theme = themes[index];
                    _isDirty = true;
                }

                ImGui.PopItemWidth();

                ImGui.SameLine();

                if (ImGui.Button($"{FontAwesome6.Pencil}##Edit Style"))
                {
                    _showStyleEditor = !_showStyleEditor;
                }

                ImGui.SameLine();

                if (ImGui.Button($"{FontAwesome6.FloppyDisk}##Save Custom.json"))
                {
                    _isDirty = true;
                    _prefs.Appearance.Theme = "Custom";
                    string json = ImGuiTheme.Create("Custom").ToJson();
                    File.WriteAllText($"Themes/Custom.json", json);
                    ImGuiThemeManager.Init();
                }
                ImGui.NextColumn();
            }

            //Ui Scale
            {
                ImGui.Text("UI Scale (Must restart)");

                ImGui.NextColumn();

                if (ImGui.DragFloat("##UiScale", ref _prefs.Appearance.UiScale, 0.01f, 1f, 3f))
                    _isDirty = true;

                ImGui.NextColumn();
            }

            ImGui.Columns(1);
        }

        if (ImGui.CollapsingHeader("Discord##General_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Enable Discord Rich Presence", ref _prefs.Discord.EnableRichPresence);
            _isDirty = !_isDirty ? ImGui.IsItemDeactivatedAfterEdit() : _isDirty;

            if (!_prefs.Discord.EnableRichPresence)
            {
                ImGui.BeginDisabled();
            }

            ImGui.Checkbox("Display Game Name", ref _prefs.Discord.ShowRomTitle);
            _isDirty = !_isDirty ? ImGui.IsItemDeactivatedAfterEdit() : _isDirty;
            ImGui.Checkbox("Display Course Name", ref _prefs.Discord.ShowCourseName);
            _isDirty = !_isDirty ? ImGui.IsItemDeactivatedAfterEdit() : _isDirty;

            if (!_prefs.Discord.EnableRichPresence)
            {
                ImGui.EndDisabled();
            }
        }

        if (ImGui.CollapsingHeader("Game##General_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2, "##Columns_Game_General_Preferences");

            ImGui.Text("Mario Kart DS Language");

            ImGui.NextColumn();

            string[] languages = new[] { "English", "Japanese", "Italian", "Spanish", "French", "German" };

            int index = Array.IndexOf(languages, _prefs.Game.Language);
            ImGui.PushItemWidth(200);
            if (ImGui.Combo("##Mario Kart DS Language", ref index, languages, languages.Length))
            {
                _prefs.Game.Language = languages[index];
                _isDirty = true;
            }

            ImGui.PopItemWidth();

            ImGui.NextColumn();

            ImGui.Columns(1);
        }
    }

    private void DrawViewport()
    {
        void drawColorEdit(string label, ref Color color)
        {
            var vec = new Vector3(color.R / 255f, color.G / 255f, color.B / 255f);

            if (ImGui.ColorEdit3(label, ref vec))
            {
                color = Color.FromArgb((int)(vec.X * 255), (int)(vec.Y * 255), (int)(vec.Z * 255));
                _isDirty = true;
            }
        }

        //if (ImGui.CollapsingHeader("Camera Control##Viewport_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        //{
        //    ImGui.Columns(2, "##Columns_Camera Control##Viewport_Preferences");

        //    ImGui.Text("Mode");
        //    ImGui.NextColumn();

        //    var mode = (object)_prefs.Viewport.CameraControl.Mode;
        //    ImGui.PushItemWidth(200);
        //    if (ImGuiEx.ComboEnum("##CameraMode", ref mode))
        //    {
        //        _prefs.Viewport.CameraControl.Mode = (Preferences.ViewportPrefs.CameraControlPrefs.CameraMode)mode;
        //        _isDirty = true;
        //    }
        //    ImGui.PopItemWidth();

        //    ImGui.NextColumn();

        //    ImGui.Columns(1);
        //}

        if (ImGui.CollapsingHeader("Various##Viewport_Prefs_Various", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (ImGui.Checkbox("Show Camera and Tool Controls Hints", ref _prefs.Viewport.ShowToolCameraHint))
                _isDirty = true;
        }

        if (ImGui.CollapsingHeader("Colors##Viewport_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            drawColorEdit("Map Objects", ref _prefs.Viewport.Colors.MapObjects);
            drawColorEdit("Paths", ref _prefs.Viewport.Colors.Paths);
            drawColorEdit("Start Points", ref _prefs.Viewport.Colors.StartPoints);
            drawColorEdit("Respawn Points", ref _prefs.Viewport.Colors.RespawnPoints);
            drawColorEdit("Kart Point 2D", ref _prefs.Viewport.Colors.KartPoint2D);
            drawColorEdit("Cannon Points", ref _prefs.Viewport.Colors.CannonPoints);
            drawColorEdit("Mission Points", ref _prefs.Viewport.Colors.KartPointMission);
            drawColorEdit("Enemy Paths", ref _prefs.Viewport.Colors.EnemyPaths);
            drawColorEdit("Battle Enemy Paths", ref _prefs.Viewport.Colors.MgEnemyPaths);
            drawColorEdit("Item Paths", ref _prefs.Viewport.Colors.ItemPaths);
            drawColorEdit("Check Points", ref _prefs.Viewport.Colors.CheckPoint);
            drawColorEdit("Key Check Points", ref _prefs.Viewport.Colors.KeyCheckPoint);
            drawColorEdit("Areas", ref _prefs.Viewport.Colors.Areas);
            drawColorEdit("Cameras", ref _prefs.Viewport.Colors.Cameras);
        }
    }

    private void DrawKeyBindings()
    {
        if (ImGui.CollapsingHeader("Shortcuts##KeyBindings_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2);
            DrawKeyBindingInput("Undo last action", ref _prefs.KeyBindings.Shortcuts.Undo);
            DrawKeyBindingInput("Redo last action", ref _prefs.KeyBindings.Shortcuts.Redo);
            ImGui.Columns(1);
        }

        if (ImGui.CollapsingHeader("Viewport Camera##KeyBindings_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2);
            DrawKeyBindingInput("Forward", ref _prefs.KeyBindings.Viewport.Forward);
            DrawKeyBindingInput("Left", ref _prefs.KeyBindings.Viewport.Left);
            DrawKeyBindingInput("Back", ref _prefs.KeyBindings.Viewport.Backward);
            DrawKeyBindingInput("Right", ref _prefs.KeyBindings.Viewport.Right);
            DrawKeyBindingInput("Ascend", ref _prefs.KeyBindings.Viewport.Up);
            DrawKeyBindingInput("Descend", ref _prefs.KeyBindings.Viewport.Down);
            ImGui.Columns(1);
        }

        if (ImGui.CollapsingHeader("Gizmo##KeyBindings_Preferences", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Columns(2);

            DrawKeyBindingInput("Collision Snapping", ref _prefs.KeyBindings.Gizmo.SnapToCollision);

            DrawKeyBindingInput("Draw", ref _prefs.KeyBindings.Gizmo.ToolsDraw);
            DrawKeyBindingInput("Translate", ref _prefs.KeyBindings.Gizmo.ToolsTranslate);
            DrawKeyBindingInput("Rotate", ref _prefs.KeyBindings.Gizmo.ToolsRotate);
            DrawKeyBindingInput("Scale", ref _prefs.KeyBindings.Gizmo.ToolsScale);

            DrawKeyBindingInput("X Axis Constraint", ref _prefs.KeyBindings.Gizmo.ToolsAxisConstraintX);
            DrawKeyBindingInput("Y Axis Constraint", ref _prefs.KeyBindings.Gizmo.ToolsAxisConstraintY);
            DrawKeyBindingInput("Z Axis Constraint", ref _prefs.KeyBindings.Gizmo.ToolsAxisConstraintZ);

            DrawKeyBindingInput("XY Plane Constraint", ref _prefs.KeyBindings.Gizmo.ToolsAxisConstraintXY);
            DrawKeyBindingInput("XZ Plane Constraint", ref _prefs.KeyBindings.Gizmo.ToolsAxisConstraintXZ);
            DrawKeyBindingInput("YZ Plane Constraint", ref _prefs.KeyBindings.Gizmo.ToolsAxisConstraintYZ);

            ImGui.Columns(1);
        }
    }

    private string _keyBindingCurrentLabel;

    private static ImGuiKey? GetPressedKey()
    {
        for (ImGuiKey key = ImGuiKey.NamedKey_BEGIN; key < ImGuiKey.MouseLeft; key++)
        {
            if (ImGui.IsKeyDown(key))
                return key;
        }

        return null;
    }

    private void DrawKeyBindingInput(string label, ref KeyBinding value, bool allowModifierKeys = true)
    {
        bool active = _keyBindingCurrentLabel == label;

        ImGui.Text(label);
        ImGui.NextColumn();

        var region = ImGui.GetContentRegionAvail();

        var buttonSize = new Vector2(region.X, 0);

        if (active)
        {
            var io = ImGui.GetIO();

            if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                _keyBindingCurrentLabel = null;
            }
            else
            {
                var newInput = GetPressedKey();
                if (newInput.HasValue)
                {
                    //Assign new input here

                    if (allowModifierKeys)
                        value = new KeyBinding(newInput.Value, io.KeyCtrl, io.KeyShift, io.KeyAlt);
                    else
                        value = new KeyBinding(newInput.Value);


                    _keyBindingCurrentLabel = null;
                    _isDirty = true;
                }
            }

            ImGui.BeginDisabled();
        }

        if (!active ? ImGui.Button($"{value}##{label}_Button", buttonSize)
                : ImGui.Button($"Press a key combination...##{label}_Button", buttonSize))
        {
            _keyBindingCurrentLabel = label;
        }

        if (active)
        {
            ImGui.EndDisabled();
        }

        ImGui.NextColumn();
    }
}