using HaroohiePals.Actions;
using System.Collections.Generic;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class PropertyEditorContext
{
    public PropertyGridView Widget;
    public PropertyGridItem Property;
    public List<IAction> TempActions;
    public bool ApplyEdits = false;
}
