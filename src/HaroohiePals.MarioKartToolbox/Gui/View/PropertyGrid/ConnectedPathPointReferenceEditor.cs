using HaroohiePals.Gui.View.PropertyGrid;
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.Actions;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.Extensions;
using HaroohiePals.NitroKart.MapData.Intermediate;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.PropertyGrid;

class ConnectedPathPointReferenceEditor<TPoint, TPath> : IPropertyEditor
    where TPoint : IReferenceable<TPoint>, IMapDataEntry
    where TPath : ConnectedPath<TPath, TPoint>, new()
{
    protected MkdsMapData MapData;
    protected MapDataCollection<TPath> Collection;

    public ConnectedPathPointReferenceEditor(MkdsMapData mapData, MapDataCollection<TPath> collection)
    {
        MapData = mapData;
        Collection = collection;
    }

    public bool IsEditorForProperty(PropertyInfo propertyInfo) 
        => propertyInfo.PropertyType == typeof(Reference<TPoint>) && Collection != null;

    /// <summary>
    /// Check if all values of a property contained in the selection are the same
    /// </summary>
    private static bool AreSameValues(object[] sourceObjects, string propertyName)
    {
        if (sourceObjects.Length == 0)
            return false;
        if (sourceObjects.Length == 1)
            return true;

        var firstRef =
            (Reference<TPoint>)sourceObjects[0].GetType().GetProperty(propertyName).GetValue(sourceObjects[0]);

        object firstItem = firstRef != null ? firstRef.Target : null;

        return sourceObjects.Skip(1).All(x =>
        {
            var secondRef = (Reference<TPoint>)x.GetType().GetProperty(propertyName).GetValue(x);
            object secondItem = secondRef != null ? secondRef.Target : null;

            if (firstItem == null || secondItem == null)
                return false;

            return secondItem.Equals(firstItem);
        });
    }

    public bool Draw(string label, PropertyEditorContext context)
    {
        var prop = context.Property.SourceObjects[0].GetType().GetProperty(context.Property.Name);

        var firstValue = (Reference<TPoint>)prop.GetValue(context.Property.SourceObjects[0]);

        bool sameValues = AreSameValues(context.Property.SourceObjects, context.Property.Name);

        var text = sameValues
            ? firstValue == null ? "Select..." : MapData.GetEntryDisplayName(firstValue.Target)
            : "Multiple values";


        switch (MapDataReferenceEditor.Draw(label, sameValues ? firstValue : null, text, Collection.Select(x => x.Points),
                    null, MapData, out var newValue))
        {
            case MapDataReferenceEditMode.Clear:
            case MapDataReferenceEditMode.UpdateAdd:
                foreach (var obj in context.Property.SourceObjects)
                {
                    if (obj is IMapDataEntry mapDataEntry)
                    {
                        prop = mapDataEntry.GetType().GetProperty(context.Property.Name);
                        var oldRef = (Reference<TPoint>)prop.GetValue(mapDataEntry);

                        context.TempActions.Add(new SetMapDataEntryReferenceAction<TPoint>(mapDataEntry, context.Property.Name,
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
}
