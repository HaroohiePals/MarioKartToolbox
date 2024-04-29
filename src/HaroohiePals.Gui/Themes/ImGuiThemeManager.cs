using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaroohiePals.Gui.Themes;

public static class ImGuiThemeManager
{
    private const string DEFAULT_THEME = "Dark";
    private const string THEMES_PATH = "Themes";

    public static List<ImGuiTheme> Themes = new();
    public static string CurrentTheme { get; private set; }

    public static void Init()
    {
        if (Directory.Exists(THEMES_PATH))
        {
            Themes.Clear();

            foreach (var file in Directory.GetFiles(THEMES_PATH, "*.json"))
            {
                try
                {
                    var theme = ImGuiTheme.FromJson(File.ReadAllText(file));
                    Themes.Add(theme);
                }
                catch
                {

                }
            }

            Apply(DEFAULT_THEME);
        }
    }

    public static void Apply(ImGuiTheme theme) => theme.Apply();

    public static void Apply(string name)
    {
        var theme = Themes.FirstOrDefault(x => x.Name == name);

        if (theme != null)
            Apply(theme);
    }
}
