using HaroohiePals.Actions;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class ArrayPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType.IsArray;

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var rangeAttr = context.Property.PropertyInfo.GetCustomAttribute<RangeAttribute>();
        object min = rangeAttr?.Minimum ?? null;
        object max = rangeAttr?.Maximum ?? null;

        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);
        var array = (Array)prop.GetValue(context.Property.SourceObjects[0]);

        int length = GetShortestArrayLength(context.Property);

        if (length == -1)
            return false;

        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        float wFull = ImGui.CalcItemWidth();
        float wItem = (float)Math.Max(1.0f, Math.Floor((wFull - spacing * (length - 1)) / length));
        float wItemLast = (float)Math.Max(1.0f, Math.Floor(wFull - (wItem + spacing) * (length - 1)));

        // var batchAction = new BatchAction();
        var actions = new List<IAction>();

        for (int i = 0; i < length; i++)
        {
            var element = array.GetValue(i);
            var elementType = element?.GetType();

            if (i > 0)
                ImGui.SameLine(0, spacing);
            ImGui.PushItemWidth(i == length - 1 ? wItemLast : wItem);

            if (element != null)
            {
                bool sameValues = AreSameArrayValues(context.Property, i);

                if (ImGuiEx.DragScalar($"{label}[{i}]", sameValues ? element : Convert.ChangeType(0, elementType),
                        out var outProp, sameValues ? null : "", min, max))
                {
                    Array.ForEach(context.Property.SourceObjects, x =>
                    {
                        var srcProp = x.GetType().GetProperty(context.Property.Name);
                        var srcArr = (Array)srcProp.GetValue(x);

                        actions.Add(new SetArrayItemAction(srcArr, i, outProp));
                    });
                    result = true;
                }

                if (!context.ApplyEdits)
                    context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();
            }
        }

        if (actions.Count > 0)
            context.TempActions.AddRange(actions);

        return result;
    }

    private static int GetShortestArrayLength(PropertyGridItem property)
        => PropertyGridUtil.GetShortestArrayLength(property.SourceObjects, property.Name);

    private static bool AreSameArrayValues(PropertyGridItem property, int index)
        => PropertyGridUtil.AreSameArrayValues(property.SourceObjects, property.Name, index);
}