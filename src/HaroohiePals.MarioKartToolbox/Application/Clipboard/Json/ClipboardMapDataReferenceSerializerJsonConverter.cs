#nullable enable
using HaroohiePals.IO.Reference;
using HaroohiePals.MarioKart.MapData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class ClipboardMapDataReferenceSerializerJsonConverter : JsonConverter
{
    private readonly IMapDataEntry[] _clipboardEntries;
    private readonly Dictionary<IMapDataEntry, string> _localMapping = new();

    private int _localRefId = 0;

    public override bool CanRead => false;

    public IReadOnlyDictionary<IMapDataEntry, string> LocalMapping => _localMapping;

    public ClipboardMapDataReferenceSerializerJsonConverter(IMapDataEntry[] clipboardEntries)
    {
        _clipboardEntries = clipboardEntries;
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
        JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is null)
        {
            writer.WriteNull();
            return;
        }

        var targetProperty = value.GetType().GetProperty("Target");
        if (targetProperty is null)
            throw new Exception();
        if (targetProperty.GetValue(value) is not IMapDataEntry target)
        {
            writer.WriteNull();
            return;
        }

        int index = Array.IndexOf(_clipboardEntries, target);
        string refString;
        if (index < 0)
        {
            if (!_localMapping.TryGetValue(target, out refString!))
            {
                refString = GetNextLocalRefString();
                _localMapping[target] = refString;
            }
        }
        else
        {
            refString = index.ToString();
        }

        writer.WriteValue(refString);
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

    private string GetNextLocalRefString()
    {
        return "local_" + _localRefId++;
    }
}