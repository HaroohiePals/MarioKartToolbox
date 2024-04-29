using static HaroohiePals.NitroKart.MapData.MkdsMapObjectId;

namespace HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;

public static class MkdsMobjSettingsFactory
{
    public static MkdsMobjSettings Construct(MkdsMapObjectId objectId, MkdsMobjSettings source = null)
        => objectId switch
        {
            Itembox      => new ItemboxSettings(source),
            MoveItembox  => new MoveItemboxSettings(source),
            Gear         => new GearSettings(source),
            TestCylinder => new RotatingCylinderSettings(source),
            RotaryRoom   => new RotatingCylinderSettings(source),
            RotaryBridge => new RotatingCylinderSettings(source),
            Cow          => new CowSettings(source),
            Dossun       => new DossunSettings(source),
            Snowman      => new SnowmanSettings(source),
            MoveTree     => new MoveTreeSettings(source),
            MkdEfBurner  => new MkdEfBurnerSettings(source),
            Flipper      => new FlipperSettings(source),
            Fireball2    => new Fireball2Settings(source),
            IronBall     => new IronBallSettings(source),
            Sanbo        => new SanboSettings(source),
            _            => new MkdsMobjSettings(source)
        };
}