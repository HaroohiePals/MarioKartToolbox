using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport.Projection;

public interface IProjection
{
    Matrix4 GetProjectionMatrix(ViewportContext context);
}