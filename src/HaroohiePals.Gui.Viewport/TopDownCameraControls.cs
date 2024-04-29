using HaroohiePals.Gui.Viewport.Projection;
using ImGuiNET;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public class TopDownCameraControls
{
    private readonly OrthographicProjection _projection;

    public float MinimumViewport { get; set; }
    public float MaximumViewport { get; set; }

    public TopDownCameraControls(OrthographicProjection projection, float minimumViewport, float maximumViewport)
    {
        _projection     = projection;
        MinimumViewport = minimumViewport;
        MaximumViewport = maximumViewport;
    }

    public void Update(ViewportContext context, float deltaTime)
    {
        UpdateCameraDrag(context);
        UpdateZoom(context);
    }

    private void UpdateCameraDrag(ViewportContext context)
    {
        if (!ImGui.IsWindowFocused())
            return;

        var disp = _projection.GetDisplayRect(context);
        var drag = ImGui.GetMouseDragDelta(ImGuiMouseButton.Right);
        ImGui.ResetMouseDragDelta(ImGuiMouseButton.Right);
        _projection.ViewportOffset -= (drag.X * disp.Size.X / context.ViewportSize.X,
            drag.Y * disp.Size.Y / context.ViewportSize.Y);
    }

    private void SetViewportSize(float viewportSize)
    {
        if (viewportSize > MaximumViewport)
            _projection.ViewportSize = MaximumViewport;
        else if (viewportSize < MinimumViewport)
            _projection.ViewportSize = MinimumViewport;
        else
            _projection.ViewportSize = viewportSize;
    }

    private void UpdateZoom(ViewportContext context)
    {
        if (!ImGui.IsWindowHovered())
            return;

        if (ImGui.GetIO().MouseWheel == 0)
            return;

        var disp = _projection.GetDisplayRect(context);
        //Calculate the real number of mouse wheel clicks
        // int realDelta = e.Delta / SystemInformation.MouseWheelScrollDelta;
        float realDelta = ImGui.GetIO().MouseWheel;
        //Convert window relative to panel1 relative coordinates
        var mouseRel = ImGui.GetMousePos() - ImGui.GetCursorScreenPos();
        double realX = mouseRel.X * disp.Size.X / context.ViewportSize.X + disp.Min.X;
        double realY = mouseRel.Y * disp.Size.Y / context.ViewportSize.Y + disp.Min.Y;
        double newWidth, newHeight;
        if (realDelta < 0) //zoom out
        {
            //Make the realdelta positive
            realDelta = -realDelta;
            //For each mouse wheel click, the visible area is doubled,
            //so for multiple clicks it's 2^RealDelta
            double scale = System.Math.Pow(1.5f, realDelta);
            newWidth = disp.Size.X * scale;
            newHeight = disp.Size.Y * scale;
        }
        else //zoom in
        {
            //For each mouse wheel click, the visible area is halved,
            //so for multiple clicks it's 2^RealDelta
            double scale = System.Math.Pow(1.5f, realDelta);
            newWidth = disp.Size.X / scale;
            newHeight = disp.Size.Y / scale;
        }

        if (context.ViewportSize.X <= context.ViewportSize.Y)
        {
            if (newWidth > MaximumViewport * 2)
            {
                newWidth = MaximumViewport * 2;
                newHeight = newWidth * context.ViewportSize.Y / context.ViewportSize.X;
            }

            if (newWidth < MinimumViewport * 2)
            {
                newWidth = MinimumViewport * 2;
                newHeight = newWidth * context.ViewportSize.Y / context.ViewportSize.X;
            }
        }
        else
        {
            if (newHeight > MaximumViewport * 2)
            {
                newHeight = MaximumViewport * 2;
                newWidth = newHeight * context.ViewportSize.X / context.ViewportSize.Y;
            }

            if (newHeight < MinimumViewport * 2)
            {
                newHeight = MinimumViewport * 2;
                newWidth = newHeight * context.ViewportSize.X / context.ViewportSize.Y;
            }
        }

        double x1 = realX - newWidth / 2;
        double y1 = realY - newHeight / 2;
        double x2 = realX + newWidth / 2;
        double y2 = realY + newHeight / 2;

        //find the offset
        double offsetX = mouseRel.X * newWidth / context.ViewportSize.X + x1;
        x1 += -offsetX + realX;
        x2 += -offsetX + realX;

        double offsetY = mouseRel.Y * newHeight / context.ViewportSize.Y + y1;
        y1 += -offsetY + realY;
        y2 += -offsetY + realY;

        double halfWidthX = (x2 - x1) / 2f;
        double halfWidthY = (y2 - y1) / 2f;
        _projection.ViewportOffset = new Vector2((float)(x1 + halfWidthX), (float)(y1 + halfWidthY));
        SetViewportSize((float)(context.ViewportSize.X <= context.ViewportSize.Y ? halfWidthX : halfWidthY));
    }
}