using HaroohiePals.Actions;
using ImGuiNET;
using OpenTK.Mathematics;

namespace HaroohiePals.Gui.Viewport
{
    public abstract class DrawTool
    {
        public bool IsMouseDrag { get; private set; }
        public bool IsMouseDown { get; private set; }
        public bool IsMouseUp { get; private set; }
        public bool IsUsing { get; private set; }

        protected IAtomicActionBuilder _atomicActionBuilder;

        protected abstract bool MouseDown(ViewportContext context, Vector3d rayStart, Vector3d rayDir);
        protected abstract bool MouseDrag(ViewportContext context, Vector3d rayStart, Vector3d rayDir);
        protected abstract bool MouseUp(ViewportContext context, Vector3d rayStart, Vector3d rayDir);

        private void GetMouseRay(ViewportContext context, out Vector3d rayStart, out Vector3d rayDir)
        {
            var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos();
            var invMtx = (context.ViewMatrix * context.ProjectionMatrix).Inverted();
            var pos = new Vector3(mousePos.X, context.ViewportSize.Y - mousePos.Y - 1, 0);
            var near = Vector3.Unproject(pos, 0, 0, context.ViewportSize.X, context.ViewportSize.Y, 0, 1, invMtx);
            pos.Z = 1;
            var far = Vector3.Unproject(pos, 0, 0, context.ViewportSize.X, context.ViewportSize.Y, 0, 1, invMtx);

            rayStart = near;
            rayDir = (far - near).Normalized();
        }

        private void CancelDraw()
        {
            _atomicActionBuilder?.Cancel();
            _atomicActionBuilder = null;

            IsMouseDown = false;
            IsMouseDrag = false;
            IsMouseUp = false;
            IsUsing = false;
        }

        public void PerformDraw(ViewportContext context)
        {
            try
            {
                if (ImGui.IsKeyPressed(ImGuiKey.Escape))
                {
                    CancelDraw();
                    return;
                }

                if (_atomicActionBuilder == null)
                    _atomicActionBuilder = context.ActionStack.PerformAtomic();

                if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    IsMouseDown = true;
                    GetMouseRay(context, out var rayStart, out var rayDir);
                    IsUsing = MouseDown(context, rayStart, rayDir);
                }
                else
                {
                    IsMouseDown = false;
                }

                if (ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    IsMouseDrag = true;
                    GetMouseRay(context, out var rayStart, out var rayDir);
                    IsUsing = MouseDrag(context, rayStart, rayDir);
                }
                else
                {
                    IsMouseDrag = false;
                }

                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left))
                {
                    IsMouseUp = true;
                    GetMouseRay(context, out var rayStart, out var rayDir);
                    MouseUp(context, rayStart, rayDir);

                    IsUsing = false;
                    _atomicActionBuilder?.Commit();
                    _atomicActionBuilder = null;
                }
                else
                {
                    IsMouseUp = false;
                }
            }
            catch
            {

            }
        }
    }
}
