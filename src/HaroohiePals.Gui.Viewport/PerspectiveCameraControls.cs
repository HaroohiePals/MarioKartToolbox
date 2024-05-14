using ImGuiNET;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport;

public class PerspectiveCameraControls
{
    private readonly ViewCube _viewCube = new();

    public float MinControlSpeed { get; set; } = 0.1f;
    public float MaxControlSpeed { get; set; } = 5f;
    public float ControlSpeed    { get; set; } = 1;

    public ViewportKeyBindings ViewportKeyBindings { get; set; } = new();

    public bool ShowViewCube { get; set; } = true;

    public void Update(ViewportContext context, float deltaTime)
    {
        UpdateCameraKeys(context, deltaTime);
        UpdateCameraDrag(context);
        UpdateCameraMouseWheel(context);
    }

    public void Render(ViewportContext context)
    {
        if (ShowViewCube)
            RenderViewCube(context);
    }

    private void UpdateCameraKeys(ViewportContext context, float deltaTime)
    {
        if (!ImGui.IsWindowFocused() || !ImGui.IsMouseDown(ImGuiMouseButton.Right))
            return;

        var controls = ViewportKeyBindings;

        float speed = 100f * deltaTime * 16f * ControlSpeed;

        var viewInverse = context.ViewMatrix.Inverted();

        var io = ImGui.GetIO();

        if (io.KeyAlt)
            speed /= 10;
        else if (io.KeyShift)
            speed *= 10;

        var translation = Vector3.Zero;

        if (controls.Forward.IsDown(false))
            translation -= viewInverse.Row2.Xyz * speed; //Forward
        if (controls.Backward.IsDown(false))
            translation += viewInverse.Row2.Xyz * speed; //Backwards

        if (controls.Left.IsDown(false))
            translation -= viewInverse.Row0.Xyz * speed; //Left
        if (controls.Right.IsDown(false))
            translation += viewInverse.Row0.Xyz * speed; //Right

        if (controls.Up.IsDown(false))
            translation += Vector3.UnitY * speed; //Up 
        if (controls.Down.IsDown(false))
            translation -= Vector3.UnitY * speed; //Down

        if (translation != Vector3.Zero)
        {
            viewInverse        *= Matrix4.CreateTranslation(translation);
            context.ViewMatrix =  viewInverse.Inverted();
        }
    }

    private void UpdateCameraDrag(ViewportContext context)
    {
        if (!ImGui.IsWindowFocused() || !ImGui.IsItemActive())
            return;

        if (ImGui.IsMouseDragging(ImGuiMouseButton.Right))
        {
            // Cam rot

            float deltaX = ImGui.GetIO().MouseDelta.X * 0.002f;

            if (Vector3.Dot(context.ViewMatrix.Inverted().Row1.Xyz, Vector3.UnitY) < 0)
                deltaX = -deltaX;

            var rx = Matrix4.CreateFromAxisAngle(context.ViewMatrix.Row1.Xyz, deltaX);
            var ry = Matrix4.CreateFromAxisAngle(Vector3.UnitX, ImGui.GetIO().MouseDelta.Y * 0.002f);
            context.ViewMatrix *= rx * ry;
        }
        else if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle))
        {
            // Cam move
            // todo
        }
    }

    private void UpdateCameraMouseWheel(ViewportContext context)
    {
        if (!ImGui.IsItemHovered() && !ImGui.IsWindowHovered())
            return;

        if (ImGui.GetIO().MouseWheel == 0)
            return;

        float amount = ImGui.GetIO().MouseWheel;

        if (ImGui.IsMouseDragging(ImGuiMouseButton.Right))
        {
            //When changing the camera rotation, you can use the scroll wheel to change the speed of the WASD movement

            ControlSpeed += amount / 10;

            ControlSpeed = System.Math.Clamp(ControlSpeed, MinControlSpeed, MaxControlSpeed);
        }
        else
        {
            // Otherwise just zoom in

            var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos();
            var invMtx   = (context.ViewMatrix * context.ProjectionMatrix).Inverted();
            var pos      = new Vector3(mousePos.X, context.ViewportSize.Y - mousePos.Y - 1, 0);
            var near     = Vector3.Unproject(pos, 0, 0, context.ViewportSize.X, context.ViewportSize.Y, 0, 1, invMtx);
            pos.Z = 1;
            var far = Vector3.Unproject(pos, 0, 0, context.ViewportSize.X, context.ViewportSize.Y, 0, 1, invMtx);

            var zoomDirection = (far - near).Normalized();

            var io = ImGui.GetIO();

            float speed = ControlSpeed;
            if (io.KeyAlt)
                speed /= 10;
            else if (io.KeyShift)
                speed *= 10;

            speed = amount * speed * 64;

            var viewInverse = context.ViewMatrix.Inverted();
            viewInverse        *= Matrix4.CreateTranslation(speed * zoomDirection);
            context.ViewMatrix =  viewInverse.Inverted();
        }
    }

    private void RenderViewCube(ViewportContext context)
    {
        var   contentPos          = ImGui.GetWindowPos() + ImGui.GetWindowContentRegionMin();
        float viewManipulateRight = contentPos.X + context.ViewportSize.X;
        float viewManipulateTop   = contentPos.Y;

        var viewMtx = context.ViewMatrix;

        _viewCube.ViewManipulate(ref viewMtx, 1, (viewManipulateRight - 128, viewManipulateTop), (128, 128),
            0x10101010);

        context.ViewMatrix = viewMtx;
    }
}