using HaroohiePals.Actions;
using HaroohiePals.Graphics;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class Rgb555ArrayPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(Rgb555[]);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);
        var array = (Rgb555[])prop.GetValue(context.Property.SourceObjects[0]);

        int length = GetShortestArrayLength(context.Property);

        if (length == -1)
            return false;

        //float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        //float wFull = ImGui.CalcItemWidth();
        //float wItem = (float)System.Math.Max(1.0f, System.Math.Floor((wFull - spacing * (length - 1)) / length));
        //float wItemLast = (float)System.Math.Max(1.0f, System.Math.Floor(wFull - (wItem + spacing) * (length - 1)));

        var actions = new List<IAction>();

        for (int i = 0; i < length; i++)
        {
            var val = array[i];
            var col = new System.Numerics.Vector3(val.R / 31f, val.G / 31f, val.B / 31f);
            //if (i > 0)
            //    ImGui.SameLine(0, spacing);
            //ImGui.PushItemWidth(i == length - 1 ? wItemLast : wItem);

            bool sameValues = AreSameArrayValues(context.Property, i);

            if (ImGui.ColorEdit3($"{label}[{i}]", ref col, ImGuiColorEditFlags.Uint8))
            {
                Array.ForEach(context.Property.SourceObjects, x =>
                {
                    var targetColor = new Rgb555((int)(col.X * 31), (int)(col.Y * 31), (int)(col.Z * 31));

                    var srcProp = x.GetType().GetProperty(context.Property.Name);
                    var srcArr = (Array)srcProp.GetValue(x);

                    actions.Add(new SetArrayItemAction(srcArr, i, targetColor));
                });
                result = true;
            }

            if (!context.ApplyEdits)
                context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();
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