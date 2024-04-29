using HaroohiePals.Gui;
using HaroohiePals.Gui.View.ImSequencer;
using HaroohiePals.Gui.View.Pane;
using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using ImGuiNET;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

internal class TimelinePaneView : PaneView
{
    private readonly TimelinePaneViewModel _viewModel;
    private readonly ImSequencerView _sequencer = new();

    public TimelinePaneView(TimelinePaneViewModel viewModel) : base("Timeline", new Vector2(400, 600))
    {
        _viewModel = viewModel;
        _sequencer.Sequence = _viewModel.GetSequence();
        _sequencer.Options = ImSequencerOptions.EditAll;
    }

    public override void DrawContent()
    {
        RenderTimeControls();

        int prevFrame = _viewModel.CurrentFrame;
        _sequencer.CurrentFrame = prevFrame;
        _sequencer.StartFrame = _viewModel.Context.TimelineManager.StartFrame;
        _sequencer.EndFrame = _viewModel.Context.TimelineManager.EndFrame;
        _sequencer.Draw();

        _viewModel.ChangeSelectedEntry(_sequencer.SelectedEntry);

        if (prevFrame != _sequencer.CurrentFrame)
            _viewModel.GoToFrame(_sequencer.CurrentFrame);
    }


    private void RenderTimeControls()
    {
        float scale = ImGuiEx.GetUiScale();

        float btnSize = 24 * scale;
        float spacing = 2 * scale;

        int itemCount = 5;

        float itemWidth = btnSize + spacing;
        float allItemsWidth = itemCount * itemWidth;

        float availableX = ImGui.GetContentRegionAvail().X;

        int curFrame = _viewModel.Context.TimelineManager.CurrentFrame;
        ImGui.PushItemWidth(50f * scale);
        if (ImGui.DragInt("Frame", ref curFrame, 1f, _viewModel.Context.TimelineManager.StartFrame, _viewModel.Context.TimelineManager.EndFrame))
            _viewModel.GoToFrame(curFrame);
        ImGui.SameLine();
        ImGui.DragFloat("Speed", ref _viewModel.Context.TimelineManager.Speed, 0.1f, 0f, 10f);
        ImGui.SameLine();
        int startFrame = _viewModel.Context.TimelineManager.StartFrame;
        if (ImGui.DragInt("Start", ref startFrame, 1f, -100000, _viewModel.Context.TimelineManager.EndFrame))
            _viewModel.Context.TimelineManager.StartFrame = startFrame;
        ImGui.SameLine();
        int endFrame = _viewModel.Context.TimelineManager.EndFrame;
        if (ImGui.DragInt("End", ref endFrame, 1f, _viewModel.Context.TimelineManager.StartFrame, 100000))
            _viewModel.Context.TimelineManager.EndFrame = endFrame;
        ImGui.PopItemWidth();
        ImGui.SameLine();
        bool loopEnabled = _viewModel.Context.TimelineManager.LoopEnabled;
        if (ImGui.Checkbox("Loop", ref loopEnabled))
            _viewModel.Context.TimelineManager.LoopEnabled = loopEnabled;
        ImGui.SameLine();

        int itemIndex = 0;
        ImGui.SetCursorPosX(availableX / 2f - allItemsWidth / 2f + itemIndex++ * itemWidth);
        if (ImGui.Button($"{FontAwesome6.BackwardFast}", new(btnSize)))
        {
            _viewModel.GoToStart();
        }
        ImGui.SameLine();
        ImGui.SetCursorPosX(availableX / 2f - allItemsWidth / 2f + itemIndex++ * itemWidth);
        if (ImGui.Button($"{FontAwesome6.BackwardStep}", new(btnSize)))
        {
            _viewModel.GoToFrame(curFrame - 1);
        }
        bool oldIsPlaying = _viewModel.IsPlaying;

        ImGui.SameLine();
        ImGui.SetCursorPosX(availableX / 2f - allItemsWidth / 2f + itemIndex++ * itemWidth);
        if (oldIsPlaying && ImGui.Button($"{FontAwesome6.Pause}", new(btnSize)))
            _viewModel.Pause();
        if (!oldIsPlaying && ImGui.Button($"{FontAwesome6.Play}", new(btnSize)))
            _viewModel.Play();

        ImGui.SameLine();
        ImGui.SetCursorPosX(availableX / 2f - allItemsWidth / 2f + itemIndex++ * itemWidth);
        if (ImGui.Button($"{FontAwesome6.ForwardStep}", new(btnSize)))
            _viewModel.GoToFrame(curFrame + 1);
        ImGui.SameLine();
        ImGui.SetCursorPosX(availableX / 2f - allItemsWidth / 2f + itemIndex++ * itemWidth);
        if (ImGui.Button($"{FontAwesome6.ForwardFast}", new(btnSize)))
            _viewModel.GoToEnd();
    }

}