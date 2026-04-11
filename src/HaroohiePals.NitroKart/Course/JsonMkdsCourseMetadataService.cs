#nullable enable
using System;
using System.Text;
using HaroohiePals.IO.Archive;
using HaroohiePals.NitroKart.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HaroohiePals.NitroKart.Course;

public class JsonMkdsCourseMetadataService : IMkdsCourseMetadataService
{
    private const string COURSE_METADATA_FILENAME = "metadata.json";
    private const string COURSE_METADATA_PATH = $"/{COURSE_METADATA_FILENAME}";

    private readonly JsonSerializerSettings _jsonSettings = new()
    {
        Formatting = Formatting.Indented,
        Converters = { new Vector2dJsonConverter(), new StringEnumConverter() }
    };

    public MkdsCourseMetadata Load(Archive mainArchive)
    {
        var defaultValue = CreateDefault();
        
        if (!mainArchive.ExistsFile(COURSE_METADATA_PATH))
            return defaultValue;

        try
        {
            byte[] data = mainArchive.GetFileData(COURSE_METADATA_PATH);
            string json = Encoding.UTF8.GetString(data);
            var metadata = JsonConvert.DeserializeObject<MkdsCourseMetadata>(json, _jsonSettings);

            if (metadata is null)
                return defaultValue;
            
            metadata.GlobalMapSettings ??= defaultValue.GlobalMapSettings;
            metadata.LocalMapSettings ??= defaultValue.LocalMapSettings;
            
            return metadata;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    public void Save(Archive mainArchive, MkdsCourseMetadata metadata)
    {
        string json = JsonConvert.SerializeObject(metadata, _jsonSettings);
        byte[] data = Encoding.UTF8.GetBytes(json);
        mainArchive.SetFileData(COURSE_METADATA_PATH, data);
    }

    private static MkdsCourseMetadata CreateDefault() => new()
    {
        LocalMapSettings = new MkdsLocalMapSettings(),
        GlobalMapSettings = new MkdsGlobalMapSettings(),
    };
}
