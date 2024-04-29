using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace HaroohiePals.Gui.View.ImSequencer;

// Ported from https://github.com/CedricGuillemet/ImGuizmo/blob/master/ImSequencer.cpp
public class ImSequencerView : IView
{
    public ISequence Sequence;
    public int CurrentFrame = 0;
    public bool Expanded = true;
    public int SelectedEntry = -1;
    public int FirstFrame = 0;

    public int StartFrame = 0;
    public int EndFrame = 500;

    public bool UseSequenceStartEnd = false;

    public ImSequencerOptions Options;

    private bool _movingScrollBar = false;
    private bool _movingCurrentFrame = false;

    private bool _panningView = false;
    private Vector2 _panningViewSource = Vector2.Zero;
    private int _panningViewFrame = 0;

    private bool _sizingRBar = false;
    private bool _sizingLBar = false;

    private float _framePixelWidth = 10;
    private float _framePixelWidthTarget = 10;
    private int _movingEntry = -1;
    private int _movingPos = -1;
    private int _movingPart = -1;
    private bool _canInteract = false;

    public bool Draw()
    {
        bool ret = false;
        var io = ImGui.GetIO();
        int cx = (int)io.MousePos.X;
        int cy = (int)io.MousePos.Y;

        _canInteract = ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows);

        int delEntry = -1;
        int dupEntry = -1;

        bool popupOpened = false;
        int sequenceCount = Sequence.GetItemCount();
        if (sequenceCount == 0)
            return false;
        ImGui.BeginGroup();

        var drawList = ImGui.GetWindowDrawList();
        var canvasPos = ImGui.GetCursorScreenPos();            // ImDrawList API uses screen coordinates!
        var canvasSize = ImGui.GetContentRegionAvail();        // Resize canvas to what's available

        //int firstFrameUsed = firstFrame ? *firstFrame : 0;
        int firstFrameUsed = FirstFrame;

        float controlHeight = sequenceCount * ImSequencerViewStyleConsts.ItemHeight;
        for (int i = 0; i < sequenceCount; i++)
            controlHeight += Sequence.GetCustomHeight(i);
        int frameCount = Math.Max(GetFrameMax() - GetFrameMin(), 1);

        var customDraws = new List<ImSequencerCustomDraw>();
        var compactCustomDraws = new List<ImSequencerCustomDraw>();

        // zoom in/out
        int visibleFrameCount = (int)Math.Floor((canvasSize.X - ImSequencerViewStyleConsts.LegendWidth) / _framePixelWidth);
        float barWidthRatio = Math.Min(visibleFrameCount / (float)frameCount, 1f);
        float barWidthInPixels = barWidthRatio * (canvasSize.X - ImSequencerViewStyleConsts.LegendWidth);

        var regionRect = new ImRect(canvasPos, canvasPos + canvasSize);

        if (ImGui.IsWindowFocused() && io.KeyAlt && io.MouseDown[2])
        {
            if (!_panningView)
            {
                _panningViewSource = io.MousePos;
                _panningView = true;
                _panningViewFrame = FirstFrame;
            }
            FirstFrame = _panningViewFrame - (int)((io.MousePos.X - _panningViewSource.X) / _framePixelWidth);
            FirstFrame = Math.Clamp(FirstFrame, GetFrameMin(), GetFrameMax() - visibleFrameCount);
        }
        if (_panningView && !io.MouseDown[2])
        {
            _panningView = false;
        }
        _framePixelWidthTarget = Math.Clamp(_framePixelWidthTarget, 0.1f, 50f);

        _framePixelWidth = OpenTK.Mathematics.MathHelper.Lerp(_framePixelWidth, _framePixelWidthTarget, 0.33f);

        frameCount = GetFrameMax() - GetFrameMin();
        if (visibleFrameCount >= frameCount)
            FirstFrame = GetFrameMin();


