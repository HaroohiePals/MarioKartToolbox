using HaroohiePals.Gui.View.Menu;
using System.Collections.Generic;

namespace HaroohiePals.Gui.View;

public abstract class WindowContentView : IView
{
    /// <summary>
    /// Menu entries to merge on top of the base menu entries
    /// </summary>
    public virtual IReadOnlyCollection<MenuItem> MenuItems => new List<MenuItem>();

    public abstract bool Draw();
    public virtual void Update(UpdateArgs args) { }
}