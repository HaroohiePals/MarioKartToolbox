using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public interface IPropertyEditor
{
    bool IsEditorForProperty(PropertyInfo propertyInfo);

    bool Draw(string label, PropertyEditorContext context);
}