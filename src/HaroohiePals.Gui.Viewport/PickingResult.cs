namespace HaroohiePals.Gui.Viewport;

public record struct PickingResult(int GroupId, int Index, int SubIndex)
{
    public static readonly PickingResult Invalid = new(-1, 0, 0);
    public readonly bool IsInvalid => GroupId == Invalid.GroupId;
}