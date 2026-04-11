#nullable enable
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.Course;

public class MkdsCourseMetadata
{
    public MkdsLocalMapSettings? LocalMapSettings { get; set; }
    public MkdsGlobalMapSettings? GlobalMapSettings { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; } = new();
}