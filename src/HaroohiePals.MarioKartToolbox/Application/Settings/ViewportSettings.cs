using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct ViewportSettings()
{
    public IReadOnlyDictionary<string, IReadOnlyDictionary<RenderGroupVisibilityManager.VisibleEntity,
        RenderGroupVisibilityManager.VisibilityType>> VisibleEntities =
        new Dictionary<string, IReadOnlyDictionary<RenderGroupVisibilityManager.VisibleEntity,
            RenderGroupVisibilityManager.VisibilityType>>();

    public bool IntelRenderWorkaround = false;
    public bool ShowToolCameraHint = true;

    public ViewportColorSettings Colors = new();
}