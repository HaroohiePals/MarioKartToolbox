using HaroohiePals.Actions;
using ImGuiNET;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class StringPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo)
        => propertyInfo.PropertyType == typeof(string);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var firstSelection = context.Property.SourceObjects[0];
        var propertyValue = firstSelection.GetType().GetProperty(context.Property.Name).GetValue(firstSelection);

        string tempStr = PropertyGridUtil.AreSameValues(context.Property.SourceObjects, context.Property.Name)
            ? (string)propertyValue ?? string.Empty
            : string.Empty;

        if (ImGui.InputText(label, ref tempStr, 1000))
        {
            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                tempStr));

            result = true;
        }

        context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        return result;
    }
}