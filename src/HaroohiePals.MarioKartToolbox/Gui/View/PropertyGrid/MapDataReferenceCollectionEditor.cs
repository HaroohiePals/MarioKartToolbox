using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Actions;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class MapDataReferenceCollectionEditor<T> : IPropertyEditor
    where T : IReferenceable<T>, IMapDataEntry
{
    protected MkdsMapData MapData;
    protected MapDataCollection<T> Collection;

    public MapDataReferenceCollectionEditor(MkdsMapData mapData, MapDataCollection<T> collection)
    {
        MapData = mapData;
        Collection = collection;
    }

    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(MapDataReferenceCollection<T>);

    public bool Draw(string label, PropertyEditorContext context)
    {
        if (context.Property.SourceObjects.Length > 1)
        {
            ImGui.Text("Multiple values");
            return false;
        }

        bool result = false;

        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);
        var refCollection = (MapDataReferenceCollection<T>)prop.GetValue(context.Property.SourceObjects[0]);

        T newValue;
        IEnumerable<T> filteredCollection;

        int i = 0;
        foreach (var item in refCollection)
        {
            var others = refCollection.Where(x => x != null && !x.Equals(item));

            //Change reference
            string text = item == null ? $"{typeof(T).Name} Invalid" : $"{MapData.GetEntryDisplayName(item.Target)}";

            filteredCollection = Collection.Where(x => !others.Any(y => y.Target.Equals(x)));

            bool updateAddOrClear = false;
            T targetValue = default;

            switch (MapDataReferenceEditor<T>.Draw($"{label}##[{i}]", item, text, Collection, filteredCollection, MapData, out newValue))
            {
                case MapDataReferenceEditMode.Pick:
                    //todo
                    break;
                case MapDataReferenceEditMode.UpdateAdd:
                    updateAddOrClear = true;
                    targetValue = newValue;
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
                        var targetCollection = (MapDataReferenceCollection<T>)prop.GetValue(mapDataEntry);

                        context.TempActions.Add(new SetReferenceCollectionItemAction<T>(targetCollection, item, targetValue));
                    }
                }
                context.ApplyEdits = true;
            }

            i++;
        }

        // Add dropdown

        filteredCollection = Collection.Where(x => !refCollection.Any(y => y != null && y.Target.Equals(x)));

        int count = refCollection.Count();
        int filteredCount = filteredCollection.Count();

        int collectionLimit = 3; //3 is only valid for epat/ipat/cpat. Mepa has 8, but that is handled in a different class (MgEnemyPointReferenceCollectionEditor)

        if (filteredCount == 0 || count == collectionLimit)
            return result;

        switch (MapDataReferenceEditor<T>.Draw($"{label}##[{i}]", null, "Add...", Collection, filteredCollection, MapData, out newValue))
        {
            case MapDataReferenceEditMode.UpdateAdd:
                foreach (var obj in context.Property.SourceObjects)
                {
                    if (obj is IMapDataEntry mapDataEntry)
                    {
                        prop = mapDataEntry.GetType().GetProperty(context.Property.Name);
                        var targetCollection = (MapDataReferenceCollection<T>)prop.GetValue(mapDataEntry);

                        context.TempActions.Add(new SetReferenceCollectionItemAction<T>(targetCollection, null, newValue));
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
