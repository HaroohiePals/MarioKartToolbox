#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class Vector3dJsonConverter : JsonConverter<Vector3d>
{
    private const string PROPERTY_X_NAME = "x";
    private const string PROPERTY_Y_NAME = "y";
    private const string PROPERTY_Z_NAME = "z";

    public override void WriteJson(JsonWriter writer, Vector3d value, JsonSerializer serializer)
    {
        var jObj = new JObject
        {
            [PROPERTY_X_NAME] = value.X,
            [PROPERTY_Y_NAME] = value.Y,
            [PROPERTY_Z_NAME] = value.Z
        };
        jObj.WriteTo(writer);
    }

    public override Vector3d ReadJson(JsonReader reader, Type objectType, Vector3d existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObj = JObject.Load(reader);
        return new Vector3d(
            jObj.Value<double>(PROPERTY_X_NAME),
            jObj.Value<double>(PROPERTY_Y_NAME),
            jObj.Value<double>(PROPERTY_Z_NAME));
    }
}