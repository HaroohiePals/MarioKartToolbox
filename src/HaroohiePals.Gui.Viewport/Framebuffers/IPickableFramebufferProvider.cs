namespace HaroohiePals.Gui.Viewport.Framebuffers;

public interface IPickableFramebufferProvider : IGLFramebufferProvider
{
    uint GetPickingId(int x, int y);
    uint[] GetPickingIds(int x, int y, int width, int height);
}