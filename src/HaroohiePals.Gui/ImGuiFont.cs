namespace HaroohiePals.Gui;

public sealed record ImGuiFont(byte[] Data)
{
    public static readonly ImGuiFont Default = new ImGuiFont(Resources.Fonts.Roboto_Regular);
}
