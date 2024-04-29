namespace HaroohiePals.MarioKartToolbox.KCollision;

public record MkdsColTypeInfo(MkdsCollisionType Type, bool IsFloor, bool IsWall, bool IgnoreDrivers,
    bool IgnoreItems, string[] Variants);
