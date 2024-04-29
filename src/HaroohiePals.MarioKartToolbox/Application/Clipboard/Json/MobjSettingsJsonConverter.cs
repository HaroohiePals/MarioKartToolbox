#nullable enable
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Application.Clipboard.Json;

sealed class MobjSettingsJsonConverter : JsonConverter<MkdsMobjSettings>
{
    public override MkdsMobjSettings? ReadJson(JsonReader reader, Type objectType, MkdsMobjSettings? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        var settings = new MkdsMobjSettings();
        var token = JArray.Load(reader);
        var settingsArray = token.Values<ushort>().ToArray();
        if (settingsArray.Length != settings.Settings.Length)
            throw new Exception();
        settingsArray.CopyTo(settings.Settings, 0);
        return settings;
    }

    public override void WriteJson(JsonWriter writer, MkdsMobjSettings? value, JsonSerializer serializer)
    {
        value ??= new MkdsMobjSettings();
        JArray.FromObject(value.Settings).WriteTo(writer);
    }
}