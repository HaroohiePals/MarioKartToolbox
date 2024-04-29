using ImGuiNET;
using Newtonsoft.Json;
using System.Drawing;
using System.Numerics;

namespace HaroohiePals.Gui.Themes;

public class ImGuiTheme
{
    public string Name;

    //public float FrameBorderSize;
    //public float WindowRounding;
    //public float ChildRounding;
    //public float FrameRounding;
    //public float PopupRounding;
    //public float ScrollbarRounding;
    //public float GrabRounding;
    //public float TabRounding;

    public float Alpha;
    public float DisabledAlpha;
    public Vector2 WindowPadding;
    public float WindowRounding;
    public float WindowBorderSize;
    public Vector2 WindowMinSize;
    public Vector2 WindowTitleAlign;
    public ImGuiDir WindowMenuButtonPosition;
    public float ChildRounding;
    public float ChildBorderSize;
    public float PopupRounding;
    public float PopupBorderSize;
    public Vector2 FramePadding;
    public float FrameRounding;
    public float FrameBorderSize;
    public Vector2 ItemSpacing;
    public Vector2 ItemInnerSpacing;
    public Vector2 CellPadding;
    public Vector2 TouchExtraPadding;
    public float IndentSpacing;
    public float ColumnsMinSpacing;
    public float ScrollbarSize;
    public float ScrollbarRounding;
    public float GrabMinSize;
    public float GrabRounding;
    public float LogSliderDeadzone;
    public float TabRounding;
    public float TabBorderSize;
    public float TabMinWidthForCloseButton;
    public ImGuiDir ColorButtonPosition;
    public Vector2 ButtonTextAlign;
    public Vector2 SelectableTextAlign;
    public Vector2 DisplayWindowPadding;
    public Vector2 DisplaySafeAreaPadding;
    public float MouseCursorScale;
    public bool AntiAliasedLines;
    public bool AntiAliasedLinesUseTex;
    public bool AntiAliasedFill;
    public float CurveTessellationTol;
    public float CircleTessellationMaxError;

    public class ThemeColors
    {
        public Color Text;
        public Color TextDisabled;
        public Color TextSelectedBg;
        public Color WindowBg;
        public Color ChildBg;
        public Color PopupBg;
        public Color Border;
        public Color BorderShadow;
        public Color FrameBg;
        public Color FrameBgHovered;
        public Color FrameBgActive;
        public Color TitleBg;
        public Color TitleBgActive;
        public Color TitleBgCollapsed;
        public Color MenuBarBg;
        public Color ScrollbarBg;
        public Color ScrollbarGrab;
        public Color ScrollbarGrabHovered;
        public Color ScrollbarGrabActive;
        public Color CheckMark;
        public Color SliderGrab;
        public Color SliderGrabActive;
        public Color Button;
        public Color ButtonHovered;
        public Color ButtonActive;
        public Color Header;
        public Color HeaderHovered;
        public Color HeaderActive;
        public Color Separator;
        public Color SeparatorHovered;
        public Color SeparatorActive;
        public Color ResizeGrip;
        public Color ResizeGripHovered;
        public Color ResizeGripActive;
        public Color PlotLines;
        public Color PlotLinesHovered;
        public Color PlotHistogram;
        public Color PlotHistogramHovered;
        public Color ModalWindowDimBg;
        public Color DragDropTarget;
        public Color NavHighlight;
        public Color DockingPreview;
        public Color DockingEmptyBg;
        public Color Tab;
        public Color TabActive;
        public Color TabUnfocused;
        public Color TabUnfocusedActive;
        public Color TabHovered;
        public Color TableHeaderBg;
        public Color TableBorderStrong;
        public Color TableBorderLight;
        public Color TableRowBg;
        public Color TableRowBgAlt;
        public Color NavWindowingHighlight;
        public Color NavWindowingDimBg;
    }

    public ThemeColors Colors = new();

