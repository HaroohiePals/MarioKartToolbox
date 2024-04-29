using HaroohiePals.NitroKart.MapData;

namespace HaroohiePals.NitroKart.MapObj;

public record MkdsMapObjInfo(MkdsMapObjectId Id, string Name, string Description, string[] RequiredFiles, bool IsTimeTrialVisible = true, bool IsPathRequired = false)
{
    public override string ToString() => $"{Name}";
}