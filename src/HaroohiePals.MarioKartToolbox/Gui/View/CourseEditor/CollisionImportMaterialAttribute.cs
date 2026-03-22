using System.ComponentModel;
using HaroohiePals.MarioKartToolbox.KCollision;

namespace HaroohiePals.MarioKartToolbox.Gui.View.CourseEditor;

public class CollisionImportMaterialAttribute
{
    public MkdsKclPrismAttribute Attribute;
    public readonly string MaterialName;

    public CollisionImportMaterialAttribute(string materialName, MkdsKclPrismAttribute attribute)
    {
        MaterialName = materialName;
        Attribute = attribute;
        Type = attribute.Type; //update the implied flags
    }

    [Category("Settings")] public bool Enabled { get; set; } = true;

    [Category("Settings")]
    public MkdsCollisionType Type
    {
        get => Attribute.Type;
        set
        {
            Attribute.Type = value;
            var info = MkdsCollisionConsts.GetColTypeInfo(value);
            Attribute.IsFloor = info.IsFloor;
            Attribute.IsWall = info.IsWall;
            Attribute.IgnoreDrivers = info.IgnoreDrivers;
            Attribute.IgnoreItems = info.IgnoreItems;
        }
    }

    [Category("Settings")]
    public MkdsCollisionVariant Variant
    {
        get => Attribute.Variant;
        set => Attribute.Variant = value;
    }

    [Category("Flags"), DisplayName("Shadow on Bottom Map")]
    public bool Map2dShadow
    {
        get => Attribute.Map2dShadow;
        set => Attribute.Map2dShadow = value;
    }

    [Category("Settings"), DisplayName("Light"), Description("ID of the color inside Stag")]
    public MkdsCollisionLightId LightId
    {
        get => Attribute.LightId;
        set => Attribute.LightId = value;
    }

    [Category("Flags"), DisplayName("Unused Flag")]
    public bool UnusedFlag
    {
        get => Attribute.UnusedFlag;
        set => Attribute.UnusedFlag = value;
    }

    public override string ToString()
    {
        string collisionType = Attribute.Type.ToString();
        string disabled = Enabled ? "" : "(X)";

        try
        {
            collisionType += $" - {MkdsCollisionConsts.GetVariantName(Attribute)}";
        }
        catch
        {
            // ignored
        }

        return $"{disabled}{MaterialName} ({collisionType})";
    }
}