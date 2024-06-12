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

namespace HaroohiePals.Gui;

internal sealed class ImGuiController : IDisposable
{
    private static readonly Keys[] KeyValues = Enum.GetValues<Keys>();

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

    private List<ImGuiIconGlyph> _icons = new();
    private IReadOnlyCollection<ImGuiFont> _fonts;
    private ImGuiFont _iconFont;

    private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

    private float _uiScale = 1.0f;

    private Dictionary<Keys, ImGuiKey> _imguiKeyMap = new Dictionary<Keys, ImGuiKey>();
    private readonly List<char> _pressedInputChars = new List<char>();

    /// <summary>
    /// Constructs a new ImGuiController.
    /// </summary>
    public ImGuiController(int width, int height, ImGuiGameWindowSettings settings)
    {
        _fonts = settings.Fonts;
        _iconFont = settings.IconFont;
        _uiScale = settings.UiScale;
        _windowWidth = width;
        _windowHeight = height;

        IntPtr context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);

        var io = ImGui.GetIO();

        InitFonts();

        io.BackendFlags = ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasViewports |
                          ImGuiBackendFlags.HasMouseCursors;
        io.ConfigFlags = ImGuiConfigFlags.DockingEnable;
        io.ConfigWindowsResizeFromEdges = true;
        io.ConfigWindowsMoveFromTitleBarOnly = true;
        io.ConfigDragClickToInputText = true;

        InitIcons(settings.IconGlyphs);
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

    internal void PressInputChar(char keyChar)
        => _pressedInputChars.Add(keyChar);

    private int RegisterIcon(char character, int size)
    {
        var io = ImGui.GetIO();
        return io.Fonts.AddCustomRectFontGlyph(io.Fonts.Fonts[0], character, size, size, size);
    }

