namespace HaroohiePals.Gui;

public sealed record ImGuiFont(byte[] FontData)
{
    public static readonly ImGuiFont Default = new ImGuiFont(Resources.Fonts.Roboto_Regular);
}
