namespace HaroohiePals.Gui.Viewport;

public record struct PickingResult(int GroupId, int Index, int SubIndex)
{
    public static readonly PickingResult Invalid = new(0xFF - 1, 0x7FFFF, 0x1E);

    //Subtract 1 from the Group ID to get the original value back.
    //This is done to fix an issue where Alpha = 0 pixels would be discarded on some GPUs
    public PickingResult(uint pickingId)
        : this((int)(pickingId >> 24 & 0xFF) - 1, (int)(pickingId & 0x7FFFF), (int)(pickingId >> 19 & 0x1F) - 1) { }

    public readonly bool IsInvalid => GroupId == Invalid.GroupId;
}
