namespace HaroohiePals.NitroKart.MapObj.Common;

class ItemboxShadowRenderPart : RenderPart<Itembox>
{
    public ItemboxShadowRenderPart(MkdsContext context)
        : base(context, RenderPartType.Normal, true, false)
    {
    }
}