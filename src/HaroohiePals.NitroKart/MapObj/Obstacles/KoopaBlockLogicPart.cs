namespace HaroohiePals.NitroKart.MapObj.Obstacles;
class KoopaBlockLogicPart : LogicPart<KoopaBlock>
{
    public KoopaBlockLogicPart(MkdsContext context)
        : base(context, LogicPartType.Type0) { }

    protected override void Update(KoopaBlock instance)
        => instance.Update();
}