        // --
        if (!Expanded)
        {
            ImGui.InvisibleButton("canvas", new Vector2(canvasSize.X - canvasPos.X, ImSequencerViewStyleConsts.ItemHeight));
            drawList.AddRectFilled(canvasPos, new Vector2(canvasSize.X + canvasPos.X, canvasPos.Y + ImSequencerViewStyleConsts.ItemHeight), ImSequencerViewStyleConsts.CollapseHeaderBgColor, 0);

            string tmps = string.Format(Sequence.GetCollapseFmt(), frameCount, sequenceCount);

            drawList.AddText(new Vector2(canvasPos.X + ImGuiEx.CalcUiScaledValue(26f), canvasPos.Y + ImGuiEx.CalcUiScaledValue(2f)), ImSequencerViewStyleConsts.TextColor, tmps);
        }
        else
        {
            bool hasScrollBar = true;
            /*
            int framesPixelWidth = int(frameCount * framePixelWidth);
            if ((framesPixelWidth + legendWidth) >= canvas_size.X)
            {
                hasScrollBar = true;
            }
            */
            // test scroll area

            var headerSize = new Vector2(canvasSize.X, ImSequencerViewStyleConsts.ItemHeight);
            var scrollBarSize = new Vector2(canvasSize.X, ImGuiEx.CalcUiScaledValue(14f));

            ImGui.InvisibleButton("topBar", headerSize);
            drawList.AddRectFilled(canvasPos, canvasPos + headerSize, ImSequencerViewStyleConsts.TopBarColor, 0);
            var childFramePos = ImGui.GetCursorScreenPos();
            var childFrameSize = new Vector2(canvasSize.X, canvasSize.Y - ImGuiEx.CalcUiScaledValue(8f) - headerSize.Y - (hasScrollBar ? scrollBarSize.Y : 0));


            ImGui.PushStyleColor(ImGuiCol.FrameBg, 0);
            ImGui.BeginChildFrame(ImGui.GetID(GetHashCode()), childFrameSize);
            Sequence.Focused = ImGui.IsWindowFocused();
            ImGui.InvisibleButton("contentBar", new Vector2(canvasSize.X, controlHeight));
            var contentMin = ImGui.GetItemRectMin();
            var contentMax = ImGui.GetItemRectMax();

            var contentRect = new ImRect(contentMin, contentMax);

            float contentHeight = contentMax.Y - contentMin.Y;

            // full background
            drawList.AddRectFilled(canvasPos, canvasPos + canvasSize, ImSequencerViewStyleConsts.BackgroundColor, 0);

            // current frame top
            var topRect = new ImRect(new Vector2(canvasPos.X + ImSequencerViewStyleConsts.LegendWidth, canvasPos.Y), new Vector2(canvasPos.X + canvasSize.X, canvasPos.Y + ImSequencerViewStyleConsts.ItemHeight));

            if (_canInteract)
            {
                if (!_movingCurrentFrame && !_movingScrollBar && _movingEntry == -1 && Options.HasFlag(ImSequencerOptions.ChangeFrame) && CurrentFrame >= 0 && topRect.Contains(io.MousePos) && io.MouseDown[0])
                {
                    _movingCurrentFrame = true;
                }
                if (_movingCurrentFrame)
                {
                    if (frameCount > 0)
                    {
                        CurrentFrame = (int)((io.MousePos.X - topRect.Min.X) / _framePixelWidth) + firstFrameUsed;
                        if (CurrentFrame < GetFrameMin())
                            CurrentFrame = GetFrameMin();
                        if (CurrentFrame >= GetFrameMax())
                            CurrentFrame = GetFrameMax();
                    }
                    if (!io.MouseDown[0])
                        _movingCurrentFrame = false;
                }
            }

            //header
            drawList.AddRectFilled(canvasPos, new Vector2(canvasSize.X + canvasPos.X, canvasPos.Y + ImSequencerViewStyleConsts.ItemHeight), ImSequencerViewStyleConsts.CollapseHeaderBgColor, 0);
            if (Options.HasFlag(ImSequencerOptions.Add))
            {
                if (SequencerAddDelButton(drawList, new Vector2(canvasPos.X + ImSequencerViewStyleConsts.LegendWidth - ImSequencerViewStyleConsts.ItemHeight, canvasPos.Y + 2), true))
                    ImGui.OpenPopup("addEntry");

                if (ImGui.BeginPopup("addEntry"))
                {
                    for (int i = 0; i < Sequence.GetItemTypeCount(); i++)
                        if (ImGui.Selectable(Sequence.GetItemTypeName(i)))
                        {
                            Sequence.Add(i);
                            SelectedEntry = Sequence.GetItemCount() - 1;
                        }

                    ImGui.EndPopup();
                    popupOpened = true;
                }
            }

            //header frame number and lines
            int modFrameCount = 10;
            int frameStep = 1;
            while (modFrameCount * _framePixelWidth < 150)
            {
                modFrameCount *= 2;
                frameStep *= 2;
            };
            int halfModFrameCount = modFrameCount / 2;

            void drawLine(int i, float regionHeight)
            {
                bool baseIndex = i % modFrameCount == 0 || i == GetFrameMax() || i == GetFrameMin();
                bool halfIndex = i % halfModFrameCount == 0;

                float px = canvasPos.X + (i * _framePixelWidth) + ImSequencerViewStyleConsts.LegendWidth - (firstFrameUsed * _framePixelWidth);
                float tiretStart = ImGuiEx.CalcUiScaledValue(baseIndex ? 4 : halfIndex ? 10 : 14);
                float tiretEnd = baseIndex ? regionHeight : ImSequencerViewStyleConsts.ItemHeight;

                if (px <= canvasSize.X + canvasPos.X && px >= canvasPos.X + ImSequencerViewStyleConsts.LegendWidth)
                {
                    drawList.AddLine(new Vector2(px, canvasPos.Y + tiretStart), new Vector2(px, canvasPos.Y + tiretEnd - 1), ImSequencerViewStyleConsts.FrameSubdivColor, 1);

                    drawList.AddLine(new Vector2(px, canvasPos.Y + ImSequencerViewStyleConsts.ItemHeight), new Vector2(px, canvasPos.Y + regionHeight - 1), ImSequencerViewStyleConsts.FrameSubdivContentColor, 1);
                }

                if (baseIndex && px > canvasPos.X + ImSequencerViewStyleConsts.LegendWidth)
                {
                    drawList.AddText(new Vector2((float)px + 3, canvasPos.Y), ImSequencerViewStyleConsts.FrameSubdivTextColor, i.ToString());
                }
            };

            void drawLineContent(int i, float regionHeight = 0f)
            {
                float px = canvasPos.X + (i * _framePixelWidth) + ImSequencerViewStyleConsts.LegendWidth - (firstFrameUsed * _framePixelWidth);
                float tiretStart = contentMin.Y;
                float tiretEnd = contentMax.Y;

                if (px <= canvasSize.X + canvasPos.X && px >= canvasPos.X + ImSequencerViewStyleConsts.LegendWidth)
                {
                    //draw_list.AddLine(ImVec2((float)px, canvas_pos.Y + (float)tiretStart), ImVec2((float)px, canvas_pos.Y + (float)tiretEnd - 1), _frameSubdivColor, 1);

                    drawList.AddLine(new Vector2(px, tiretStart), new Vector2(px, tiretEnd), ImSequencerViewStyleConsts.FrameSubdivContentColor, 1);
                }
            };

            for (int i = GetFrameMin(); i <= GetFrameMax(); i += frameStep)
            {
                drawLine(i, ImSequencerViewStyleConsts.ItemHeight);
            }

            drawLine(GetFrameMin(), ImSequencerViewStyleConsts.ItemHeight);
            drawLine(GetFrameMax(), ImSequencerViewStyleConsts.ItemHeight);

            // clip content
            drawList.PushClipRect(childFramePos, childFramePos + childFrameSize, true);

            // draw item names in the legend rect on the left
            int customHeight = 0;
            for (int i = 0; i < sequenceCount; i++)
            {
                Sequence.Get(i, out _, out _, out int type, out _);
                var tpos = new Vector2(contentMin.X + 3, contentMin.Y + i * ImSequencerViewStyleConsts.ItemHeight + 2 + customHeight);
                drawList.AddText(tpos, ImSequencerViewStyleConsts.TextColor, Sequence.GetItemLabel(i));

                if (Options.HasFlag(ImSequencerOptions.Delete))
                {
                    if (SequencerAddDelButton(drawList, new Vector2(contentMin.X + ImSequencerViewStyleConsts.LegendWidth - ImSequencerViewStyleConsts.ItemHeight + ImGuiEx.CalcUiScaledValue(2f) - ImGuiEx.CalcUiScaledValue(10f), tpos.Y + ImGuiEx.CalcUiScaledValue(2f)), false))
                        delEntry = i;

                    if (SequencerAddDelButton(drawList, new Vector2(contentMin.X + ImSequencerViewStyleConsts.LegendWidth - ImSequencerViewStyleConsts.ItemHeight - ImSequencerViewStyleConsts.ItemHeight + ImGuiEx.CalcUiScaledValue(2f) - ImGuiEx.CalcUiScaledValue(10f), tpos.Y + ImGuiEx.CalcUiScaledValue(2f)), true))
                        dupEntry = i;
                }
                customHeight += Sequence.GetCustomHeight(i);
            }

            // slots background
            customHeight = 0;
            for (int i = 0; i < sequenceCount; i++)
            {
                uint col = (i & 1) != 0 ? ImSequencerViewStyleConsts.SlotBackgroundOddColor : ImSequencerViewStyleConsts.SlotBackgroundEvenColor;

                int localCustomHeight = Sequence.GetCustomHeight(i);
                var pos = new Vector2(contentMin.X + ImSequencerViewStyleConsts.LegendWidth, contentMin.Y + ImSequencerViewStyleConsts.ItemHeight * i + 1 + customHeight);
                var sz = new Vector2(canvasSize.X + canvasPos.X, pos.Y + ImSequencerViewStyleConsts.ItemHeight - 1 + localCustomHeight);

                if (_canInteract)
                {
                    if (!popupOpened && cy >= pos.Y && cy < pos.Y + (ImSequencerViewStyleConsts.ItemHeight + localCustomHeight) && _movingEntry == -1 && cx > contentMin.X && cx < contentMin.X + canvasSize.X)
                    {
                        col = ImSequencerViewStyleConsts.RowHoveredColor;
                        pos.X -= ImSequencerViewStyleConsts.LegendWidth;
                    }
                }

                drawList.AddRectFilled(pos, sz, col, 0);
                customHeight += localCustomHeight;
            }

            drawList.PushClipRect(childFramePos + new Vector2(ImSequencerViewStyleConsts.LegendWidth, 0f), childFramePos + childFrameSize, true);

            // vertical frame lines in content area
            for (int i = GetFrameMin(); i <= GetFrameMax(); i += frameStep)
            {
                drawLineContent(i, (int)contentHeight);
            }
            drawLineContent(GetFrameMin(), (int)contentHeight);
            drawLineContent(GetFrameMax(), (int)contentHeight);

            // selection
            bool selected = /*selectedEntry &&*/ SelectedEntry > -1;
            if (selected)
            {
                customHeight = 0;
                for (int i = 0; i < SelectedEntry; i++)
                    customHeight += Sequence.GetCustomHeight(i);
                drawList.AddRectFilled(new Vector2(contentMin.X, contentMin.Y + ImSequencerViewStyleConsts.ItemHeight * SelectedEntry + customHeight), new Vector2(contentMin.X + canvasSize.X, contentMin.Y + ImSequencerViewStyleConsts.ItemHeight * (SelectedEntry + 1) + customHeight), ImSequencerViewStyleConsts.RowActiveColor, ImSequencerViewStyleConsts.RowHighlightRectRounding);
            }

            // slots
            customHeight = 0;
            for (int i = 0; i < sequenceCount; i++)
            {
                Sequence.Get(i, out int start, out int end, out _, out uint color);
                int localCustomHeight = Sequence.GetCustomHeight(i);

                var pos = new Vector2(contentMin.X + ImSequencerViewStyleConsts.LegendWidth - firstFrameUsed * _framePixelWidth, contentMin.Y + ImSequencerViewStyleConsts.ItemHeight * i + 1 + customHeight);
                var slotP1 = new Vector2(pos.X + start * _framePixelWidth, pos.Y + 2);
                var slotP2 = new Vector2(pos.X + end * _framePixelWidth + _framePixelWidth, pos.Y + ImSequencerViewStyleConsts.ItemHeight - 2);
                var slotP3 = new Vector2(pos.X + end * _framePixelWidth + _framePixelWidth, pos.Y + ImSequencerViewStyleConsts.ItemHeight - 2 + localCustomHeight);
                uint slotColor = color | ImSequencerViewStyleConsts.SlotColorAlphaValue;
                uint slotColorHalf = color & ImSequencerViewStyleConsts.SlotColorHalfMask | ImSequencerViewStyleConsts.SlotColorHalfAlphaValue;

                if (slotP1.X <= canvasSize.X + contentMin.X && slotP2.X >= contentMin.X + ImSequencerViewStyleConsts.LegendWidth)
                {
                    drawList.AddRectFilled(slotP1, slotP3, slotColorHalf, ImSequencerViewStyleConsts.SlotRectRounding);
                    drawList.AddRectFilled(slotP1, slotP2, slotColor, ImSequencerViewStyleConsts.SlotRectRounding);
                }
                if (_canInteract && new ImRect(slotP1, slotP2).Contains(io.MousePos) && io.MouseDoubleClicked[0])
                {
                    Sequence.DoubleClick(i);
                }
                // Ensure grabbable handles
                float max_handle_width = slotP2.X - slotP1.X / 3.0f;
                float min_handle_width = Math.Min(10.0f, max_handle_width);
                float handle_width = Math.Clamp(_framePixelWidth / 2.0f, min_handle_width, max_handle_width);
                ImRect[] rects = new[]{
                    new ImRect(slotP1, new Vector2(slotP1.X + handle_width, slotP2.Y)),
                    new ImRect(new Vector2(slotP2.X - handle_width, slotP1.Y), slotP2),
                    new ImRect(slotP1, slotP2) };

                uint[] quadColor = new[] { ImSequencerViewStyleConsts.QuadColor, ImSequencerViewStyleConsts.QuadColor, (uint)(slotColor + (selected ? 0 : ImSequencerViewStyleConsts.UnselectedSlotQuadColorBgrValue)) };
                if (_movingEntry == -1 && Options.HasFlag(ImSequencerOptions.EditStartEnd))// TODOFOCUS && backgroundRect.Contains(io.MousePos))
                {
                    for (int j = 2; j >= 0; j--)
                    {
                        var rc = rects[j];
                        if (!rc.Contains(io.MousePos))
                            continue;
                        drawList.AddRectFilled(rc.Min, rc.Max, quadColor[j], ImSequencerViewStyleConsts.QuadRectRounding);
                    }

                    for (int j = 0; j < 3; j++)
                    {
                        var rc = rects[j];
                        if (!rc.Contains(io.MousePos))
                            continue;
                        if (!new ImRect(childFramePos, childFramePos + childFrameSize).Contains(io.MousePos))
                            continue;
                        if (_canInteract && ImGui.IsMouseClicked(0) && !_movingScrollBar && !_movingCurrentFrame)
                        {
                            _movingEntry = i;
                            _movingPos = cx;
                            _movingPart = j + 1;
                            Sequence.BeginEdit(_movingEntry);
                            break;
                        }
                    }
                }

                // custom draw
                if (localCustomHeight > 0)
                {
                    var rp = new Vector2(canvasPos.X, contentMin.Y + ImSequencerViewStyleConsts.ItemHeight * i + 1 + customHeight);
                    var customRect = new ImRect(rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth - (firstFrameUsed - GetFrameMin() - 0.5f) * _framePixelWidth, ImSequencerViewStyleConsts.ItemHeight),
                          rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth + (GetFrameMax() - firstFrameUsed - 0.5f + 2f) * _framePixelWidth, localCustomHeight + ImSequencerViewStyleConsts.ItemHeight));
                    var clippingRect = new ImRect(rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth, ImSequencerViewStyleConsts.ItemHeight), rp + new Vector2(canvasSize.X, localCustomHeight + ImSequencerViewStyleConsts.ItemHeight));

