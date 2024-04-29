using HaroohiePals.Actions;
using ImGuiNET;
using System.Drawing;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class ColorPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(Color);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);
        var areSameValues = PropertyGridUtil.AreSameValues(context.Property.SourceObjects, context.Property.Name);

        var val = areSameValues ? (Color)prop.GetValue(context.Property.SourceObjects[0]) : Color.White;
        var col = new System.Numerics.Vector3(val.R / 255f, val.G / 255f, val.B / 255f);
        if (ImGui.ColorEdit3(label, ref col,
                ImGuiColorEditFlags.Uint8 |
                (areSameValues ? ImGuiColorEditFlags.None : ImGuiColorEditFlags.NoInputs)))
        {
            var targetColor = Color.FromArgb((int)(col.X * 255), (int)(col.Y * 255), (int)(col.Z * 255));

            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                targetColor));

            result = true;
        }

        context.ApplyEdits = ImGui.IsItemDeactivatedAfterEdit();

        return result;
    }
}