using HaroohiePals.Actions;
using ImGuiNET;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class BooleanPropertyEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(bool);

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool result = false;

        var firstSelection = context.Property.SourceObjects[0];
        var propertyValue = firstSelection.GetType().GetProperty(context.Property.Name).GetValue(firstSelection);

        bool sameValues = PropertyGridUtil.AreSameValues(context.Property.SourceObjects, context.Property.Name);

        bool tempBool = sameValues ? (bool)propertyValue : true;

        if (!sameValues)
            igPushItemFlag(ImGuiItemFlags_MixedValue);

        if (ImGui.Checkbox(label, ref tempBool))
        {
            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                tempBool));

            result = true;
            context.ApplyEdits = true;
        }

        if (!sameValues)
            igPopItemFlag();

        return result;
    }


    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    private static extern byte igPushItemFlag(int flags);

    [DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
    private static extern byte igPopItemFlag();

    private const int ImGuiItemFlags_MixedValue = 1 << 6;
}