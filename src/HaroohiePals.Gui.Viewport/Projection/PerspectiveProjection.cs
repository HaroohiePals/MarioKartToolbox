using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport.Projection;

public sealed class PerspectiveProjection : IProjection
{
    public float Fov { get; set; }
    public float Near { get; set; }
    public float Far { get; set; }

    public PerspectiveProjection(float fov, float near, float far)
    {
        Fov = fov;
        Near = near;
        Far = far;
    }

    public Matrix4 GetProjectionMatrix(ViewportContext context)
    {
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Fov),
            context.ViewportSize.X / (float)context.ViewportSize.Y, Near, Far);
    }
}