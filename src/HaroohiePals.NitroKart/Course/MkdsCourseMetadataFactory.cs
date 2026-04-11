#nullable enable
using System;
using System.Text;
using HaroohiePals.IO.Archive;
using HaroohiePals.NitroKart.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HaroohiePals.NitroKart.Course;

public class MkdsCourseMetadataFactory
{
    private const string COURSE_METADATA_FILENAME = "metadata.json";
    public const string COURSE_METADATA_PATH = $"/{COURSE_METADATA_FILENAME}";

    public static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        Converters = { new Vector2dJsonConverter(), new StringEnumConverter() }
    };

    public MkdsCourseMetadata Create(Archive mainArchive)
    {
        if (!mainArchive.ExistsFile(COURSE_METADATA_PATH))
            return CreateDefault();

        try
        {
            byte[] data = mainArchive.GetFileData(COURSE_METADATA_PATH);
            string json = Encoding.UTF8.GetString(data);
            var metadata = JsonConvert.DeserializeObject<MkdsCourseMetadata>(json, JsonSettings);

            return metadata ?? CreateDefault();
        }
        catch (Exception)
        {
            return CreateDefault();
        }
    }

    private static MkdsCourseMetadata CreateDefault() => new()
    {
        LocalMapSettings = new MkdsLocalMapSettings()
    };
}