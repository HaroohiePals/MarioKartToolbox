#nullable enable
namespace HaroohiePals.NitroKart.Rom;

public class MkdsRomProject
{
    public required string Name { get; set; }
    public NdsRomInfo RomInfo { get; set; } = new();
    public string[] IgnoreFilePatterns { get; set; } = [];
    public uint Version { get; set; } = 0;
}
