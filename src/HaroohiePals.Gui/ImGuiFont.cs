namespace HaroohiePals.Gui;

public sealed record ImGuiFont(byte[] Data, float Size)
{
    public static readonly ImGuiFont Default = new ImGuiFont(Resources.Fonts.Roboto_Regular, 15f);
    public static readonly ImGuiFont DefaultIconFont = new ImGuiFont(Resources.Fonts.fa_solid_900, 16f);
}
