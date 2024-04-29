using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HaroohiePals.Gui.View.PropertyGrid;

public class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
{
    public bool Equals(PropertyInfo x, PropertyInfo y)
    {
        return x.Name == y.Name && x.PropertyType == y.PropertyType;
    }

    public int GetHashCode([DisallowNull] PropertyInfo obj)
    {
        return obj.Name.GetHashCode() ^ obj.PropertyType.GetHashCode();
    }
}
