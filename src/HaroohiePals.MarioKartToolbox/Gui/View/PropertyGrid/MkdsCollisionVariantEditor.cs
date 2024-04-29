using HaroohiePals.Actions;
using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.KCollision;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class MkdsCollisionVariantEditor : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(MkdsCollisionVariant);

    private MkdsCollisionType GetColType(object obj)
    {
        if (obj is MkdsKclAttributeAdapter adapter)
            return adapter.Type;

        if (obj is MaterialAttribute materialAttribute)
            return materialAttribute.Type;

        throw new Exception();
    }

    private MkdsCollisionVariant GetColVariant(object obj)
    {
        if (obj is MkdsKclAttributeAdapter adapter)
            return adapter.Variant;

        if (obj is MaterialAttribute materialAttribute)
            return materialAttribute.Variant;

        throw new Exception();
    }

    public bool Draw(string label, PropertyEditorContext context)
    {
        bool sameType = false;
        bool sameVariant = false;
        int variantIndex = -1;

        try
        {
            var firstType = GetColType(context.Property.SourceObjects[0]);
            var firstVariant = GetColVariant(context.Property.SourceObjects[0]);

            sameType = context.Property.SourceObjects.All(obj => GetColType(obj) == firstType);
            sameVariant = context.Property.SourceObjects.All(obj => GetColVariant(obj) == firstVariant);
            if (sameVariant)
                variantIndex = (int)firstVariant;
        }
        catch { }

        bool result = false;

        var comboItems = new List<string>();
        // var first        = (MkdsKclAttributeAdapter)context.Property.SourceObjects[0];
        // int variantIndex = sameVariant ? (int)firstVariant : -1;


        if (sameType)
        {
            var variants = MkdsCollisionConsts.GetVariants(GetColType(context.Property.SourceObjects[0]));

            for (int i = 0; i < 8; i++)
            {
                string variantName = i >= variants.Length ? $"{i} - None" : $"{i} - {variants[i]}";
                comboItems.Add(variantName);
            }
        }
        else
        {
            comboItems.AddRange(new[] { "0", "1", "2", "3", "4", "5", "6", "7" });
        }

        if (ImGui.Combo(label, ref variantIndex, comboItems.ToArray(), comboItems.Count))
        {
            context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects, context.Property.Name,
                (MkdsCollisionVariant)variantIndex));

            result = true;
            context.ApplyEdits = true;
        }

        return result;
    }
}