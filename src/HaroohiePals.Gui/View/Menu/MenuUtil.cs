using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Gui.View.Menu;

public static class MenuUtil
{
    public static IReadOnlyList<MenuItem> Merge(this IReadOnlyCollection<MenuItem> first, IReadOnlyCollection<MenuItem> other)
    {
        var merged = new List<MenuItem>();

        foreach (var itemA in first)
        {
            var cloned = itemA.Clone();
            merged.Add(cloned);
            var itemB = other.FirstOrDefault(x => x.Text == itemA.Text);
            if (itemB != null)
            {
                if (cloned.Items == null)
                    cloned.Items = new();
                cloned.Items.AddRange(itemB.Clone().Items);
            }
        }

        return merged;
    }
}
