using System.ComponentModel;

namespace HaroohiePals.MarioKartToolbox.KCollision;

public abstract class MkdsKclAttributeAdapter
{
    public abstract MkdsKclPrismAttribute Attribute { get; set; }

    [Category("Settings")]
    public MkdsCollisionType Type
    {
        get => Attribute.Type;
        set
        {
            var x = Attribute;
            x.Type = value;
            Attribute = x;
        }
    }

    [Category("Settings")]
    public MkdsCollisionVariant Variant
    {
        get => Attribute.Variant;
        set
        {
            var x = Attribute;
            x.Variant = value;
            Attribute = x;
        }
    }

    [Category("Flags"), DisplayName("Shadow on Bottom Map")]
    public bool Map2dShadow
    {
        get => Attribute.Map2dShadow;
        set
        {
            var x = Attribute;
            x.Map2dShadow = value;
            Attribute = x;
        }
    }

    [Category("Settings"), DisplayName("Light"), Description("ID of the color inside Stag")]
    public MkdsCollisionLightId LightId
    {
        get => Attribute.LightId;
        set
        {
            var x = Attribute;
            x.LightId = value;
            Attribute = x;
        }
    }

    [Category("Flags"), DisplayName("Ignore Drivers")]
    public bool IgnoreDrivers
    {
        get => Attribute.IgnoreDrivers;
        set
        {
            var x = Attribute;
            x.IgnoreDrivers = value;
            Attribute = x;
        }
    }

    [Category("Flags"), DisplayName("Ignore Items")]
    public bool IgnoreItems
    {
        get => Attribute.IgnoreItems;
        set
        {
            var x = Attribute;
            x.IgnoreItems = value;
            Attribute = x;
        }
    }

    [Category("Flags"), DisplayName("Is Wall")]
    public bool IsWall
    {
        get => Attribute.IsWall;
        set
        {
            var x = Attribute;
            x.IsWall = value;
            Attribute = x;
        }
    }

    [Category("Flags"), DisplayName("Is Floor")]
    public bool IsFloor
    {
        get => Attribute.IsFloor;
        set
        {
            var x = Attribute;
            x.IsFloor = value;
            Attribute = x;
        }
    }
}
