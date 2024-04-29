using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace HaroohiePals.NitroKart.MapObj;

public abstract class RenderPart
{
    public enum RenderPartType
    {
        None,
        Normal,
        Billboard
    }

    protected readonly MkdsContext _context;

    public RenderPartType Type;

    public readonly bool IsShadow;
    public readonly bool IsTranslucent;

    protected RenderPart(MkdsContext context, RenderPartType type, bool isShadow = false, bool isTranslucent = false)
    {
        _context = context;
        Type = type;
        IsShadow = isShadow;
        IsTranslucent = isTranslucent;
    }

    public abstract void AddInstance(MObjInstance instance);
    public void Initialize() => GlobalInit();
    public abstract void Render(in Matrix4x3d camMtx);

    protected virtual void GlobalInit() { }
    protected virtual void GlobalPreRender() { }
    protected virtual void GlobalPostRender() { }
}

public class RenderPart<T> : RenderPart where T : MObjInstance
{
    protected readonly List<T> _instances = new();

    public RenderPart(MkdsContext context, RenderPartType type, bool isShadow = false, bool isTranslucent = false)
        : base(context, type, isShadow, isTranslucent) { }

    public override void AddInstance(MObjInstance instance)
    {
        if (instance is not T tInst)
            throw new ArgumentException(nameof(instance));

        _instances.Add(tInst);
    }

    protected virtual void Render(T instance, in Matrix4x3d camMtx, ushort alpha) { }

    public override void Render(in Matrix4x3d camMtx)
    {
        try
        {
            GlobalPreRender();
        }
        catch
        {

        }

        // todo: translucency sorting
        foreach (var instance in _instances)
        {
            if ((instance.Flags & (MObjInstance.InstanceFlags.Clipped |
                                   MObjInstance.InstanceFlags.DisableVisibilityUpdates)) != 0
                || instance.Alpha == 0)
                continue;

            try
            {
                Render(instance, camMtx, instance.Alpha);
            }
            catch
            {

            }
        }

        try
        {
            GlobalPostRender();
        }
        catch
        {

        }
    }
}