    private void InitIcons(IReadOnlyCollection<ImGuiIconGlyph> icons)
    {
        //Default icons
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.Folder_16x, FontAwesome6.Folder[0], 16));
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.Document_16x, FontAwesome6.File[0], 16));
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.ImportFile_16x, FontAwesome6.FileImport[0], 16));
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.ExportFile_16x, FontAwesome6.FileExport[0], 16));
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.VSO_RecycleBin_16x, FontAwesome6.TrashCan[0], 16));
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.Undo_16x, FontAwesome6.ArrowRotateLeft[0], 16));
        _icons.Add(new ImGuiIconGlyph(Resources.Icons.Redo_16x, FontAwesome6.ArrowRotateRight[0], 16));

        if (icons != null)
            _icons.AddRange(icons);
    }

    private void InitFonts()
    {
        bool isDefaultFont = true;

        foreach (var font in _fonts)
        {
            AddFont(font);

            // Merge icon font only for the default font (first font)
            if (isDefaultFont && _iconFont is not null)
            {
                var iconRanges = new ushort[] { FontAwesome6.IconMin, FontAwesome6.IconMax, 0 };
                AddFont(_iconFont, true, iconRanges);
            }

            isDefaultFont = false;
        }
    }

    private unsafe ImFontConfigPtr CreateImFontConfig(ImGuiFont font, bool isIconFont = false)
    {
        var conf = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());

        conf.PixelSnapH = true;
        conf.OversampleH = 1;
        conf.OversampleV = 1;

        if (isIconFont)
        {
            conf.MergeMode = true;
            conf.GlyphOffset.Y = 1;
            conf.GlyphMinAdvanceX = font.Size; // Use if you want to make the icon monospaced
        }

        return conf;
    }

    private unsafe void AddFont(ImGuiFont font, bool isIconFont = false, ushort[] iconRanges = null)
    {
        var io = ImGui.GetIO();
        var fontConfig = CreateImFontConfig(_iconFont, isIconFont);

        fixed (byte* p = font.Data)
        fixed (ushort* range = iconRanges)
        {
            io.Fonts.AddFontFromMemoryTTF((IntPtr)p, font.Data.Length, font.Size * _uiScale, fontConfig, (IntPtr)range);
        }
    }

    /// <summary>
    /// Recreates the device texture used to render text.
    /// </summary>
    private void RecreateFontDeviceTexture()
    {
        var io = ImGui.GetIO();

        Dictionary<ImGuiIconGlyph, int> registeredIcons = new();

        foreach (var icon in _icons)
        {
            registeredIcons.Add(icon, RegisterIcon(icon.CodePoint, 16));
        }

        io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

        foreach (var icon in _icons)
        {
            LoadIcon(registeredIcons[icon], pixels, icon.ImageBytes, width, height);
        }

        _fontTexture = new GLTexture(pixels, width, height, PixelFormat.Bgra, PixelType.UnsignedByte,
            TextureWrapMode.Clamp, TextureWrapMode.Clamp);
        _fontTexture.SetFilterMode(TextureFilterMode.Linear, TextureFilterMode.Linear);

        io.Fonts.SetTexID((IntPtr)_fontTexture.Handle);
        io.Fonts.ClearTexData();
    }

    private unsafe void LoadIcon(int rectId, IntPtr pixels, byte[] iconBytes, int width, int height)
    {
        var io = ImGui.GetIO();

        using (var icon = Image.Load<Bgra32>(iconBytes))
        {
            uint* pixPtr = (uint*)pixels;
            var rect = io.Fonts.GetCustomRectByIndex(rectId);
            for (int y = 0; y < rect.Height; y++)
            {
                uint* p = pixPtr + (rect.Y + y) * width + (rect.X);
                for (int x = 0; x < rect.Width; x++)
                {
                    p[x] = icon[x, y].PackedValue;
                }
            }
        }
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

    private void UpdateImGuiInput(GameWindow wnd)
    {
        var io = ImGui.GetIO();

        var mouseState = wnd.MouseState;
        var keyboardState = wnd.KeyboardState;

        io.AddMousePosEvent(mouseState.X, mouseState.Y);
        io.AddMouseButtonEvent(0, mouseState.IsButtonDown(MouseButton.Left));
        io.AddMouseButtonEvent(1, mouseState.IsButtonDown(MouseButton.Right));
        io.AddMouseButtonEvent(2, mouseState.IsButtonDown(MouseButton.Middle));
        io.AddMouseButtonEvent(3, mouseState.IsButtonDown(MouseButton.Button1));
        io.AddMouseButtonEvent(4, mouseState.IsButtonDown(MouseButton.Button2));
        io.AddMouseWheelEvent(mouseState.ScrollDelta.X, mouseState.ScrollDelta.Y);

        foreach (var key in KeyValues)
        {
            if (key == Keys.Unknown)
            {
                continue;
            }

            if (_imguiKeyMap.TryGetValue(key, out ImGuiKey imguiKey))
            {
                io.AddKeyEvent(imguiKey, keyboardState.IsKeyDown(key));
            }
        }

        io.AddKeyEvent(ImGuiKey.ModCtrl, keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl));
        io.AddKeyEvent(ImGuiKey.ModAlt, keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt));
        io.AddKeyEvent(ImGuiKey.ModShift, keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift));
        io.AddKeyEvent(ImGuiKey.ModSuper, keyboardState.IsKeyDown(Keys.LeftSuper) || keyboardState.IsKeyDown(Keys.RightSuper));

        //Process text input
        foreach (var c in _pressedInputChars)
        {
            io.AddInputCharacter(c);
        }

        _pressedInputChars.Clear();
    }

    private void SetKeyMappings()
    {
        _imguiKeyMap.Add(Keys.Tab, ImGuiKey.Tab);
        _imguiKeyMap.Add(Keys.Left, ImGuiKey.LeftArrow);
        _imguiKeyMap.Add(Keys.Right, ImGuiKey.RightArrow);
        _imguiKeyMap.Add(Keys.Up, ImGuiKey.UpArrow);
        _imguiKeyMap.Add(Keys.Down, ImGuiKey.DownArrow);
        _imguiKeyMap.Add(Keys.PageUp, ImGuiKey.PageUp);
        _imguiKeyMap.Add(Keys.PageDown, ImGuiKey.PageDown);
        _imguiKeyMap.Add(Keys.Home, ImGuiKey.Home);
        _imguiKeyMap.Add(Keys.End, ImGuiKey.End);
        _imguiKeyMap.Add(Keys.Delete, ImGuiKey.Delete);
        _imguiKeyMap.Add(Keys.Backspace, ImGuiKey.Backspace);
        _imguiKeyMap.Add(Keys.Enter, ImGuiKey.Enter);
        _imguiKeyMap.Add(Keys.Escape, ImGuiKey.Escape);
        _imguiKeyMap.Add(Keys.Space, ImGuiKey.Space);
        _imguiKeyMap.Add(Keys.Pause, ImGuiKey.Pause);
        _imguiKeyMap.Add(Keys.Minus, ImGuiKey.Minus);
        _imguiKeyMap.Add(Keys.Period, ImGuiKey.Period);
        _imguiKeyMap.Add(Keys.Comma, ImGuiKey.Comma);
        _imguiKeyMap.Add(Keys.KeyPadSubtract, ImGuiKey.KeypadSubtract);
        _imguiKeyMap.Add(Keys.KeyPadDecimal, ImGuiKey.KeypadDecimal);

        _imguiKeyMap.Add(Keys.A, ImGuiKey.A);
        _imguiKeyMap.Add(Keys.B, ImGuiKey.B);
        _imguiKeyMap.Add(Keys.C, ImGuiKey.C);
        _imguiKeyMap.Add(Keys.D, ImGuiKey.D);
        _imguiKeyMap.Add(Keys.E, ImGuiKey.E);
        _imguiKeyMap.Add(Keys.F, ImGuiKey.F);
        _imguiKeyMap.Add(Keys.G, ImGuiKey.G);
        _imguiKeyMap.Add(Keys.H, ImGuiKey.H);
        _imguiKeyMap.Add(Keys.I, ImGuiKey.I);
        _imguiKeyMap.Add(Keys.J, ImGuiKey.J);
        _imguiKeyMap.Add(Keys.K, ImGuiKey.K);
        _imguiKeyMap.Add(Keys.L, ImGuiKey.L);
        _imguiKeyMap.Add(Keys.M, ImGuiKey.M);
        _imguiKeyMap.Add(Keys.N, ImGuiKey.N);
        _imguiKeyMap.Add(Keys.O, ImGuiKey.O);
        _imguiKeyMap.Add(Keys.P, ImGuiKey.P);
        _imguiKeyMap.Add(Keys.Q, ImGuiKey.Q);
        _imguiKeyMap.Add(Keys.R, ImGuiKey.R);
        _imguiKeyMap.Add(Keys.S, ImGuiKey.S);
        _imguiKeyMap.Add(Keys.T, ImGuiKey.T);
        _imguiKeyMap.Add(Keys.U, ImGuiKey.U);
        _imguiKeyMap.Add(Keys.V, ImGuiKey.V);
        _imguiKeyMap.Add(Keys.W, ImGuiKey.W);
        _imguiKeyMap.Add(Keys.X, ImGuiKey.X);
        _imguiKeyMap.Add(Keys.Y, ImGuiKey.Y);
        _imguiKeyMap.Add(Keys.Z, ImGuiKey.Z);

        _imguiKeyMap.Add(Keys.D0, ImGuiKey._0);
        _imguiKeyMap.Add(Keys.D1, ImGuiKey._1);
        _imguiKeyMap.Add(Keys.D2, ImGuiKey._2);
        _imguiKeyMap.Add(Keys.D3, ImGuiKey._3);
        _imguiKeyMap.Add(Keys.D4, ImGuiKey._4);
        _imguiKeyMap.Add(Keys.D5, ImGuiKey._5);
        _imguiKeyMap.Add(Keys.D6, ImGuiKey._6);
        _imguiKeyMap.Add(Keys.D7, ImGuiKey._7);
        _imguiKeyMap.Add(Keys.D8, ImGuiKey._8);
        _imguiKeyMap.Add(Keys.D9, ImGuiKey._9);

        _imguiKeyMap.Add(Keys.KeyPad0, ImGuiKey.Keypad0);
        _imguiKeyMap.Add(Keys.KeyPad1, ImGuiKey.Keypad1);
        _imguiKeyMap.Add(Keys.KeyPad2, ImGuiKey.Keypad2);
        _imguiKeyMap.Add(Keys.KeyPad3, ImGuiKey.Keypad3);
        _imguiKeyMap.Add(Keys.KeyPad4, ImGuiKey.Keypad4);
        _imguiKeyMap.Add(Keys.KeyPad5, ImGuiKey.Keypad5);
        _imguiKeyMap.Add(Keys.KeyPad6, ImGuiKey.Keypad6);
        _imguiKeyMap.Add(Keys.KeyPad7, ImGuiKey.Keypad7);
        _imguiKeyMap.Add(Keys.KeyPad8, ImGuiKey.Keypad8);
        _imguiKeyMap.Add(Keys.KeyPad9, ImGuiKey.Keypad9);

        _imguiKeyMap.Add(Keys.F1, ImGuiKey.F1);
        _imguiKeyMap.Add(Keys.F2, ImGuiKey.F2);
        _imguiKeyMap.Add(Keys.F3, ImGuiKey.F3);
        _imguiKeyMap.Add(Keys.F4, ImGuiKey.F4);
        _imguiKeyMap.Add(Keys.F5, ImGuiKey.F5);
        _imguiKeyMap.Add(Keys.F6, ImGuiKey.F6);
        _imguiKeyMap.Add(Keys.F7, ImGuiKey.F7);
        _imguiKeyMap.Add(Keys.F8, ImGuiKey.F8);
        _imguiKeyMap.Add(Keys.F9, ImGuiKey.F9);
        _imguiKeyMap.Add(Keys.F10, ImGuiKey.F10);
        _imguiKeyMap.Add(Keys.F11, ImGuiKey.F11);
        _imguiKeyMap.Add(Keys.F12, ImGuiKey.F12);
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
}