    public ImGuiTheme() { }

    public static ImGuiTheme FromJson(string jsonData)
    {
        return JsonConvert.DeserializeObject<ImGuiTheme>(jsonData);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    /// <summary>
    /// Grabs current ImGui style settings and creates a Theme object. Useful for user customization.
    /// </summary>
    /// <param name="themeName"></param>
    /// <returns></returns>
    public static ImGuiTheme Create(string themeName = "Custom")
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        var theme = new ImGuiTheme();

        theme.Name = themeName;

        theme.Alpha = style.Alpha;
        theme.DisabledAlpha = style.DisabledAlpha;
        theme.WindowPadding = style.WindowPadding;
        theme.WindowRounding = style.WindowRounding;
        theme.WindowBorderSize = style.WindowBorderSize;
        theme.WindowMinSize = style.WindowMinSize;
        theme.WindowTitleAlign = style.WindowTitleAlign;
        theme.WindowMenuButtonPosition = style.WindowMenuButtonPosition;
        theme.ChildRounding = style.ChildRounding;
        theme.ChildBorderSize = style.ChildBorderSize;
        theme.PopupRounding = style.PopupRounding;
        theme.PopupBorderSize = style.PopupBorderSize;
        theme.FramePadding = style.FramePadding;
        theme.FrameRounding = style.FrameRounding;
        theme.FrameBorderSize = style.FrameBorderSize;
        theme.ItemSpacing = style.ItemSpacing;
        theme.ItemInnerSpacing = style.ItemInnerSpacing;
        theme.CellPadding = style.CellPadding;
        theme.TouchExtraPadding = style.TouchExtraPadding;
        theme.IndentSpacing = style.IndentSpacing;
        theme.ColumnsMinSpacing = style.ColumnsMinSpacing;
        theme.ScrollbarSize = style.ScrollbarSize;
        theme.ScrollbarRounding = style.ScrollbarRounding;
        theme.GrabMinSize = style.GrabMinSize;
        theme.GrabRounding = style.GrabRounding;
        theme.LogSliderDeadzone = style.LogSliderDeadzone;
        theme.TabRounding = style.TabRounding;
        theme.TabBorderSize = style.TabBorderSize;
        theme.TabMinWidthForCloseButton = style.TabMinWidthForCloseButton;
        theme.ColorButtonPosition = style.ColorButtonPosition;
        theme.ButtonTextAlign = style.ButtonTextAlign;
        theme.SelectableTextAlign = style.SelectableTextAlign;
        theme.DisplayWindowPadding = style.DisplayWindowPadding;
        theme.DisplaySafeAreaPadding = style.DisplaySafeAreaPadding;
        theme.MouseCursorScale = style.MouseCursorScale;
        theme.AntiAliasedLines = style.AntiAliasedLines;
        theme.AntiAliasedLinesUseTex = style.AntiAliasedLinesUseTex;
        theme.AntiAliasedFill = style.AntiAliasedFill;
        theme.CurveTessellationTol = style.CurveTessellationTol;
        theme.CircleTessellationMaxError = style.CircleTessellationMaxError;

        theme.Colors.Text = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.Text]);
        theme.Colors.TextDisabled = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TextDisabled]);
        theme.Colors.WindowBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.WindowBg]);
        theme.Colors.ChildBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ChildBg]);
        theme.Colors.PopupBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.PopupBg]);
        theme.Colors.Border = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.Border]);
        theme.Colors.BorderShadow = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.BorderShadow]);
        theme.Colors.FrameBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.FrameBg]);
        theme.Colors.FrameBgHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.FrameBgHovered]);
        theme.Colors.FrameBgActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.FrameBgActive]);
        theme.Colors.TitleBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TitleBg]);
        theme.Colors.TitleBgActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TitleBgActive]);
        theme.Colors.TitleBgCollapsed = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TitleBgCollapsed]);
        theme.Colors.MenuBarBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.MenuBarBg]);
        theme.Colors.ScrollbarBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ScrollbarBg]);
        theme.Colors.ScrollbarGrab = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ScrollbarGrab]);
        theme.Colors.ScrollbarGrabHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ScrollbarGrabHovered]);
        theme.Colors.ScrollbarGrabActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ScrollbarGrabActive]);
        theme.Colors.CheckMark = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.CheckMark]);
        theme.Colors.SliderGrab = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.SliderGrab]);
        theme.Colors.SliderGrabActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.SliderGrabActive]);
        theme.Colors.Button = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.Button]);
        theme.Colors.ButtonHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ButtonHovered]);
        theme.Colors.ButtonActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ButtonActive]);
        theme.Colors.Header = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.Header]);
        theme.Colors.HeaderHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.HeaderHovered]);
        theme.Colors.HeaderActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.HeaderActive]);
        theme.Colors.Separator = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.Separator]);
        theme.Colors.SeparatorHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.SeparatorHovered]);
        theme.Colors.SeparatorActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.SeparatorActive]);
        theme.Colors.ResizeGrip = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ResizeGrip]);
        theme.Colors.ResizeGripHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ResizeGripHovered]);
        theme.Colors.ResizeGripActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ResizeGripActive]);
        theme.Colors.Tab = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.Tab]);
        theme.Colors.TabHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TabHovered]);
        theme.Colors.TabActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TabActive]);
        theme.Colors.TabUnfocused = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TabUnfocused]);
        theme.Colors.TabUnfocusedActive = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TabUnfocusedActive]);
        theme.Colors.DockingPreview = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.DockingPreview]);
        theme.Colors.DockingEmptyBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.DockingEmptyBg]);
        theme.Colors.PlotLines = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.PlotLines]);
        theme.Colors.PlotLinesHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.PlotLinesHovered]);
        theme.Colors.PlotHistogram = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.PlotHistogram]);
        theme.Colors.PlotHistogramHovered = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.PlotHistogramHovered]);
        theme.Colors.TableHeaderBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TableHeaderBg]);
        theme.Colors.TableBorderStrong = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TableBorderStrong]);
        theme.Colors.TableBorderLight = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TableBorderLight]);
        theme.Colors.TableRowBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TableRowBg]);
        theme.Colors.TableRowBgAlt = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TableRowBgAlt]);
        theme.Colors.TextSelectedBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.TextSelectedBg]);
        theme.Colors.DragDropTarget = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.DragDropTarget]);
        theme.Colors.NavHighlight = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.NavHighlight]);
        theme.Colors.NavWindowingHighlight = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.NavWindowingHighlight]);
        theme.Colors.NavWindowingDimBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.NavWindowingDimBg]);
        theme.Colors.ModalWindowDimBg = ImGuiEx.ColorConvertFloat4ToColor(colors[(int)ImGuiCol.ModalWindowDimBg]);

        return theme;
    }

    /// <summary>
    /// Apply the theme to the ImGui style settings.
    /// </summary>
    public void Apply()
    {
        var style = ImGui.GetStyle();
        var colors = style.Colors;

        float scale = ImGuiEx.GetUiScale();

        style.Alpha = Alpha;
        style.DisabledAlpha = DisabledAlpha;
        style.WindowPadding = WindowPadding * scale;
        style.WindowRounding = WindowRounding;
        style.WindowBorderSize = WindowBorderSize * scale;
        style.WindowMinSize = WindowMinSize * scale;
        style.WindowTitleAlign = WindowTitleAlign;
        style.WindowMenuButtonPosition = WindowMenuButtonPosition;
        style.ChildRounding = ChildRounding;
        style.ChildBorderSize = ChildBorderSize * scale;
        style.PopupRounding = PopupRounding;
        style.PopupBorderSize = PopupBorderSize * scale;
        style.FramePadding = FramePadding * scale;
        style.FrameRounding = FrameRounding;
        style.FrameBorderSize = FrameBorderSize * scale;
        style.ItemSpacing = ItemSpacing * scale;
        style.ItemInnerSpacing = ItemInnerSpacing * scale;
        style.CellPadding = CellPadding * scale;
        style.TouchExtraPadding = TouchExtraPadding * scale;
        style.IndentSpacing = IndentSpacing * scale;
        style.ColumnsMinSpacing = ColumnsMinSpacing * scale;
        style.ScrollbarSize = ScrollbarSize * scale;
        style.ScrollbarRounding = ScrollbarRounding;
        style.GrabMinSize = GrabMinSize * scale;
        style.GrabRounding = GrabRounding;
        style.LogSliderDeadzone = LogSliderDeadzone;
        style.TabRounding = TabRounding;
        style.TabBorderSize = TabBorderSize * scale;
        style.TabMinWidthForCloseButton = TabMinWidthForCloseButton;
        style.ColorButtonPosition = ColorButtonPosition;
        style.ButtonTextAlign = ButtonTextAlign;
        style.SelectableTextAlign = SelectableTextAlign;
        style.DisplayWindowPadding = DisplayWindowPadding * scale;
        style.DisplaySafeAreaPadding = DisplaySafeAreaPadding * scale;
        style.MouseCursorScale = MouseCursorScale;
        style.AntiAliasedLines = AntiAliasedLines;
        style.AntiAliasedLinesUseTex = AntiAliasedLinesUseTex;
        style.AntiAliasedFill = AntiAliasedFill;
        style.CurveTessellationTol = CurveTessellationTol;
        style.CircleTessellationMaxError = CircleTessellationMaxError;

        colors[(int)ImGuiCol.Text] = ImGuiEx.ColorConvertColorToFloat4(Colors.Text);
        colors[(int)ImGuiCol.TextDisabled] = ImGuiEx.ColorConvertColorToFloat4(Colors.TextDisabled);
        colors[(int)ImGuiCol.WindowBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.WindowBg);
        colors[(int)ImGuiCol.ChildBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.ChildBg);
        colors[(int)ImGuiCol.PopupBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.PopupBg);
        colors[(int)ImGuiCol.Border] = ImGuiEx.ColorConvertColorToFloat4(Colors.Border);
        colors[(int)ImGuiCol.BorderShadow] = ImGuiEx.ColorConvertColorToFloat4(Colors.BorderShadow);
        colors[(int)ImGuiCol.FrameBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.FrameBg);
        colors[(int)ImGuiCol.FrameBgHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.FrameBgHovered);
        colors[(int)ImGuiCol.FrameBgActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.FrameBgActive);
        colors[(int)ImGuiCol.TitleBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.TitleBg);
        colors[(int)ImGuiCol.TitleBgActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.TitleBgActive);
        colors[(int)ImGuiCol.TitleBgCollapsed] = ImGuiEx.ColorConvertColorToFloat4(Colors.TitleBgCollapsed);
        colors[(int)ImGuiCol.MenuBarBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.MenuBarBg);
        colors[(int)ImGuiCol.ScrollbarBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.ScrollbarBg);
        colors[(int)ImGuiCol.ScrollbarGrab] = ImGuiEx.ColorConvertColorToFloat4(Colors.ScrollbarGrab);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.ScrollbarGrabHovered);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.ScrollbarGrabActive);
        colors[(int)ImGuiCol.CheckMark] = ImGuiEx.ColorConvertColorToFloat4(Colors.CheckMark);
        colors[(int)ImGuiCol.SliderGrab] = ImGuiEx.ColorConvertColorToFloat4(Colors.SliderGrab);
        colors[(int)ImGuiCol.SliderGrabActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.SliderGrabActive);
        colors[(int)ImGuiCol.Button] = ImGuiEx.ColorConvertColorToFloat4(Colors.Button);
        colors[(int)ImGuiCol.ButtonHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.ButtonHovered);
        colors[(int)ImGuiCol.ButtonActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.ButtonActive);
        colors[(int)ImGuiCol.Header] = ImGuiEx.ColorConvertColorToFloat4(Colors.Header);
        colors[(int)ImGuiCol.HeaderHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.HeaderHovered);
        colors[(int)ImGuiCol.HeaderActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.HeaderActive);
        colors[(int)ImGuiCol.Separator] = ImGuiEx.ColorConvertColorToFloat4(Colors.Separator);
        colors[(int)ImGuiCol.SeparatorHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.SeparatorHovered);
        colors[(int)ImGuiCol.SeparatorActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.SeparatorActive);
        colors[(int)ImGuiCol.ResizeGrip] = ImGuiEx.ColorConvertColorToFloat4(Colors.ResizeGrip);
        colors[(int)ImGuiCol.ResizeGripHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.ResizeGripHovered);
        colors[(int)ImGuiCol.ResizeGripActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.ResizeGripActive);
        colors[(int)ImGuiCol.Tab] = ImGuiEx.ColorConvertColorToFloat4(Colors.Tab);
        colors[(int)ImGuiCol.TabHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.TabHovered);
        colors[(int)ImGuiCol.TabActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.TabActive);
        colors[(int)ImGuiCol.TabUnfocused] = ImGuiEx.ColorConvertColorToFloat4(Colors.TabUnfocused);
        colors[(int)ImGuiCol.TabUnfocusedActive] = ImGuiEx.ColorConvertColorToFloat4(Colors.TabUnfocusedActive);
        colors[(int)ImGuiCol.DockingPreview] = ImGuiEx.ColorConvertColorToFloat4(Colors.DockingPreview);
        colors[(int)ImGuiCol.DockingEmptyBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.DockingEmptyBg);
        colors[(int)ImGuiCol.PlotLines] = ImGuiEx.ColorConvertColorToFloat4(Colors.PlotLines);
        colors[(int)ImGuiCol.PlotLinesHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.PlotLinesHovered);
        colors[(int)ImGuiCol.PlotHistogram] = ImGuiEx.ColorConvertColorToFloat4(Colors.PlotHistogram);
        colors[(int)ImGuiCol.PlotHistogramHovered] = ImGuiEx.ColorConvertColorToFloat4(Colors.PlotHistogramHovered);
        colors[(int)ImGuiCol.TableHeaderBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.TableHeaderBg);
        colors[(int)ImGuiCol.TableBorderStrong] = ImGuiEx.ColorConvertColorToFloat4(Colors.TableBorderStrong);
        colors[(int)ImGuiCol.TableBorderLight] = ImGuiEx.ColorConvertColorToFloat4(Colors.TableBorderLight);
        colors[(int)ImGuiCol.TableRowBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.TableRowBg);
        colors[(int)ImGuiCol.TableRowBgAlt] = ImGuiEx.ColorConvertColorToFloat4(Colors.TableRowBgAlt);
        colors[(int)ImGuiCol.TextSelectedBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.TextSelectedBg);
        colors[(int)ImGuiCol.DragDropTarget] = ImGuiEx.ColorConvertColorToFloat4(Colors.DragDropTarget);
        colors[(int)ImGuiCol.NavHighlight] = ImGuiEx.ColorConvertColorToFloat4(Colors.NavHighlight);
        colors[(int)ImGuiCol.NavWindowingHighlight] = ImGuiEx.ColorConvertColorToFloat4(Colors.NavWindowingHighlight);
        colors[(int)ImGuiCol.NavWindowingDimBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.NavWindowingDimBg);
        colors[(int)ImGuiCol.ModalWindowDimBg] = ImGuiEx.ColorConvertColorToFloat4(Colors.ModalWindowDimBg);
    }

    public override string ToString() => Name;
}
