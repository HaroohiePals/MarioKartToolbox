namespace HaroohiePals.MarioKartToolbox.KCollision;

public struct MkdsKclPrismAttribute
{
    private ushort _value;

    private MkdsKclPrismAttribute(ushort value)
        => _value = (ushort)(value & ~1u);

    private int GetBits(int shift, int mask)
    {
        //todo: Helper function
        return _value >> shift & mask;
    }

    private void SetBits(int value, int shift, int mask)
    {
        //todo: Helper function
        _value = (ushort)(_value & (ushort)~(mask << shift));
        _value = (ushort)(_value | (value & mask) << shift);
    }

    public bool Map2dShadow
    {
        get => GetBits(1, 1) == 1;
        set => SetBits(value ? 1 : 0, 1, 1);
    }

    public MkdsCollisionLightId LightId
    {
        get => (MkdsCollisionLightId)GetBits(2, 3);
        set => SetBits((int)value, 2, 3);
    }

    public bool IgnoreDrivers
    {
        get => GetBits(4, 1) == 1;
        set => SetBits(value ? 1 : 0, 4, 1);
    }

    public MkdsCollisionVariant Variant
    {
        get => (MkdsCollisionVariant)GetBits(5, 7);
        set => SetBits((int)value, 5, 7);
    }

    public MkdsCollisionType Type
    {
        get => (MkdsCollisionType)GetBits(8, 0x1F);
        set => SetBits((int)value, 8, 0x1F);
    }

    public bool IgnoreItems
    {
        get => GetBits(13, 1) == 1;
        set => SetBits(value ? 1 : 0, 13, 1);
    }

    public bool IsWall
    {
        get => GetBits(14, 1) == 1;
        set => SetBits(value ? 1 : 0, 14, 1);
    }

    public bool IsFloor
    {
        get => GetBits(15, 1) == 1;
        set => SetBits(value ? 1 : 0, 15, 1);
    }

    public static implicit operator ushort(MkdsKclPrismAttribute value) => value._value;
    public static implicit operator MkdsKclPrismAttribute(ushort value) => new(value);
}