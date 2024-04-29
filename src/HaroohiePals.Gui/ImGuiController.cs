using HaroohiePals.Graphics3d;
using HaroohiePals.Graphics3d.OpenGL;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Image = SixLabors.ImageSharp.Image;
using TextureWrapMode = HaroohiePals.Graphics3d.TextureWrapMode;

namespace HaroohiePals.Gui
{
    internal partial class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        private GLVertexArray _vertexArray;
        private GLBuffer<ImDrawVert> _vertexBuffer;
        private int _vertexBufferSize;
        private GLBuffer<ushort> _indexBuffer;
        private int _indexBufferSize;

        private GLTexture _fontTexture;
        private GLShader _shader;

        private int _windowWidth;
        private int _windowHeight;

        private List<ImGuiIcon> _icons = new();

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        private float _uiScale = 1.0f;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height, float uiScale = 1.0f, ImGuiIcon[] icons = null)
        {
            _uiScale = uiScale;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);
            //ImGuizmo.SetImGuiContext(context);
            //IntPtr implotContext = ImPlot.CreateContext();
            //ImPlot.SetCurrentContext(implotContext);
            //ImPlot.SetImGuiContext(context);

            var io = ImGui.GetIO();

            //io.Fonts.AddFontDefault();
            ImFontConfigPtr conf;
            unsafe
            {
                conf = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());
            }

            conf.PixelSnapH = true; //false;
            conf.OversampleH = 1;
            conf.OversampleV = 1;

            ImFontConfigPtr conf2;

            unsafe
            {
                byte[] font = Resources.Fonts.Roboto_Regular;
                fixed (byte* p = Resources.Fonts.Roboto_Regular)
                    io.Fonts.AddFontFromMemoryTTF((IntPtr)p, font.Length, 15f * _uiScale, conf);
                conf2 = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());
            }

            conf2.PixelSnapH = true;
            conf2.OversampleH = 1;
            conf2.OversampleV = 1;
            conf2.MergeMode = true;
            conf2.GlyphOffset.Y = 1;
            conf2.GlyphMinAdvanceX = 16f; // Use if you want to make the icon monospaced
            var iconRanges = new ushort[] { FontAwesome6.IconMin, FontAwesome6.IconMax, 0 };
            unsafe
            {
                byte[] font = Resources.Fonts.fa_solid_900;
                fixed (byte* p = Resources.Fonts.fa_solid_900)
                {
                    fixed (ushort* rang = iconRanges)
                        io.Fonts.AddFontFromMemoryTTF((IntPtr)p, font.Length, 16f * _uiScale, conf2, (IntPtr)rang);
                }
            }

            io.BackendFlags = ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasViewports |
                              ImGuiBackendFlags.HasMouseCursors;
            io.ConfigFlags = ImGuiConfigFlags.DockingEnable;
            io.ConfigWindowsResizeFromEdges = true;
            io.ConfigWindowsMoveFromTitleBarOnly = true;
            io.ConfigDragClickToInputText = true;

            InitIcons(icons);
            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            //ImGuizmo.BeginFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            _vertexArray = new GLVertexArray();

            _vertexBufferSize = 500;
            _indexBufferSize = 1000;

            _vertexBuffer = new(_vertexBufferSize, BufferUsageHint.DynamicDraw);
            _indexBuffer = new(_indexBufferSize, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core
uniform mat4 projection_matrix;
layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;
out vec4 color;
out vec2 texCoord;
void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string FragmentSource = @"#version 330 core
uniform sampler2D in_fontTexture;
in vec4 color;
in vec2 texCoord;
out vec4 outputColor;
void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";
            _shader = new GLShader(VertexSource, FragmentSource);

            _vertexArray.Bind();
            _vertexBuffer.Bind(BufferTarget.ArrayBuffer);
            _indexBuffer.Bind(BufferTarget.ElementArrayBuffer);

            int stride = Unsafe.SizeOf<ImDrawVert>();

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // We don't need to unbind the element buffer as that is connected to the vertex array
            // And you should not touch the element buffer when there is no vertex array bound.

            Util.CheckGLError("End of ImGui setup");
        }

        private int RegisterIcon(char character, int size)
        {
            var io = ImGui.GetIO();
            return io.Fonts.AddCustomRectFontGlyph(io.Fonts.Fonts[0], character, size, size, size);
        }

        private void InitIcons(ImGuiIcon[] icons)
        {
            //Default icons
            _icons.Add(new ImGuiIcon(Resources.Icons.Folder_16x, FontAwesome6.Folder[0], 16));
            _icons.Add(new ImGuiIcon(Resources.Icons.Document_16x, FontAwesome6.File[0], 16));
            _icons.Add(new ImGuiIcon(Resources.Icons.ImportFile_16x, FontAwesome6.FileImport[0], 16));
            _icons.Add(new ImGuiIcon(Resources.Icons.ExportFile_16x, FontAwesome6.FileExport[0], 16));
            _icons.Add(new ImGuiIcon(Resources.Icons.VSO_RecycleBin_16x, FontAwesome6.TrashCan[0], 16));
            _icons.Add(new ImGuiIcon(Resources.Icons.Undo_16x, FontAwesome6.ArrowRotateLeft[0], 16));
            _icons.Add(new ImGuiIcon(Resources.Icons.Redo_16x, FontAwesome6.ArrowRotateRight[0], 16));

            if (icons != null)
                _icons.AddRange(icons);
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        private void RecreateFontDeviceTexture()
        {
            var io = ImGui.GetIO();

            Dictionary<ImGuiIcon, int> registeredIcons = new();

            foreach (var icon in _icons)
                registeredIcons.Add(icon, RegisterIcon(icon.CodePoint, 16));

            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            void loadIcon(int rectId, byte[] iconBytes)
            {
                using (var icon = Image.Load<Bgra32>(iconBytes))
                {
                    unsafe
                    {
                        uint* pixPtr = (uint*)pixels;
                        var rect = io.Fonts.GetCustomRectByIndex(rectId);
                        for (int y = 0; y < rect.Height; y++)
                        {
                            uint* p = pixPtr + (rect.Y + y) * width + (rect.X);
                            for (int x = 0; x < rect.Width; x++)
                                p[x] = icon[x, y].PackedValue;
                        }
                    }
                }
            }

            foreach (var icon in _icons)
                loadIcon(registeredIcons[icon], icon.ImageBytes);

            _fontTexture = new GLTexture(pixels, width, height, PixelFormat.Bgra, PixelType.UnsignedByte,
                TextureWrapMode.Clamp, TextureWrapMode.Clamp);
            _fontTexture.SetFilterMode(TextureFilterMode.Linear, TextureFilterMode.Linear);

            io.Fonts.SetTexID((IntPtr)_fontTexture.Handle);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData());
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGui.NewFrame();
            //ImGuizmo.BeginFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private static readonly Keys[] KeyValues = Enum.GetValues<Keys>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            var io = ImGui.GetIO();

            var mouseState = wnd.MouseState;
            var keyboardState = wnd.KeyboardState;

            io.MouseDown[0] = mouseState[MouseButton.Left];
            io.MouseDown[1] = mouseState[MouseButton.Right];
            io.MouseDown[2] = mouseState[MouseButton.Middle];

            var point = new Vector2i((int)mouseState.X, (int)mouseState.Y);
            io.MousePos.X = point.X;
            io.MousePos.Y = point.Y;

            foreach (var key in KeyValues)
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }

                io.KeysDown[(int)key] = keyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }

            PressedChars.Clear();

            io.KeyCtrl = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space;
            io.KeyMap[(int)ImGuiKey.Pause] = (int)Keys.Pause;
            io.KeyMap[(int)ImGuiKey.Minus] = (int)Keys.Minus;
            io.KeyMap[(int)ImGuiKey.Period] = (int)Keys.Period;
            io.KeyMap[(int)ImGuiKey.Comma] = (int)Keys.Comma;
            io.KeyMap[(int)ImGuiKey.KeypadSubtract] = (int)Keys.KeyPadSubtract;
            io.KeyMap[(int)ImGuiKey.KeypadDecimal] = (int)Keys.KeyPadDecimal;

            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.B] = (int)Keys.B;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.D] = (int)Keys.D;
            io.KeyMap[(int)ImGuiKey.E] = (int)Keys.E;
            io.KeyMap[(int)ImGuiKey.F] = (int)Keys.F;
            io.KeyMap[(int)ImGuiKey.G] = (int)Keys.G;
            io.KeyMap[(int)ImGuiKey.H] = (int)Keys.H;
            io.KeyMap[(int)ImGuiKey.I] = (int)Keys.I;
            io.KeyMap[(int)ImGuiKey.J] = (int)Keys.J;
            io.KeyMap[(int)ImGuiKey.K] = (int)Keys.K;
            io.KeyMap[(int)ImGuiKey.L] = (int)Keys.L;
            io.KeyMap[(int)ImGuiKey.M] = (int)Keys.M;
            io.KeyMap[(int)ImGuiKey.N] = (int)Keys.N;
            io.KeyMap[(int)ImGuiKey.O] = (int)Keys.O;
            io.KeyMap[(int)ImGuiKey.P] = (int)Keys.P;
            io.KeyMap[(int)ImGuiKey.Q] = (int)Keys.Q;
            io.KeyMap[(int)ImGuiKey.R] = (int)Keys.R;
            io.KeyMap[(int)ImGuiKey.S] = (int)Keys.S;
            io.KeyMap[(int)ImGuiKey.T] = (int)Keys.T;
            io.KeyMap[(int)ImGuiKey.U] = (int)Keys.U;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.W] = (int)Keys.W;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;

            io.KeyMap[(int)ImGuiKey._0] = (int)Keys.D0;
            io.KeyMap[(int)ImGuiKey._1] = (int)Keys.D1;
            io.KeyMap[(int)ImGuiKey._2] = (int)Keys.D2;
            io.KeyMap[(int)ImGuiKey._3] = (int)Keys.D3;
            io.KeyMap[(int)ImGuiKey._4] = (int)Keys.D4;
            io.KeyMap[(int)ImGuiKey._5] = (int)Keys.D5;
            io.KeyMap[(int)ImGuiKey._6] = (int)Keys.D6;
            io.KeyMap[(int)ImGuiKey._7] = (int)Keys.D7;
            io.KeyMap[(int)ImGuiKey._8] = (int)Keys.D8;
            io.KeyMap[(int)ImGuiKey._9] = (int)Keys.D9;

            io.KeyMap[(int)ImGuiKey.Keypad0] = (int)Keys.KeyPad0;
            io.KeyMap[(int)ImGuiKey.Keypad1] = (int)Keys.KeyPad1;
            io.KeyMap[(int)ImGuiKey.Keypad2] = (int)Keys.KeyPad2;
            io.KeyMap[(int)ImGuiKey.Keypad3] = (int)Keys.KeyPad3;
            io.KeyMap[(int)ImGuiKey.Keypad4] = (int)Keys.KeyPad4;
            io.KeyMap[(int)ImGuiKey.Keypad5] = (int)Keys.KeyPad5;
            io.KeyMap[(int)ImGuiKey.Keypad6] = (int)Keys.KeyPad6;
            io.KeyMap[(int)ImGuiKey.Keypad7] = (int)Keys.KeyPad7;
            io.KeyMap[(int)ImGuiKey.Keypad8] = (int)Keys.KeyPad8;
            io.KeyMap[(int)ImGuiKey.Keypad9] = (int)Keys.KeyPad9;

            io.KeyMap[(int)ImGuiKey.F1] = (int)Keys.F1;
            io.KeyMap[(int)ImGuiKey.F2] = (int)Keys.F2;
            io.KeyMap[(int)ImGuiKey.F3] = (int)Keys.F3;
            io.KeyMap[(int)ImGuiKey.F4] = (int)Keys.F4;
            io.KeyMap[(int)ImGuiKey.F5] = (int)Keys.F5;
            io.KeyMap[(int)ImGuiKey.F6] = (int)Keys.F6;
            io.KeyMap[(int)ImGuiKey.F7] = (int)Keys.F7;
            io.KeyMap[(int)ImGuiKey.F8] = (int)Keys.F8;
            io.KeyMap[(int)ImGuiKey.F9] = (int)Keys.F9;
            io.KeyMap[(int)ImGuiKey.F10] = (int)Keys.F10;
            io.KeyMap[(int)ImGuiKey.F11] = (int)Keys.F11;
            io.KeyMap[(int)ImGuiKey.F12] = (int)Keys.F12;
        }

        private void RenderImDrawData(ImDrawDataPtr drawData)
        {
            if (drawData.CmdListsCount == 0)
            {
                return;
            }

            GL.Viewport(0, 0, _windowWidth, _windowHeight);

            for (int i = 0; i < drawData.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = drawData.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size;
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)System.Math.Max(_vertexBufferSize * 1.5f, vertexSize);
                    _vertexBuffer.BufferData(newSize, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    //Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size;
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)System.Math.Max(_indexBufferSize * 1.5f, indexSize);
                    _indexBuffer.BufferData(newSize, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    //Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            var mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f, io.DisplaySize.X,
                io.DisplaySize.Y, 0.0f,
                -1.0f, 1.0f);

            _shader.Use();
            _shader.SetMatrix4("projection_matrix", mvp, false);
            _shader.SetInt("in_fontTexture", 0);
            Util.CheckGLError("Projection");

            _vertexArray.Bind();
            Util.CheckGLError("VAO");

            drawData.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                var cmdList = drawData.CmdListsRange[n];

                _vertexBuffer.BufferSubData(0, cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size);
                Util.CheckGLError($"Data Vert {n}");

                _indexBuffer.BufferSubData(0, cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size);
                Util.CheckGLError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmdList.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmdList.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        Util.CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X),
                            (int)(clip.W - clip.Y));
                        Util.CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount,
                                DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)),
                                (int)pcmd.VtxOffset);
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort,
                                (int)pcmd.IdxOffset * sizeof(ushort));
                        }

                        Util.CheckGLError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _vertexArray.Dispose();
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();

            _fontTexture.Dispose();
            _shader.Dispose();
        }
    }
}