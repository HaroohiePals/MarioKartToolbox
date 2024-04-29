#nullable enable
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class MapDataContractResolver : DefaultContractResolver
{
    public MapDataContractResolver()
    {
        NamingStrategy = new CamelCaseNamingStrategy();
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var properties = base.CreateProperties(type, memberSerialization);

        if (type == typeof(MkdsMapObject))
        {
            properties = properties.Where(p => p.UnderlyingName != nameof(MkdsMapObject.ObjectIdUshort)).ToList();
        }
        else if (type == typeof(MkdsArea))
        {
            properties = properties.Where(p => p.UnderlyingName != nameof(MkdsArea.AreaTypeByte)).ToList();
        }

        return properties;
    }
}