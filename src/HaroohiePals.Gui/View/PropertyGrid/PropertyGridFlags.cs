using System;

namespace HaroohiePals.Gui.View.PropertyGrid;

[Flags]
public enum PropertyGridFlags : int
{
    None = 0,
    ShowCategories = 1,
    SortProperty = 2,
    SortPropertyDescending = 4,
    SortCategory = 8,
    SortCategoryDescending = 16,
}