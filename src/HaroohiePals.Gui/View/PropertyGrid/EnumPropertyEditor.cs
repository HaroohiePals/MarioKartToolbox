using HaroohiePals.Actions;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class EnumPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo)
        => propertyInfo.PropertyType.IsEnum;

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var firstSelection = context.Property.SourceObjects[0];
        var propertyValue = firstSelection.GetType().GetProperty(context.Property.Name).GetValue(firstSelection);

        object tempEnum = propertyValue;

        if (ImGuiEx.ComboEnum(label, ref tempEnum,
                PropertyGridUtil.AreSameValues(context.Property.SourceObjects, context.Property.Name) ? null : -1))
        {
            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                tempEnum));

            result = true;
            context.ApplyEdits = true;
        }

        return result;
    }
}