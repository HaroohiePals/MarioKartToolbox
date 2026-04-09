using HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

sealed class MinimapViewportViewModel(ICourseEditorContext context)
{
    public ICourseEditorContext Context { get; } = context;

    public void ExtendLeft()
    {
        Console.WriteLine("Todo");
    }

    public void ExtendRight()
    {
        Console.WriteLine("Todo");
    }

    public void ExtendDown()
    {
        Console.WriteLine("Todo");
    }

    public void ExtendUp()
    {
        Console.WriteLine("Todo");
    }
}