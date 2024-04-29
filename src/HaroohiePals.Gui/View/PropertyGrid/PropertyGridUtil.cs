using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

static class PropertyGridUtil
{
    public static IEnumerable<PropertyInfo> SelectUniqueProperties(object[] sourceObjects)
    {
        var lists = sourceObjects.Select(x => x.GetType().GetProperties()).ToList();

        if (lists.Count == 0)
            return null;

        var result = lists[0].AsEnumerable();

        foreach (var list in lists.Skip(1))
        {
            result = result.Intersect(list, new PropertyInfoEqualityComparer());
        }

        return result;
    }

    /// <summary>
    /// Check if all values of a property contained in the selection are the same
    /// </summary>
    public static bool AreSameValues(object[] sourceObjects, string propertyName)
    {
        if (sourceObjects.Length == 0)
            return false;
        if (sourceObjects.Length == 1)
            return true;

        var firstItem = sourceObjects[0].GetType().GetProperty(propertyName).GetValue(sourceObjects[0]);

        return sourceObjects.Skip(1).All(x =>
        {
            var secondItem = x.GetType().GetProperty(propertyName).GetValue(x);

            if (firstItem == null || secondItem == null)
                return false;

            return secondItem.Equals(firstItem);
        });
    }

    /// <summary>
    /// Check if the array elements at some index of all selected arrays are the same
    /// </summary>
    public static bool AreSameArrayValues(object[] sourceObjects, string propertyName, int index)
    {
        if (sourceObjects.Length == 0)
            return false;
        if (sourceObjects.Length == 1)
            return true;

        var firstItem = ((Array)sourceObjects[0].GetType().GetProperty(propertyName).GetValue(sourceObjects[0])).GetValue(index);

        return sourceObjects.Skip(1).All(x =>
        {
            var secondItem = ((Array)x.GetType().GetProperty(propertyName).GetValue(x)).GetValue(index);

            if (firstItem == null || secondItem == null)
                return false;

            return secondItem.Equals(firstItem);
        });
    }

    public static int GetShortestArrayLength(object[] sourceObjects, string propertyName) 
        => sourceObjects.Min(x => ((Array)x.GetType().GetProperty(propertyName).GetValue(x))?.Length) ?? -1;
}
