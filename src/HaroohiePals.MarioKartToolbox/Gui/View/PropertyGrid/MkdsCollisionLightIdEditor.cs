using HaroohiePals.Actions;
using HaroohiePals.Graphics;
using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class MkdsCollisionLightIdEditor : IPropertyEditor
{
    private MkdsStageInfo _stageInfo;

    public MkdsCollisionLightIdEditor(MkdsStageInfo stageInfo = null)
    {
        _stageInfo = stageInfo;
    }

    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(MkdsCollisionLightId);

    private MkdsCollisionLightId GetLightId(object obj)
    {
        if (obj is MkdsKclAttributeAdapter adapter)
            return adapter.LightId;

        if (obj is MaterialAttribute materialAttribute)
            return materialAttribute.LightId;

        throw new Exception();
    }

    public bool Draw(string label, PropertyEditorContext context)
    {
        var first = GetLightId(context.Property.SourceObjects[0]);
        bool sameValue = context.Property.SourceObjects.All(obj => GetLightId(obj) == first);
        bool result = false;

        string[] comboItems = new[] { "Color 0", "Color 1", "Color 2", "Color 3" };
        int itemIndex = sameValue ? (int)first : -1;

        if (_stageInfo != null && sameValue)
        {
            var color = Rgb555.White;
            if ((int)first >= 0 && (int)first <= 3)
                color = _stageInfo.KclLightColors[(int)first];

            var col = new Vector3(color.R / 31f, color.G / 31f, color.B / 31f);
            ImGui.ColorEdit3(label + "Color", ref col,
                ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.NoTooltip |
                ImGuiColorEditFlags.NoInputs);

            ImGui.SameLine();
        }

        if (ImGui.BeginCombo(label + "new", itemIndex == -1 ? "" : comboItems[itemIndex]))
        {
            for (int i = 0; i < 4; i++)
            {
                if (ImGui.Selectable(comboItems[i] + "##" + label + "_item_" + i, i == itemIndex))
                {
                    itemIndex = i;
                    context.TempActions.Add(new SetPropertyAction(context.Property.SourceObjects,
                        context.Property.Name,
                        (MkdsCollisionLightId)itemIndex));

                    result = true;
                    context.ApplyEdits = true;
                }

                ImGui.SameLine();
                var col = new Vector4(_stageInfo.KclLightColors[i].R / 31f, _stageInfo.KclLightColors[i].G / 31f, _stageInfo.KclLightColors[i].B / 31f, 1);
                ImGui.ColorButton(label + "Color_" + i, col, ImGuiColorEditFlags.None, new Vector2(15));
            }

            ImGui.EndCombo();
        }

        return result;
    }
}