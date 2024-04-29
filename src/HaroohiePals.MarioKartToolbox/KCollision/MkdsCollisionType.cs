namespace HaroohiePals.MarioKartToolbox.KCollision;

//Move this to nitro kart lib later

public enum MkdsCollisionType : int
{
    Road,
    SlipperyRoad,
    WeakOffRoad,
    OffRoad,
    SoundTrigger,
    HeavyOffRoad,
    SlipperyRoad2,
    BoostPad,
    Wall,
    InvisibleWall, //ignored by cameras and items
    OutOfBounds,
    FallBoundary,
    JumpPad,
    RoadNoDrivers, //for out of bounds areas unreachable by drivers, but where items might drop
    WallNoDrivers,
    CannonActivator,
    EdgeWall,
    FallsWater,
    BoostPadMinSpeed, //forces a minimum speed to ensure certain jumps can be made
    Loop,
    SpecialRoad,
    Wall3,
    ForceRecalculateRoute
}
