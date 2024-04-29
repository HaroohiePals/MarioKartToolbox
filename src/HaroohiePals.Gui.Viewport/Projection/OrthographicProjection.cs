using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport.Projection;

public class OrthographicProjection : IProjection
{
    public float ViewportSize { get; set; }
    public Vector2 ViewportOffset { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }

    public OrthographicProjection(float viewportSize, Vector2 viewportOffset, float near, float far)
    {
        ViewportSize = viewportSize;
        ViewportOffset = viewportOffset;
        Near = near;
        Far = far;
    }

    public Matrix4 GetProjectionMatrix(ViewportContext context)
    {
        var displayRect = GetDisplayRect(context);
        return Matrix4.CreateOrthographicOffCenter(
            displayRect.Min.X, displayRect.Max.X,
            displayRect.Max.Y, displayRect.Min.Y,
            Near, Far);
    }

    public Box2 GetDisplayRect(ViewportContext context)
    {
        float max = Math.Max(
            ViewportSize * 2 / context.ViewportSize.X,
            ViewportSize * 2 / context.ViewportSize.Y);

        float minX = ViewportOffset.X - max * context.ViewportSize.X / 2f;
        float minY = ViewportOffset.Y - max * context.ViewportSize.Y / 2f;

        return new Box2(minX, minY, minX + max * context.ViewportSize.X, minY + max * context.ViewportSize.Y);
    }
}