#nullable enable

using HaroohiePals.Gui.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Gui.View.Menu;

public class MenuItem
{
    public MenuItem() { }

    public MenuItem(string text, IReadOnlyCollection<MenuItem> items)
    {
        Text = text;
        Items = items.ToList();
    }

    public MenuItem(string text, Action? action = null, KeyBinding? shortcut = null)
    {
        Text = text;
        Action = action;
        Shortcut = shortcut;
    }

    public string Text = "";
    public KeyBinding? Shortcut = null;
    public bool Selected = false;
    public bool Separator = false;
    public Action? Action = null;

    public List<MenuItem> Items = [];

    public MenuItem Clone() => new()
    {
        Text = Text,
        Action = Action,
        Items = Items?.Select(x => x.Clone()).ToList() ?? [],
        Selected = Selected,
        Separator = Separator,
        Shortcut = Shortcut
    };
}