using System;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class PropertyGridItem
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public bool IsReadOnly { get; set; } = false;
    public Type Type { get; set; }
    public PropertyInfo PropertyInfo { get; set; }
    public object[] SourceObjects { get; set; }
    public PropertyGridView NestedPropertyGridView { get; set; }
    public IPropertyEditor Editor { get; set; }
}
