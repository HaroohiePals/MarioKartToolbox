using HaroohiePals.Actions;
using HaroohiePals.Graphics;
using ImGuiNET;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class Rgb555PropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(Rgb555);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);
        var areSameValues = PropertyGridUtil.AreSameValues(context.Property.SourceObjects, context.Property.Name);

        var val = areSameValues ? (Rgb555)prop.GetValue(context.Property.SourceObjects[0]) : Rgb555.White;
        var col = new System.Numerics.Vector3(val.R / 31f, val.G / 31f, val.B / 31f);
        if (ImGui.ColorEdit3(label, ref col,
                ImGuiColorEditFlags.Uint8 |
                (areSameValues ? ImGuiColorEditFlags.None : ImGuiColorEditFlags.NoInputs)))
        {
            var targetColor = new Rgb555((int)(col.X * 31), (int)(col.Y * 31), (int)(col.Z * 31));

            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                targetColor));

            result = true;
        }

        context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        return result;
    }
}