using ImGuiNET;
using System.Drawing;

namespace HaroohiePals.Gui.View.ImSequencer;

static class ImSequencerViewStyleConsts
{
    //Colors
    public static uint TextColor => ImGui.GetColorU32(ImGuiCol.Text);
    public static uint ButtonColor => ImGui.GetColorU32(ImGuiCol.TextDisabled);
    public static uint ButtonHoveredColor => ImGui.GetColorU32(ImGuiCol.ButtonHovered);
    public static uint CollapseHeaderBgColor => ImGui.GetColorU32(ImGuiCol.TableHeaderBg);
    public static uint FrameSubdivColor => ImGui.GetColorU32(ImGuiCol.NavWindowingDimBg);
    public static uint FrameSubdivContentColor
        => ImGuiEx.ColorConvertColorToU32(Color.FromArgb(60, ImGuiEx.GetColor(ImGuiCol.NavWindowingDimBg)));
    public static uint FrameSubdivTextColor => TextColor;
    public static uint FrameCursorColor => ImGuiEx.ColorConvertColorToU32(Color.FromArgb(160, Color.Red));
    public static uint FrameCursorTextColor => ImGui.GetColorU32(ImGuiCol.TextDisabled);
    public static uint QuadColor => ImGuiEx.ColorConvertColorToU32(Color.White);
    public static uint TopBarColor => ImGuiEx.ColorConvertColorToU32(Color.Blue);
    public static uint BackgroundColor => ImGui.GetColorU32(ImGuiCol.ChildBg);
    public static uint CopyPasteInRectColor => ImGuiEx.ColorConvertColorToU32(Color.Orange);
    public static uint CopyPasteColor => ImGuiEx.ColorConvertColorToU32(Color.Black);
    public static uint BottomScrollbarBackColor => ImGui.GetColorU32(ImGuiCol.ScrollbarBg);
    public static uint BottomScrollbarBackColor2 => ImGui.GetColorU32(ImGuiCol.ScrollbarBg);
    public static uint ScrollbarColor => ImGui.GetColorU32(ImGuiCol.ScrollbarGrab);
    public static uint ScrollbarHoveredColor => ImGui.GetColorU32(ImGuiCol.ScrollbarGrabHovered);
    public static uint ScrollbarHandleColor => ImGui.GetColorU32(ImGuiCol.ScrollbarGrab);
    public static uint ScrollbarHandleHoveredColor => ImGui.GetColorU32(ImGuiCol.ScrollbarGrabHovered);
    public static uint SlotBackgroundOddColor => ImGui.GetColorU32(ImGuiCol.TableRowBg);
    public static uint SlotBackgroundEvenColor => ImGui.GetColorU32(ImGuiCol.TableRowBgAlt);
    public static uint RowActiveColor => ImGuiEx.ColorConvertColorToU32(Color.FromArgb(128, Color.OrangeRed));
    public static uint RowHoveredColor
        => ImGuiEx.ColorConvertColorToU32(Color.FromArgb(60, ImGuiEx.GetColor(ImGuiCol.TextSelectedBg)));
    public static uint SlotColorAlphaValue => 0xFF000000;
    public static uint SlotColorHalfMask => 0xFFFFFF;
    public static uint SlotColorHalfAlphaValue => 0x40000000;
    public static uint UnselectedSlotQuadColorBgrValue => 0x00202020;
    //Dimensions
    public static float FrameCursorWidth => ImGuiEx.CalcUiScaledValue(8f);
    public static float RowHighlightRectRounding => ImGuiEx.CalcUiScaledValue(0f);
    public static float SlotRectRounding => ImGuiEx.CalcUiScaledValue(2f);
    public static float QuadRectRounding => ImGuiEx.CalcUiScaledValue(2f);
    public static float ScrollbarRounding => ImGui.GetStyle().ScrollbarRounding;
    public static float ScrollbarBackRounding => ScrollbarRounding;
    public static float ItemHeight => ImGuiEx.CalcUiScaledValue(20f);
    public static float LegendWidth => ImGuiEx.CalcUiScaledValue(200f);
    public static float ButtonRounding => ImGui.GetStyle().TabRounding;
    public static float ScrollbarHandleWidth => ImGuiEx.CalcUiScaledValue(14f);
    public static float MinScrollbarWidth => ImGuiEx.CalcUiScaledValue(44f);
}