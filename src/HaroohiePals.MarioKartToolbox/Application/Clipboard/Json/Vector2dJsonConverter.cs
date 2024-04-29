#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class Vector2dJsonConverter : JsonConverter<Vector2d>
{
    private const string PROPERTY_X_NAME = "x";
    private const string PROPERTY_Y_NAME = "y";

    public override void WriteJson(JsonWriter writer, Vector2d value, JsonSerializer serializer)
    {
        var jObj = new JObject
        {
            [PROPERTY_X_NAME] = value.X,
            [PROPERTY_Y_NAME] = value.Y
        };
        jObj.WriteTo(writer);
    }

    public override Vector2d ReadJson(JsonReader reader, Type objectType, Vector2d existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObj = JObject.Load(reader);
        return new Vector2d(
            jObj.Value<double>(PROPERTY_X_NAME),
            jObj.Value<double>(PROPERTY_Y_NAME));
    }
}