                    var legendRect = new ImRect(rp + new Vector2(0, ImSequencerViewStyleConsts.ItemHeight), rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth, localCustomHeight));
                    var legendClippingRect = new ImRect(canvasPos + new Vector2(0f, ImSequencerViewStyleConsts.ItemHeight), canvasPos + new Vector2(ImSequencerViewStyleConsts.LegendWidth, localCustomHeight + ImSequencerViewStyleConsts.ItemHeight));
                    customDraws.Add(new ImSequencerCustomDraw(i, customRect, legendRect, clippingRect, legendClippingRect));
                }
                else
                {
                    var rp = new Vector2(canvasPos.X, contentMin.Y + ImSequencerViewStyleConsts.ItemHeight * i + customHeight);
                    var customRect = new ImRect(rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth - (firstFrameUsed - GetFrameMin() - 0.5f) * _framePixelWidth, 0f),
                        rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth + (GetFrameMax() - firstFrameUsed - 0.5f + 2f) * _framePixelWidth, ImSequencerViewStyleConsts.ItemHeight));
                    var clippingRect = new ImRect(rp + new Vector2(ImSequencerViewStyleConsts.LegendWidth, 0f), rp + new Vector2(canvasSize.X, ImSequencerViewStyleConsts.ItemHeight));

                    compactCustomDraws.Add(new ImSequencerCustomDraw(i, customRect, new ImRect(), clippingRect, new ImRect()));
                }
                customHeight += localCustomHeight;
            }


            // moving
            if (_movingEntry > -1)
            {
                ImGui.SetNextFrameWantCaptureMouse(true);

                int diffFrame = (int)((cx - _movingPos) / _framePixelWidth);
                if (Math.Abs(diffFrame) > 0)
                {
                    Sequence.Get(_movingEntry, out int start, out int end, out _, out _);
                    SelectedEntry = _movingEntry;
                    int l = start;
                    int r = end;
                    if ((_movingPart & 1) != 0)
                        l += diffFrame;
                    if ((_movingPart & 2) != 0)
                        r += diffFrame;
                    if (l < 0)
                    {
                        if ((_movingPart & 2) != 0)
                            r -= l;
                        l = 0;
                    }
                    if ((_movingPart & 1) != 0 && l > r)
                        l = r;
                    if ((_movingPart & 2) != 0 && r < l)
                        r = l;
                    _movingPos += (int)(diffFrame * _framePixelWidth);

                    Sequence.Edit(_movingEntry, l, r);
                }
                if (!io.MouseDown[0])
                {
                    // single select
                    if (diffFrame == 0 && _movingPart != 0)
                    {
                        SelectedEntry = _movingEntry;
                        ret = true;
                    }

                    _movingEntry = -1;
                    Sequence.EndEdit();
                }
            }

            // cursor
            if (CurrentFrame >= FirstFrame && CurrentFrame <= GetFrameMax())
            {
                float cursorOffset = contentMin.X + ImSequencerViewStyleConsts.LegendWidth + (CurrentFrame - firstFrameUsed) * _framePixelWidth + _framePixelWidth / 2 - ImSequencerViewStyleConsts.FrameCursorWidth * 0.5f;
                drawList.AddLine(new Vector2(cursorOffset, canvasPos.Y), new Vector2(cursorOffset, contentMax.Y), ImSequencerViewStyleConsts.FrameCursorColor, ImSequencerViewStyleConsts.FrameCursorWidth);

                drawList.AddText(new Vector2(cursorOffset + ImGuiEx.CalcUiScaledValue(10f), canvasPos.Y + ImGuiEx.CalcUiScaledValue(2f)), ImSequencerViewStyleConsts.FrameCursorTextColor, CurrentFrame.ToString());
            }

            drawList.PopClipRect();
            drawList.PopClipRect();

            foreach (var customDraw in customDraws)
                Sequence.CustomDraw(customDraw.Index, drawList, customDraw.CustomRect, customDraw.LegendRect, customDraw.ClippingRect, customDraw.LegendClippingRect);
            foreach (var customDraw in compactCustomDraws)
                Sequence.CustomDrawCompact(customDraw.Index, drawList, customDraw.CustomRect, customDraw.ClippingRect);

            // copy paste
            if (Options.HasFlag(ImSequencerOptions.CopyPaste))
            {
                var rectCopy = new ImRect(new Vector2(contentMin.X + 100, canvasPos.Y + 2), new Vector2(contentMin.X + 100 + 30, canvasPos.Y + ImSequencerViewStyleConsts.ItemHeight - 2));
                bool inRectCopy = rectCopy.Contains(io.MousePos);
                uint copyColor = inRectCopy ? ImSequencerViewStyleConsts.CopyPasteInRectColor : ImSequencerViewStyleConsts.CopyPasteColor;
                drawList.AddText(rectCopy.Min, copyColor, "Copy");

                var rectPaste = new ImRect(new Vector2(contentMin.X + 140, canvasPos.Y + 2), new Vector2(contentMin.X + 140 + 30, canvasPos.Y + ImSequencerViewStyleConsts.ItemHeight - 2));
                bool inRectPaste = rectPaste.Contains(io.MousePos);
                uint pasteColor = inRectPaste ? ImSequencerViewStyleConsts.CopyPasteInRectColor : ImSequencerViewStyleConsts.CopyPasteColor;
                drawList.AddText(rectPaste.Min, pasteColor, "Paste");

                if (inRectCopy && io.MouseReleased[0])
                {
                    Sequence.Copy();
                }
                if (inRectPaste && io.MouseReleased[0])
                {
                    Sequence.Paste();
                }
            }
            //

            ImGui.EndChildFrame();
            ImGui.PopStyleColor();
            if (hasScrollBar)
            {
                ImGui.InvisibleButton("scrollBar", scrollBarSize);
                var scrollBarMin = ImGui.GetItemRectMin();
                var scrollBarMax = ImGui.GetItemRectMax();

                // ratio = number of frames visible in control / number to total frames

                float startFrameOffset = (firstFrameUsed - GetFrameMin()) / (float)frameCount * (canvasSize.X - ImSequencerViewStyleConsts.LegendWidth);
                var scrollBarA = new Vector2(scrollBarMin.X + ImSequencerViewStyleConsts.LegendWidth, scrollBarMin.Y - ImGuiEx.CalcUiScaledValue(2f));
                var scrollBarB = new Vector2(scrollBarMin.X + canvasSize.X, scrollBarMax.Y - ImGuiEx.CalcUiScaledValue(1f));
                drawList.AddRectFilled(scrollBarA, scrollBarB, ImSequencerViewStyleConsts.BottomScrollbarBackColor, 0);

                var scrollBarRect = new ImRect(scrollBarA, scrollBarB);
                bool inScrollBar = _canInteract && scrollBarRect.Contains(io.MousePos);
                drawList.AddRectFilled(scrollBarA, scrollBarB, ImSequencerViewStyleConsts.BottomScrollbarBackColor2, ImSequencerViewStyleConsts.ScrollbarBackRounding);

                var scrollBarC = new Vector2(scrollBarMin.X + ImSequencerViewStyleConsts.LegendWidth + startFrameOffset, scrollBarMin.Y);
                var scrollBarD = new Vector2(scrollBarMin.X + ImSequencerViewStyleConsts.LegendWidth + barWidthInPixels + startFrameOffset, scrollBarMax.Y - ImGuiEx.CalcUiScaledValue(2f));
                drawList.AddRectFilled(scrollBarC, scrollBarD, inScrollBar || _movingScrollBar ? ImSequencerViewStyleConsts.ScrollbarHoveredColor : ImSequencerViewStyleConsts.ScrollbarColor, ImSequencerViewStyleConsts.ScrollbarRounding);

                var barHandleLeft = new ImRect(scrollBarC, new Vector2(scrollBarC.X + ImSequencerViewStyleConsts.ScrollbarHandleWidth, scrollBarD.Y));
                var barHandleRight = new ImRect(new Vector2(scrollBarD.X - ImSequencerViewStyleConsts.ScrollbarHandleWidth, scrollBarC.Y), scrollBarD);

                bool onLeft = _canInteract && barHandleLeft.Contains(io.MousePos);
                bool onRight = _canInteract && barHandleRight.Contains(io.MousePos);

                drawList.AddRectFilled(barHandleLeft.Min, barHandleLeft.Max, onLeft || _sizingLBar ? ImSequencerViewStyleConsts.ScrollbarHandleHoveredColor : ImSequencerViewStyleConsts.ScrollbarHandleColor, ImSequencerViewStyleConsts.ScrollbarRounding);
                drawList.AddRectFilled(barHandleRight.Min, barHandleRight.Max, onRight || _sizingRBar ? ImSequencerViewStyleConsts.ScrollbarHandleHoveredColor : ImSequencerViewStyleConsts.ScrollbarHandleColor, ImSequencerViewStyleConsts.ScrollbarRounding);

                var scrollBarThumb = new ImRect(scrollBarC, scrollBarD);

                if (_canInteract)
                {
                    if (_sizingLBar || _sizingRBar || onLeft || onRight)
                        ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);

                    if (_sizingRBar)
                    {
                        if (!io.MouseDown[0])
                        {
                            _sizingRBar = false;
                        }
                        else
                        {
                            float barNewWidth = Math.Max(barWidthInPixels + io.MouseDelta.X, ImSequencerViewStyleConsts.MinScrollbarWidth);
                            float barRatio = barNewWidth / barWidthInPixels;
                            _framePixelWidthTarget = _framePixelWidth = _framePixelWidth / barRatio;
                            int newVisibleFrameCount = (int)((canvasSize.X - ImSequencerViewStyleConsts.LegendWidth) / _framePixelWidthTarget);
                            int lastFrame = FirstFrame + newVisibleFrameCount;
                            if (lastFrame > GetFrameMax())
                            {
                                _framePixelWidthTarget = _framePixelWidth = (canvasSize.X - ImSequencerViewStyleConsts.LegendWidth) / (GetFrameMax() - FirstFrame);
                            }
                        }
                    }
                    else if (_sizingLBar)
                    {
                        if (!io.MouseDown[0])
                        {
                            _sizingLBar = false;
                        }
                        else
                        {
                            if (MathF.Abs(io.MouseDelta.X) > float.Epsilon)
                            {
                                float barNewWidth = Math.Max(barWidthInPixels - io.MouseDelta.X, ImSequencerViewStyleConsts.MinScrollbarWidth);
                                float barRatio = barNewWidth / barWidthInPixels;
                                float previousFramePixelWidthTarget = _framePixelWidthTarget;
                                _framePixelWidthTarget = _framePixelWidth = _framePixelWidth / barRatio;
                                int newVisibleFrameCount = (int)(visibleFrameCount / barRatio);
                                int newFirstFrame = FirstFrame + newVisibleFrameCount - visibleFrameCount;
                                newFirstFrame = Math.Clamp(newFirstFrame, GetFrameMin(), Math.Max(GetFrameMax() - visibleFrameCount, GetFrameMin()));
                                if (newFirstFrame == FirstFrame)
                                {
                                    _framePixelWidth = _framePixelWidthTarget = previousFramePixelWidthTarget;
                                }
                                else
                                {
                                    FirstFrame = newFirstFrame;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_movingScrollBar)
                        {
                            if (!io.MouseDown[0])
                            {
                                _movingScrollBar = false;
                            }
                            else
                            {
                                float framesPerPixelInBar = barWidthInPixels / visibleFrameCount;
                                FirstFrame = (int)((io.MousePos.X - _panningViewSource.X) / framesPerPixelInBar) - _panningViewFrame;
                                FirstFrame = Math.Clamp(FirstFrame, GetFrameMin(), Math.Max(GetFrameMax() - visibleFrameCount, GetFrameMin()));
                            }
                        }
                        else
                        {
                            if (scrollBarThumb.Contains(io.MousePos) && ImGui.IsMouseClicked(0) && !_movingCurrentFrame && _movingEntry == -1)
                            {
                                _movingScrollBar = true;
                                _panningViewSource = io.MousePos;
                                _panningViewFrame = -FirstFrame;
                            }
                            if (!_sizingRBar && onRight && ImGui.IsMouseClicked(0))
                                _sizingRBar = true;
                            if (!_sizingLBar && onLeft && ImGui.IsMouseClicked(0))
                                _sizingLBar = true;

                        }
                    }
                }
            }
        }

        ImGui.EndGroup();

        if (regionRect.Contains(io.MousePos))
        {
            bool overCustomDraw = false;
            foreach (var custom in customDraws)
            {
                if (custom.CustomRect.Contains(io.MousePos))
                {
                    overCustomDraw = true;
                }
            }
            if (overCustomDraw)
            {
            }
            else
            {
                //#if 0
                //            frameOverCursor = *firstFrame + (int)(visibleFrameCount * ((io.MousePos.X - (float)legendWidth - canvas_pos.X) / (canvas_size.X - legendWidth)));
                //            //frameOverCursor = max(min(*firstFrame - visibleFrameCount / 2, frameCount - visibleFrameCount), 0);

                //            /**firstFrame -= frameOverCursor;
                //            *firstFrame *= framePixelWidthTarget / framePixelWidth;
                //            *firstFrame += frameOverCursor;*/
                //            if (io.MouseWheel < -FLT_EPSILON)
                //            {
                //               *firstFrame -= frameOverCursor;
                //               *firstFrame = int(*firstFrame * 1.1f);
                //               framePixelWidthTarget *= 0.9f;
                //               *firstFrame += frameOverCursor;
                //            }

                //            if (io.MouseWheel > FLT_EPSILON)
                //            {
                //               *firstFrame -= frameOverCursor;
                //               *firstFrame = int(*firstFrame * 0.9f);
                //               framePixelWidthTarget *= 1.1f;
                //               *firstFrame += frameOverCursor;
                //            }
                //#endif
            }
        }

        if (SequencerAddDelButton(drawList, new Vector2(canvasPos.X + ImGuiEx.CalcUiScaledValue(2f), canvasPos.Y + ImGuiEx.CalcUiScaledValue(2f)), !Expanded))
            Expanded = !Expanded;

        if (delEntry != -1)
        {
            Sequence.Del(delEntry);
            if (SelectedEntry == delEntry || SelectedEntry >= Sequence.GetItemCount())
                SelectedEntry = -1;
        }

        if (dupEntry != -1)
        {
            Sequence.Duplicate(dupEntry);
        }
        return ret;
    }

    private bool SequencerAddDelButton(ImDrawListPtr drawList, Vector2 pos, bool add = true)
    {
        var io = ImGui.GetIO();
        var btnRect = new ImRect { Min = pos, Max = new Vector2(pos.X + ImGuiEx.CalcUiScaledValue(16f), pos.Y + ImGuiEx.CalcUiScaledValue(16f)) };

        bool overBtn = btnRect.Contains(io.MousePos);
        bool containedClick = overBtn && btnRect.Contains(io.MouseClickedPos[0]);
        bool clickedBtn = containedClick && io.MouseReleased[0];
        uint btnColor = overBtn ? ImSequencerViewStyleConsts.ButtonHoveredColor : ImSequencerViewStyleConsts.ButtonColor;
        if (containedClick && io.MouseDownDuration[0] > 0)
            btnRect.Expand(2f);

        float midy = pos.Y + ImGuiEx.CalcUiScaledValue(16f) / 2f - ImGuiEx.CalcUiScaledValue(0.5f);
        float midx = pos.X + ImGuiEx.CalcUiScaledValue(16f) / 2f - ImGuiEx.CalcUiScaledValue(0.5f);

        drawList.AddRect(btnRect.Min, btnRect.Max, btnColor, ImSequencerViewStyleConsts.ButtonRounding);
        drawList.AddLine(new Vector2(btnRect.Min.X + ImGuiEx.CalcUiScaledValue(3f), midy), new Vector2(btnRect.Max.X - ImGuiEx.CalcUiScaledValue(3f), midy), btnColor, 2);
        if (add)
            drawList.AddLine(new Vector2(midx, btnRect.Min.Y + ImGuiEx.CalcUiScaledValue(3f)), new Vector2(midx, btnRect.Max.Y - ImGuiEx.CalcUiScaledValue(3f)), btnColor, 2);
        return clickedBtn;
    }

    private int GetFrameMin() 
        => UseSequenceStartEnd ? Sequence.GetFrameMin() : StartFrame;

    private int GetFrameMax() 
        => UseSequenceStartEnd ? Sequence.GetFrameMax() : EndFrame;

}