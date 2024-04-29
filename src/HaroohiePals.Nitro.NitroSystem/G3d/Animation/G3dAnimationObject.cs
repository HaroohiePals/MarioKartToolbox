using HaroohiePals.Nitro.NitroSystem.G3d.Binary;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

public sealed class G3dAnimationObject
{
    public const ushort MapDataExist = 0x0100;
    public const ushort MapDataDisabled = 0x0200;
    public const ushort MapDataDatafield = 0x00ff;

    public G3dAnimationObject(G3dAnimation animation, G3dModel model, G3dTextureSet tex)
    {
        Frame = 0;
        AnimationResource = animation;
        Next = null;
        Priority = 127;
        Ratio = 1;
        TextureResource = tex;
        animation.InitializeAnimationObject(this, model);
    }

    public double Frame;
    public double Ratio;
    public G3dAnimation AnimationResource;

    public Delegate AnimationFunction;

    public G3dAnimationObject Next;

    public G3dTextureSet TextureResource;

    public MaterialTextureInfo[][] FrameTextureInfos;

    public byte Priority;
    public ushort[] MapData;

    public static void AddLink(ref G3dAnimationObject list, G3dAnimationObject item)
    {
        if (list is null)
        {
            list = item;
        }
        else if (list.Next is null)
        {
            if (list.Priority > item.Priority)
            {
                var cur = item;
                while (cur.Next is not null)
                {
                    cur = cur.Next;
                }

                cur.Next = list;
                list = item;
            }
            else
            {
                list.Next = item;
            }
        }
        else
        {
            var prev = list;
            var cur = list.Next;

            while (cur != null)
            {
                if (cur.Priority >= item.Priority)
                {
                    var p = item;
                    while (p.Next is not null)
                    {
                        p = p.Next;
                    }

                    prev.Next = item;
                    p.Next = cur;
                    return;
                }

                prev = cur;
                cur = cur.Next;
            }

            prev.Next = item;
        }
    }

    public static bool RemoveLink(ref G3dAnimationObject list, G3dAnimationObject item)
    {
        if (list is null)
        {
            return false;
        }

        if (list == item)
        {
            list = list.Next;
            item.Next = null;
            return true;
        }

        var cur = list.Next;
        var prev = list;
        while (cur != null)
        {
            if (cur == item)
            {
                prev.Next = cur.Next;
                cur.Next = null;
                return true;
            }

            prev = cur;
            cur = cur.Next;
        }

        return false;
    }
}