namespace HaroohiePals.MarioKartToolbox.KCollision;

public static class MkdsCollisionConsts
{
    public static readonly MkdsColTypeInfo[] ColTypes =
    {
        new(MkdsCollisionType.Road, true, false, false, false, new[]
            { "Asphalt", "Dirt", "Stone", "Concrete", "Wood Board", "Snow/Grass", "Metal Grid", "Iron Plate" }),
        new(MkdsCollisionType.SlipperyRoad, true, false, false, false, new[]
            { "Sand", "Dirt", "Grass", "Water", "Snow" }),
        new(MkdsCollisionType.WeakOffRoad, true, false, false, false, new[]
            { "Sand", "Water", "Snow", "Dirt" }),
        new(MkdsCollisionType.OffRoad, true, false, false, false, new[]
            { "Dirt", "Mud", "Grass", "Sand", "Cloud", "Snow" }),
        new(MkdsCollisionType.SoundTrigger, false, false, false, true, new[]
            { "Trigger 0", "Trigger 1", "Trigger 2", "Trigger 3", "Trigger 4" }),
        new(MkdsCollisionType.HeavyOffRoad, true, false, false, false, new[]
            { "Dirt", "Mud", "Grass", "Sand", "Flowerbed" }),
        new(MkdsCollisionType.SlipperyRoad2, true, false, false, false, new[]
            { "Ice", "Mud" }),
        new(MkdsCollisionType.BoostPad, true, false, false, false, new[]
            { "Normal", "Rainbow", "Pinball Normal", "Dirt + Pinball Cannon Enter" }),
        new(MkdsCollisionType.Wall, false, true, false, false, new[]
            { "Concrete", "Rock", "Metal", "Wood", "Bush", "No Sound", "Rope", "Ice/Metal Grid" }),
        new(MkdsCollisionType.InvisibleWall, false, true, false, true, new[]
            { "Concrete", "Rock", "Metal", "Wood", "Bush", "No Sound", "Rope", "Ice/Metal Grid" }),
        new(MkdsCollisionType.OutOfBounds, true, false, false, false, new[]
            { "Dirt", "Sand", "Dirt (No Effects)", "Snow", "Water" }),
        new(MkdsCollisionType.FallBoundary, false, false, false, true, new[]
            { "Air Fall", "Water", "Lava" }),
        new(MkdsCollisionType.JumpPad, true, false, false, false, new[]
        {
            "Low Jump", "Medium Jump", "High Jump", "No Jump (Dirt Sound Effect)", "Highest Jump",
            "Extremely High Jump (Lose control)", "Higher Extreme Jump (Lose control)",
            "Highest Extreme Jump (Lose control)"
        }),
        new(MkdsCollisionType.RoadNoDrivers, true, false, true, false, new[]
            { "Concrete", "Rock", "Metal", "Wood", "Bush", "No Sound", "Rope", "Ice/Metal Grid" }),
        new(MkdsCollisionType.WallNoDrivers, false, true, true, false, new[]
            { "Concrete", "Rock", "Metal", "Wood", "Bush", "No Sound", "Rope", "Ice/Metal Grid" }),
        new(MkdsCollisionType.CannonActivator, false, false, false, false, new[]
            { "Cannon 0", "Cannon 1", "Cannon 2", "Cannon 3", "Cannon 4", "Cannon 5", "Cannon 6", "Cannon 7" }),
        new(MkdsCollisionType.EdgeWall, false, true, false, false, new[]
            { "Concrete", "Rock", "Metal", "Wood", "Bush", "No Sound", "Rope", "Ice/Metal Grid" }),
        new(MkdsCollisionType.FallsWater, true, false, false, false, new[]
            { "Id 0", "Id 1", "Id 2" }),
        new(MkdsCollisionType.BoostPadMinSpeed, true, false, false, false, new[]
            { "Normal", "Rainbow", "Pinball Normal", "Dirt + Pinball Cannon Enter" }),
        new(MkdsCollisionType.Loop, true, false, false, false, new[]
            { "Rainbow/Boost Pad", "Rainbow/Boost Pad" }),
        new(MkdsCollisionType.SpecialRoad, true, false, false, false, new[]
            { "Dirt/Sand", "Carpet", "Rainbow", "Grass", "Stairs", "Sand", "Dirt" }),
        new(MkdsCollisionType.Wall3, false, true, false, false, new[]
            { "Concrete", "Rock", "Metal", "Wood", "Bush", "No Sound", "Rope", "Ice/Metal Grid" }),
        new(MkdsCollisionType.ForceRecalculateRoute, false, false, false, true, new[]
            { "Id 0", "Id 1", "Id 2", "Id 3", "Id 4", "Id 5", "Id 6", "Id 7" })
    };

    public static string GetVariantName(MkdsKclPrismAttribute attribute)
        => GetVariantName(attribute.Type, attribute.Variant);

    public static string GetVariantName(MkdsCollisionType type, MkdsCollisionVariant variant)
        => ColTypes[(int)type].Variants[(int)variant];

    public static string[] GetVariants(MkdsCollisionType type)
        => ColTypes[(int)type].Variants;

    public static MkdsColTypeInfo GetColTypeInfo(MkdsCollisionType type)
        => ColTypes[(int)type];
}
