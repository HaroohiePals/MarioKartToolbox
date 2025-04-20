using HaroohiePals.NitroKart.MapObj.Obstacles;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Common;

class ItemboxShadowRenderPart : RenderPart<Itembox>
{
    private Matrix4x3d _shadowMtx;
    private ushort _shadowAngle = 0;

    public ItemboxShadowRenderPart(MkdsContext context)
        : base(context, RenderPartType.Normal, true, false)
    { }

    protected override void GlobalPreRender()
    {
        _shadowAngle += 202;
        _shadowMtx = Matrix4x3d.CreateRotationY(MObjUtil.IdxToRad(_shadowAngle));
    }

    protected override void Render(Itembox instance, in Matrix4x3d camMtx, ushort alpha)
    {
        int shadowAlpha;
        int boxAlpha;
        int questionAlpha;
        int respawnCounter = instance.RespawnCounter - 64;
        if (respawnCounter >= 64)
        {
            instance.BoxAlpha = 26;
            instance.QuestionAlpha = 20;
            shadowAlpha = 13;
        }
        else
        {
            boxAlpha = (26 * respawnCounter) >> 6;
            questionAlpha = (20 * respawnCounter) >> 6;
            shadowAlpha = (13 * respawnCounter) >> 6;
            if (boxAlpha > 0)
            {
                if (questionAlpha < 1)
                    questionAlpha = 1;
                if (shadowAlpha < 1)
                    shadowAlpha = 1;
            }
            instance.BoxAlpha = boxAlpha;
            instance.QuestionAlpha = questionAlpha;
        }

        //distance = (instance->mobj.visibilityFlags & MOBJ_INST_VISIBILITY_FLAGS_DISTANCE_MASK);
        if (/*(distance <= 0x27100 || rstat_getUncontrollable()) && */shadowAlpha >= 1)
        {
            if (instance.ShadowType == ItemboxShadowType.Shadow2d)
            {
                instance.ObjectShadow.Alpha = (ushort)shadowAlpha;
                instance.ObjectShadow.RenderMat(instance is MoveItembox moveIbox ? moveIbox.Shadow2dScale : null, camMtx, alpha);
            }
            else
            {
                if (instance.ShadowType == ItemboxShadowType.Shadow3d)
                {
                    if (instance is MoveItembox moveIbox)
                    {
                        _shadowMtx.Row3 = instance.RenderPos / 16.0;
                        instance.ObjectShadow.RenderJgShadowTransformed(moveIbox.Shadow3dScale, _shadowMtx, (ushort)shadowAlpha, alpha);
                    }
                    else
                    {
                        var scale = 1.4 * instance.Scale;

                        //if (instance.BoxAnimFunc is not null)
                        //{
                        //    shadowMtx = (MtxFx43*)bbm_20E6B58(instance->renderPos);
                        //}
                        //else
                        //{
                        //    _shadowMtx.Row3 = instance.RenderPos / 16.0;
                        //    shadowMtx = _shadowMtx;
                        //}

                        _shadowMtx.Row3 = instance.RenderPos / 16.0;

                        instance.ObjectShadow.RenderJgShadowTransformed(scale, _shadowMtx, (ushort)shadowAlpha, alpha);
                    }
                }
            }
        }
    }
}