using System;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

internal class TimelineManager
{
    public float Speed = 1f;
    public float FrameRate { get; private set; } = 60;
    public int CurrentFrame { get; set; } = 0;
    public int StartFrame { get; set; } = 0;
    public int EndFrame { get; set; } = 1000;
    public bool IsPlaying { get; private set; } = false;
    public bool LoopEnabled { get; set; } = false;
    //public int LoopStart { get; set; } = 0;
    //public int LoopEnd { get; set; } = 0;

    private double _fraction = 0;

    public void Update(double time)
    {
        if (!IsPlaying)
            return;

        Speed = Math.Clamp(Speed, 0, 10f);
        double frames = _fraction + time * FrameRate * Speed;

        while (frames > 0)
        {
            CurrentFrame++;

            if (LoopEnabled && CurrentFrame > EndFrame)
                CurrentFrame = StartFrame;

            frames -= 1.0;
        }

        _fraction = frames;
    }

    public void Play() => IsPlaying = true;
    public void Pause() => IsPlaying = false;
}
