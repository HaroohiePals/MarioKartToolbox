using System;

namespace HaroohiePals.Gui.View.Toolbar;

public class ToolbarItem
{
    public ToolbarItem(char icon, string label, Action action)
    {
        Icon = icon;
        Label = label;
        Action = action;
    }

    public char Icon;
    public string Label;
    public Action Action;
    public bool Enabled = true;
}
