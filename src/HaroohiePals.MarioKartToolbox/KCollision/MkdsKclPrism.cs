using HaroohiePals.KCollision;

namespace HaroohiePals.MarioKartToolbox.KCollision;

public class MkdsKclPrism : MkdsKclAttributeAdapter
{
    private KclPrism _prism;

    public MkdsKclPrism(KclPrism prism)
    {
        _prism = prism;
    }

    public override MkdsKclPrismAttribute Attribute
    {
        get => _prism.Attribute;
        set => _prism.Attribute = value;
    }
}