using ImGuiNET;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Gui.View.Menu;

public class MenuView : IView
{
    public List<MenuItem> Items = new();

    public bool Draw()
    {
        var items = Items.ToList();

        HandleShortcuts(items);

        if (ImGui.BeginMainMenuBar())
        {
            DrawItems(items);

            ImGui.EndMainMenuBar();
            return true;
        }

        return false;
    }

    private static bool DrawItems(List<MenuItem> items)
    {
        foreach (var item in items)
        {
            if (item.Items != null && item.Items.Count > 0)
            {
                if (ImGui.BeginMenu(item.Text))
                {
                    item.Action?.Invoke();

                    if (item.Items != null && item.Items.Count > 0)
                        DrawItems(item.Items.ToList());

                    ImGui.EndMenu();
                }
            }
            else
            {
                if (item.Separator)
                {
                    ImGui.Separator();
                }
                else if (ImGui.MenuItem(item.Text, item.Shortcut?.ToString(), item.Selected))
                {
                    item.Action?.Invoke();
                }
            }
        }

        return true;
    }

    private static void HandleShortcuts(List<MenuItem> items)
    {
        foreach (var item in items)
        {
            if (item.Items != null && item.Items.Count > 0)
            {
                HandleShortcuts(item.Items);
            }
            else if (item.Shortcut.HasValue && item.Shortcut.Value.IsPressed())
            {
                item.Action?.Invoke();
            }
        }
    }
}