#nullable enable
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.Course;

public class MkdsCourseMetadata
{
    public required MkdsLocalMapSettings LocalMapSettings { get; set; }
    public Dictionary<string, string> AdditionalProperties { get; set; } = new();
}