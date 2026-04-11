using HaroohiePals.Actions;
using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.ViewModel.CourseEditor;

sealed class MinimapViewportViewModel(ICourseEditorContext context)
{
    public ICourseEditorContext Context { get; } = context;
    public bool ShowGlobalMap { get; private set; } = false;
    public bool ShowTranslucent { get; private set; } = false;

    public void ExtendLeft() => ExtendLocalMap(MinimapLocalMapExtensionDirection.Left);

    public void ExtendRight() => ExtendLocalMap(MinimapLocalMapExtensionDirection.Right);

    public void ExtendDown() => ExtendLocalMap(MinimapLocalMapExtensionDirection.Down);

    public void ExtendUp() => ExtendLocalMap(MinimapLocalMapExtensionDirection.Up);
    public bool IsExtendButtonVisible() => !ShowGlobalMap;
    public void ToggleGlobalMap() => ShowGlobalMap = !ShowGlobalMap;
    public void ToggleTranslucent() => ShowTranslucent = !ShowTranslucent;

    private void ExtendLocalMap(MinimapLocalMapExtensionDirection direction)
    {
        var mapSettings = Context.Course.Metadata.LocalMapSettings!;

        var topLeft = mapSettings.TopLeft;
        var bottomRight = mapSettings.BottomRight;

        double width = bottomRight.X - topLeft.X;
        double height = bottomRight.Y - topLeft.Y;

        var offset = direction switch
        {
            MinimapLocalMapExtensionDirection.Left => new Vector2d(-width, 0),
            MinimapLocalMapExtensionDirection.Right => new Vector2d(width, 0),
            MinimapLocalMapExtensionDirection.Up => new Vector2d(0, -height),
            MinimapLocalMapExtensionDirection.Down => new Vector2d(0, height),
            _ => throw new ArgumentOutOfRangeException(nameof(direction)),
        };

        Context.ActionStack.Add(new BatchAction(
        [
            mapSettings.SetPropertyAction(x => x.ExtendedTopLeft, topLeft + offset),
            mapSettings.SetPropertyAction(x => x.ExtendedBottomRight, bottomRight + offset),
            mapSettings.SetPropertyAction(x => x.Mode, MkdsLocalMapMode.Extended)
        ]));
    }
}
