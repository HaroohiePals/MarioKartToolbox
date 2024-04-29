using HaroohiePals.Gui;
using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class MapDataReferenceEditor<T> : IPropertyEditor
    where T : IReferenceable<T>, IMapDataEntry
{
    protected MkdsMapData MapData;
    protected MapDataCollection<T> Collection;

    public MapDataReferenceEditor(MkdsMapData mapData, MapDataCollection<T> collection)
    {
        MapData = mapData;
        Collection = collection;
    }

    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(Reference<T>) && Collection != null;

    public bool Draw(string label, PropertyEditorContext context)
    {
        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);

        var firstValue = (Reference<T>)prop.GetValue(context.Property.SourceObjects[0]);

        bool sameValues = AreSameValues(context.Property.SourceObjects, context.Property.Name);

        string text;
        if (sameValues)
        {
            if (firstValue == null)
                text = "Select...";
            else
            {
                string id;
                if (firstValue is UnresolvedBinaryMapDataReference<T> unBinRef)
                    id = "unresolved " + unBinRef.UnresolvedId;
                else if (firstValue is UnresolvedXmlMapDataReference<T> unXmlRef)
                    id = "unresolved " + unXmlRef.UnresolvedId;
                else
                    id = Collection.IndexOf(firstValue.Target).ToString();

                text = $"{MapData.GetEntryDisplayName(firstValue.Target)}";
            }
        }
        else
            text = "Multiple values";

        switch (Draw(label, sameValues ? firstValue : null, text, Collection, MapData, out var newValue))
        {
            case MapDataReferenceEditMode.Clear:
            case MapDataReferenceEditMode.UpdateAdd:
                foreach (var obj in context.Property.SourceObjects)
                {
                    if (obj is IMapDataEntry mapDataEntry)
                    {
                        prop = mapDataEntry.GetType().GetProperty(context.Property.Name);
                        var oldRef = (Reference<T>)prop.GetValue(mapDataEntry);

                        context.TempActions.Add(new SetMapDataEntryReferenceAction<T>(mapDataEntry, context.Property.Name,
                            oldRef, newValue));
                    }
                }

                context.ApplyEdits = true;
                break;
            case MapDataReferenceEditMode.Pick:
                // todo
                break;
        }

        return false;
    }

    /// <summary>
    /// Check if all values of a property contained in the selection are the same
    /// </summary>
    protected static bool AreSameValues(object[] sourceObjects, string propertyName)
    {
        if (sourceObjects.Length == 0)
            return false;
        if (sourceObjects.Length == 1)
            return true;

        var firstRef =
            (Reference<T>)sourceObjects[0].GetType().GetProperty(propertyName).GetValue(sourceObjects[0]);

        object firstItem = firstRef != null ? firstRef.Target : null;

        return sourceObjects.Skip(1).All(x =>
        {
            var secondRef = (Reference<T>)x.GetType().GetProperty(propertyName).GetValue(x);
            object secondItem = secondRef != null ? secondRef.Target : null;

            if (firstItem == null || secondItem == null)
                return false;

            return secondItem.Equals(firstItem);
        });
    }

    internal static MapDataReferenceEditMode Draw(string label, Reference<T> item, string shownText,
        MapDataCollection<T> collection, MkdsMapData mapData, out T newValue)
        => Draw(label, item, shownText, collection, collection, mapData, out newValue);

    internal static MapDataReferenceEditMode Draw(string label, Reference<T> item, string shownText,
        MapDataCollection<T> collection, IEnumerable<T> filteredCollection, MkdsMapData mapData, out T newValue)
        => MapDataReferenceEditor.Draw(label, item, shownText, new MapDataCollection<T>[] { collection },
            filteredCollection, mapData, out newValue);
}

internal static class MapDataReferenceEditor
{
    /// <summary>
    /// Generic draw function
    /// </summary>
    /// <param name="item"></param>
    /// <param name="filteredCollection"></param>
    /// <param name="mapData"></param>
    /// <param name="index"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    internal static MapDataReferenceEditMode Draw<T>(string label, Reference<T> item, string shownText,
        IEnumerable<MapDataCollection<T>> collections, IEnumerable<T> filteredCollection, MkdsMapData mapData,
        out T newValue)
        where T : IReferenceable<T>, IMapDataEntry
    {
        var result = MapDataReferenceEditMode.None;
        newValue = default;

        bool showClear = item != null;

        var regionAvail = ImGui.GetContentRegionAvail();

        float scale = ImGuiEx.GetUiScale();

        ImGui.PushItemWidth(regionAvail.X - (showClear ? 64 : 32) * scale);
        if (ImGui.BeginCombo($"## {label}_Select", shownText))
        {
            foreach (var collection in collections)
            {
                foreach (var entry in collection)
                {
                    if (filteredCollection != null && !filteredCollection.Contains(entry))
                        continue;

                    bool isSelected = item != null && entry.Equals(item.Target);

                    string itemText = mapData.GetEntryDisplayName(entry) ??
                                      $"{typeof(T).Name} #{collection.IndexOf(entry)}";

                    if (ImGui.Selectable(itemText, isSelected))
                    {
                        result = MapDataReferenceEditMode.UpdateAdd;
                        newValue = entry;
                    }

                    // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.Button($"{FontAwesome6.EyeDropper}## {label}_Pick"))
        {
            result = MapDataReferenceEditMode.Pick;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted("Select a point from the viewport");
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }

        if (showClear)
        {
            ImGui.SameLine();
            if (ImGui.Button($"{FontAwesome6.Trash}## {label}_Delete"))
            {
                result = MapDataReferenceEditMode.Clear;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted("Clear the reference");
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        return result;
    }
}