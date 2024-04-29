using HaroohiePals.Actions;
using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class Vector3PropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(Vector3d);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var rangeAttr = context.Property.PropertyInfo.GetCustomAttribute<RangeAttribute>();
        object min = rangeAttr?.Minimum ?? null;
        object max = rangeAttr?.Maximum ?? null;

        int components = 3;

        float spacing = ImGui.GetStyle().ItemInnerSpacing.X;
        float wFull = ImGui.CalcItemWidth();
        float wItem = (float)Math.Max(1.0f,
            Math.Floor((wFull - spacing * (components - 1)) / components));
        float wItemLast =
            (float)Math.Max(1.0f, Math.Floor(wFull - (wItem + spacing) * (components - 1)));

        AreSameValues(context.Property, out bool sameX, out bool sameY, out bool sameZ);

        Vector3d vec = (0, 0, 0);
        foreach (var obj in context.Property.SourceObjects)
        {
            var prop = obj.GetType().GetProperty(context.Property.Name);
            vec += (Vector3d)prop.GetValue(obj);
        }
        vec /= context.Property.SourceObjects.Count();

        //vec = (sameX ? vec.X : context.Property., sameY ? vec.Y : 0, sameZ ? vec.Z : 0);

        var actions = new List<IAction>();

        ImGui.PushItemWidth(wItem);
        if (ImGuiEx.DragScalar($"{label}_X", vec.X, out var outPropX, sameX ? null : "", min, max))
        {
            Array.ForEach(context.Property.SourceObjects, x =>
            {
                var curProp = x.GetType().GetProperty(context.Property.Name);

                var realValue = (Vector3d)curProp.GetValue(x);

                realValue.X = (double)outPropX;

                //curProp.SetValue(x, realValue);
                actions.Add(new SetPropertyAction(x, context.Property.Name, realValue));
            });
            result = true;
        }

        if (!context.ApplyEdits)
            context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        ImGui.SameLine(0, spacing);
        ImGui.PushItemWidth(wItem);
        if (ImGuiEx.DragScalar($"{label}_Y", vec.Y, out var outPropY, sameY ? null : "", min, max))
        {
            Array.ForEach(context.Property.SourceObjects, x =>
            {
                var curProp = x.GetType().GetProperty(context.Property.Name);

                var realValue = (Vector3d)curProp.GetValue(x);

                realValue.Y = (double)outPropY;

                //curProp.SetValue(x, realValue);
                actions.Add(new SetPropertyAction(x, context.Property.Name, realValue));
            });
            result = true;
        }

        if (!context.ApplyEdits)
            context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        ImGui.SameLine(0, spacing);
        ImGui.PushItemWidth(wItemLast);
        if (ImGuiEx.DragScalar($"{label}_Z", vec.Z, out var outPropZ, sameZ ? null : "", min, max))
        {
            Array.ForEach(context.Property.SourceObjects, x =>
            {
                var curProp = x.GetType().GetProperty(context.Property.Name);

                var realValue = (Vector3d)curProp.GetValue(x);

                realValue.Z = (double)outPropZ;

                //curProp.SetValue(x, realValue);
                actions.Add(new SetPropertyAction(x, context.Property.Name, realValue));
            });
            result = true;
        }

        if (!context.ApplyEdits)
            context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        if (actions.Count > 0)
            context.TempActions.AddRange(actions);

        return result;
    }

    /// <summary>
    /// Check which components of Vec3 are different inside the entire selection
    /// </summary>
    private static void AreSameValues(PropertyGridItem property, out bool sameX, out bool sameY, out bool sameZ)
    {
        if (property.SourceObjects.Length == 0)
        {
            sameX = sameY = sameZ = false;
            return;
        }

        if (property.SourceObjects.Length == 1)
        {
            sameX = sameY = sameZ = true;
            return;
        }

        sameX = property.SourceObjects.Skip(1).All(x =>
        {
            double value = ((Vector3d)x.GetType().GetProperty(property.Name).GetValue(x)).X;
            return value.Equals(((Vector3d)property.SourceObjects[0].GetType().GetProperty(property.Name)
                .GetValue(property.SourceObjects[0])).X);
        });
        sameY = property.SourceObjects.Skip(1).All(x =>
        {
            double value = ((Vector3d)x.GetType().GetProperty(property.Name).GetValue(x)).Y;
            return value.Equals(((Vector3d)property.SourceObjects[0].GetType().GetProperty(property.Name)
                .GetValue(property.SourceObjects[0])).Y);
        });
        sameZ = property.SourceObjects.Skip(1).All(x =>
        {
            double value = ((Vector3d)x.GetType().GetProperty(property.Name).GetValue(x)).Z;
            return value.Equals(((Vector3d)property.SourceObjects[0].GetType().GetProperty(property.Name)
                .GetValue(property.SourceObjects[0])).Z);
        });
    }
}