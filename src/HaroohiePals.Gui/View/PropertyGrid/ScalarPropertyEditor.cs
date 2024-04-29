using HaroohiePals.Actions;
using ImGuiNET;
using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class ScalarPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => ImGuiEx.IsScalarType(propertyInfo.PropertyType);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var rangeAttr = context.Property.PropertyInfo.GetCustomAttribute<RangeAttribute>();
        object min = rangeAttr?.Minimum ?? null;
        object max = rangeAttr?.Maximum ?? null;

        var firstSelection = context.Property.SourceObjects[0];
        var propertyValue = firstSelection.GetType().GetProperty(context.Property.Name).GetValue(firstSelection);

        var sameValues = PropertyGridUtil.AreSameValues(context.Property.SourceObjects, context.Property.Name);

        if (ImGuiEx.DragScalar(label, sameValues ? propertyValue : Convert.ChangeType(0, context.Property.Type),
                out var outProp, sameValues ? null : "", min, max))
        {
            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                Convert.ChangeType(outProp, context.Property.Type)));

            result = true;
        }

        context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        return result;
    }
}