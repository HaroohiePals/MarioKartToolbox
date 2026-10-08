using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class MgEnemyPointReferenceCollectionEditor(MkdsMapData mapData, MapDataCollection<MkdsMgEnemyPath> collection)
    : IPropertyEditor
{
    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(MapDataReferenceCollection<MkdsMgEnemyPoint>);

    public bool Draw(string label, PropertyEditorContext context)
    {
        if (context.Property.SourceObjects.Length > 1)
        {
            ImGui.Text("Multiple values");
            return false;
        }

        bool result = false;

        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);
        var refCollection = (MapDataReferenceCollection<MkdsMgEnemyPoint>)prop.GetValue(context.Property.SourceObjects[0]);

        MkdsMgEnemyPoint newValue;
        IEnumerable<MkdsMgEnemyPoint> filteredCollection = null;

        int i = 0;
        foreach (var item in refCollection)
        {
            var others = refCollection.Where(x => !x.Equals(item));

            //Change reference
            string text = mapData.GetEntryDisplayName(item.Target);

            //filteredCollection = Collection.Where(x => !others.Any(y => y.Target.Equals(x)));

            bool updateAddOrClear = false;
            MkdsMgEnemyPoint targetValue = null;

            switch (MapDataReferenceEditor.Draw($"{label}##[{i}]", item, text, collection.Select(x => x.Points), filteredCollection, mapData, out newValue))
            {
                case MapDataReferenceEditMode.Pick:
                    //todo
                    break;
                case MapDataReferenceEditMode.UpdateAdd:
                    updateAddOrClear = true;
                    //targetValue = newValue;
                    break;
                case MapDataReferenceEditMode.Clear:
                    updateAddOrClear = true;
                    break;
            }

            if (updateAddOrClear)
            {
                foreach (var obj in context.Property.SourceObjects)
                {
                    if (obj is IMapDataEntry mapDataEntry)
                    {
                        prop = mapDataEntry.GetType().GetProperty(context.Property.Name);
                        var targetCollection = (MapDataReferenceCollection<MkdsMgEnemyPoint>)prop.GetValue(mapDataEntry);

                        context.TempActions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(targetCollection, item, targetValue));
                    }
                }
                context.ApplyEdits = true;
            }

            i++;
        }

        // Add dropdown

        int count = refCollection.Count();
        int collectionLimit = MkdsMapDataConsts.MAX_MEPA_LINKS_COUNT;

        if (count == collectionLimit)
            return result;

        switch (MapDataReferenceEditor.Draw($"{label}##[{i}]", null, "Add...", collection.Select(x => x.Points), filteredCollection, mapData, out newValue))
        {
            case MapDataReferenceEditMode.UpdateAdd:
                foreach (var obj in context.Property.SourceObjects)
                {
                    if (obj is IMapDataEntry mapDataEntry)
                    {
                        prop = mapDataEntry.GetType().GetProperty(context.Property.Name);
                        var targetCollection = (MapDataReferenceCollection<MkdsMgEnemyPoint>)prop.GetValue(mapDataEntry);

                        context.TempActions.Add(new SetReferenceCollectionItemAction<MkdsMgEnemyPoint>(targetCollection, null, newValue));
                    }
                }
                context.ApplyEdits = true;

                result = true;
                break;
            case MapDataReferenceEditMode.Pick:
                //todo
                break;
        }

        return result;
    }
}
