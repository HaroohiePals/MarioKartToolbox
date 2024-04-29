using System;

namespace HaroohiePals.Gui.View.ImSequencer;

[Flags]
public enum ImSequencerOptions
{
    EditNone = 0,
    EditStartEnd = 1 << 1,
    ChangeFrame = 1 << 3,
    Add = 1 << 4,
    Delete = 1 << 5,
    CopyPaste = 1 << 6,
    EditAll = EditStartEnd | ChangeFrame
}
