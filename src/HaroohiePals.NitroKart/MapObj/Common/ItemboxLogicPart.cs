namespace HaroohiePals.NitroKart.MapObj.Common;

class ItemboxLogicPart : LogicPart<Itembox>
{
    public ItemboxLogicPart(MkdsContext context) 
        : base(context, LogicPartType.Type0)
    {
    }

    protected override void Update(Itembox instance)
        => instance.Update();
}
