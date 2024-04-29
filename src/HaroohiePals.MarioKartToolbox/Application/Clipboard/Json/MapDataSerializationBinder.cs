#nullable enable
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class MapDataSerializationBinder : ISerializationBinder
{
    private static readonly IReadOnlyDictionary<Type, string> TypeToStringMapping = new Dictionary<Type, string>
    {
        [typeof(MkdsMapObject)] = "obji",
        [typeof(MkdsPath)] = "path",
        [typeof(MkdsPathPoint)] = "poit",
        [typeof(MkdsRespawnPoint)] = "ktpj",
        [typeof(MkdsStartPoint)] = "ktps",
        [typeof(MkdsKartPoint2d)] = "ktp2",
        [typeof(MkdsKartPointMission)] = "ktpm",
        [typeof(MkdsCannonPoint)] = "ktpc",
        [typeof(MkdsCheckPointPath)] = "cpat",
        [typeof(MkdsCheckPoint)] = "cpoi",
        [typeof(MkdsItemPath)] = "ipat",
        [typeof(MkdsItemPoint)] = "ipoi",
        [typeof(MkdsEnemyPath)] = "epat",
        [typeof(MkdsEnemyPoint)] = "epoi",
        [typeof(MkdsMgEnemyPath)] = "mepa",
        [typeof(MkdsMgEnemyPoint)] = "mepo",
        [typeof(MkdsArea)] = "area",
        [typeof(MkdsCamera)] = "came"
    };

    private static readonly IReadOnlyDictionary<string, Type> StringToTypeMapping =
        TypeToStringMapping.ToDictionary(x => x.Value, x => x.Key);

    public Type BindToType(string? assemblyName, string typeName)
    {
        return StringToTypeMapping[typeName];
    }

    public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
    {
        assemblyName = null;
        typeName = TypeToStringMapping[serializedType];
    }
}