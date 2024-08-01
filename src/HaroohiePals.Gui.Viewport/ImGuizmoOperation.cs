namespace HaroohiePals.Gui.Viewport;

[Flags]
enum ImGuizmoOperation : uint
{
    TranslateX = (1u << 0),
    TranslateY = (1u << 1),
    TranslateZ = (1u << 2),
    RotateX = (1u << 3),
    RotateY = (1u << 4),
    RotateZ = (1u << 5),
    RotateScreen = (1u << 6),
    ScaleX = (1u << 7),
    ScaleY = (1u << 8),
    ScaleZ = (1u << 9),
    Bounds = (1u << 10),
    ScaleXUniversal = (1u << 11),
    ScaleYUniversal = (1u << 12),
    ScaleZUniversal = (1u << 13),

    Translate = TranslateX | TranslateY | TranslateZ,
    Rotate = RotateX | RotateY | RotateZ | RotateScreen,
    Scale = ScaleX | ScaleY | ScaleZ,
    ScaleUniversal = ScaleXUniversal | ScaleYUniversal | ScaleZUniversal, // universal
    Universal = Translate | Rotate | ScaleUniversal
};
