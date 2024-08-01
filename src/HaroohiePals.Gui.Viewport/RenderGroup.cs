using HaroohiePals.Mathematics;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public abstract class RenderGroup
{
    public bool Enabled        { get; set; } = true;
    public int  PickingGroupId { get; set; }

    public virtual void Update(float deltaTime) { }
    public abstract void Render(ViewportContext context);

    public virtual object GetObject(int index) => null;

    public virtual bool TryGetObjectTransform(object obj, int subIndex, out Transform transform)
    {
        transform = Transform.Identity;
        return false;
    }

    public virtual bool TrySetObjectTransform(object obj, int subIndex, in Transform transform) 
        => false;

    public virtual bool TryGetLocalObjectBounds(object obj, int subIndex, out Box3d bounds)
    {
        bounds = new Box3d();
        return false;
    }

    public virtual bool ContainsObject(object obj) => false;
}