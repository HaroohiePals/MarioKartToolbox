#nullable enable
#nullable enable
using System;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

record ViewportSideToolbarExtraTool(string Icon, string Name, Action OnClick, 
    Func<bool>? IsVisible = null, Func<bool>? IsSelected = null);
