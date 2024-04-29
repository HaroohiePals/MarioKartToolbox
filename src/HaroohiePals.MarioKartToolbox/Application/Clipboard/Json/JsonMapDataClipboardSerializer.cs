#nullable enable
using HaroohiePals.MarioKart.MapData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class JsonMapDataClipboardSerializer : IMapDataClipboardSerializer
{
    private const string MKTOOLBOX_MAPDATA_ID = "712bfaaa-3336-45c6-b4c0-c73b9207542d";
    private const string PROPERTY_DATA_TYPE_ID_NAME = "dataTypeId";
    private const string PROPERTY_UNIQUE_ID_NAME = "uniqueId";
    private const string PROPERTY_ENTRIES_NAME = "entries";

    public MapDataClipboardSerializationResult Serialize(IEnumerable<IMapDataEntry> entries)
    {
        var entryArray = entries.ToArray();
        string uniqueId = GetUniqueId();
        var serializer = CreateSerializer();
        var referenceConverter = new ClipboardMapDataReferenceSerializerJsonConverter(entryArray);
        serializer.Converters.Add(referenceConverter);
        string result;
        using (var stringWriter = new StringWriter())
        {
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartObject();
                {
                    new JProperty(PROPERTY_DATA_TYPE_ID_NAME, MKTOOLBOX_MAPDATA_ID).WriteTo(writer);
                    new JProperty(PROPERTY_UNIQUE_ID_NAME, uniqueId).WriteTo(writer);
                    writer.WritePropertyName(PROPERTY_ENTRIES_NAME);
                    serializer.Serialize(writer, entryArray);
                }
                writer.WriteEndObject();
            }

            result = stringWriter.ToString();
        }

        return new MapDataClipboardSerializationResult(result, uniqueId,
            referenceConverter.LocalMapping.ToDictionary(x => x.Value, x => x.Key));
    }

    public IReadOnlyCollection<IMapDataEntry> Deserialize(string clipboardString,
        MapDataClipboardSerializationResult? lastClipboardSerializationResult)
    {
        try
        {
            var serializer = CreateSerializer();
            var referenceConverter = new ClipboardMapDataReferenceResolverJsonConverter();
            serializer.Converters.Add(referenceConverter);
            using (var stringReader = new StringReader(clipboardString))
            using (var reader = new JsonTextReader(stringReader))
            {
                reader.Read();
                reader.Read();

                if (reader.Value is not PROPERTY_DATA_TYPE_ID_NAME)
                    return Array.Empty<IMapDataEntry>();

                reader.Read();

                if (reader.Value is not MKTOOLBOX_MAPDATA_ID)
                    return Array.Empty<IMapDataEntry>();

                reader.Read();

                if (reader.Value is PROPERTY_UNIQUE_ID_NAME)
                {
                    reader.Read();
                    if (lastClipboardSerializationResult is not null &&
                        reader.Value as string == lastClipboardSerializationResult.UniqueId)
                    {
                        referenceConverter.LocalMapping = lastClipboardSerializationResult.LocalMapping;
                    }

                    reader.Read();
                }

                if (reader.Value is not PROPERTY_ENTRIES_NAME)
                    return Array.Empty<IMapDataEntry>();

                reader.Read();

                var entries = serializer.Deserialize<IMapDataEntry[]>(reader);

                if (entries is null)
                    return Array.Empty<IMapDataEntry>();

                var resolver = new WeakJsonMapDataReferenceResolver(entries);
                foreach (var mapDataEntry in entries)
                {
                    mapDataEntry.ResolveReferences(resolver);
                }

                return entries;
            }
        }
        catch
        {
            return Array.Empty<IMapDataEntry>();
        }
    }

    private string GetUniqueId()
    {
        return Guid.NewGuid().ToString();
    }

    private JsonSerializer CreateSerializer()
    {
        var serializer = new JsonSerializer
        {
            ContractResolver = new MapDataContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            SerializationBinder = new MapDataSerializationBinder()
        };
        serializer.Converters.Add(new Vector2dJsonConverter());
        serializer.Converters.Add(new Vector3dJsonConverter());
        serializer.Converters.Add(new MobjSettingsJsonConverter());
        return serializer;
    }
}