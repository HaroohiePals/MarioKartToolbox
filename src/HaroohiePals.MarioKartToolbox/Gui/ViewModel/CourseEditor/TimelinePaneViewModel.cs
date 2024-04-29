using HaroohiePals.Gui;
using HaroohiePals.Gui.View.ImSequencer;
using ImGuiNET;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

class TimelinePaneViewModel
{
    public ICourseEditorContext Context { get; }
    private CameraIntroSequence _cameraIntroSequence;
    public bool IsPlaying => Context.TimelineManager.IsPlaying;
    public int CurrentFrame => Context.TimelineManager.CurrentFrame;
    private int _lastSelectedEntry = -1;

    public TimelinePaneViewModel(ICourseEditorContext context)
    {
        Context = context;
        _cameraIntroSequence = new CameraIntroSequence(Context.Course.MapData?.Cameras, Context.ActionStack);
    }

    public void GoToFrame(int frameNumber) => Context.TimelineManager.CurrentFrame = frameNumber;
    public void GoToStart() => GoToFrame(Context.TimelineManager.StartFrame);
    public void GoToEnd() => GoToFrame(Context.TimelineManager.EndFrame);

    public void Play() => Context.TimelineManager.Play();
    public void Pause() => Context.TimelineManager.Pause();

    public ISequence GetSequence() => _cameraIntroSequence;
    public void ChangeSelectedEntry(int index)
    {
        if (index != -1 && _lastSelectedEntry != index)
            Context.SceneObjectHolder.SetSelection(_cameraIntroSequence.GetIntroCameraByOrderIndex(index));
        _lastSelectedEntry = index;
    }
}