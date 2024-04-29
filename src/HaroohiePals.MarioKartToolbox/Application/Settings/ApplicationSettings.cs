namespace HaroohiePals.MarioKartToolbox.Application.Settings;

record struct ApplicationSettings()
{
    public DiscordSettings Discord = new();
    public GameSettings Game = new();
    public AppearanceSettings Appearance = new();
    public ViewportSettings Viewport = new();
    public KeyBindingSettings KeyBindings = new();
    public CourseEditorSettings CourseEditor = new();
}