#nullable enable
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class ClipboardMapDataReferenceResolverJsonConverter : JsonConverter
{
    public override bool CanWrite => false;

    public IReadOnlyDictionary<string, IMapDataEntry> LocalMapping { get; set; } =
        new Dictionary<string, IMapDataEntry>();

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        if (reader.Value is not string refValue)
            return null;

        if (refValue.StartsWith("local"))
        {
            if (LocalMapping.TryGetValue(refValue, out var target))
                return CreateWeakMapDataReference(target);

            return null;
        }

        return CreateUnresolvedWeakJsonMapDataReference(GetTargetType(objectType), int.Parse(refValue));
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override bool CanConvert(Type objectType)
    {
        var genericObjectType = objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType;
        while (genericObjectType != typeof(Reference<>) && objectType.BaseType != null)
        {
            objectType = objectType.BaseType;
            genericObjectType = objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType;
        }

        return genericObjectType == typeof(Reference<>) &&
               objectType.GetGenericArguments()[0].GetInterface(nameof(IMapDataEntry)) is not null;
    }

    private object CreateWeakMapDataReference(object target)
    {
        return Activator.CreateInstance(typeof(WeakMapDataReference<>).MakeGenericType(target.GetType()), target)!;
    }

    private object CreateUnresolvedWeakJsonMapDataReference(Type targetType, int index)
    {
        return Activator.CreateInstance(typeof(UnresolvedWeakJsonMapDataReference<>).MakeGenericType(targetType),
            index)!;
    }

    private Type GetTargetType(Type referenceType)
    {
        var genericReferenceType =
            referenceType.IsGenericType ? referenceType.GetGenericTypeDefinition() : referenceType;
        while (genericReferenceType != typeof(Reference<>) && referenceType.BaseType != null)
        {
            referenceType = referenceType.BaseType;
            genericReferenceType =
                referenceType.IsGenericType ? referenceType.GetGenericTypeDefinition() : referenceType;
        }

        return referenceType.GetGenericArguments()[0];
    }
}