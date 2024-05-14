using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui.Themes;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using Image = OpenTK.Windowing.Common.Input.Image;

namespace HaroohiePals.Gui;

public abstract class ImGuiGameWindow : GameWindow
{
    private static readonly Color4 ClearColor = new Color4(0, 32, 48, 255);

    private ImGuiController _controller;
    private float _uiScale;
    protected IReadOnlyCollection<ImGuiIcon> _appIcons;

    public ImGuiGameWindow() : this(ImGuiGameWindowSettings.Default) { }
    public ImGuiGameWindow(ImGuiGameWindowSettings settings)
        : base(GameWindowSettings.Default, new NativeWindowSettings
    {
        Title = settings.Title,
        Size = settings.Size,
        APIVersion = new Version(4, 0),
        Profile = ContextProfile.Core,
        Flags = ContextFlags.ForwardCompatible
    })
    {
        _appIcons = settings.AppIcons;
        _uiScale = settings.UiScale;
    }

    protected abstract void RenderLayout(FrameEventArgs args);

    protected override void OnLoad()
    {
        base.OnLoad();

        GLContext.Current = new GLContext();

        _controller = new ImGuiController(ClientSize.X, ClientSize.Y, _uiScale, _appIcons.ToArray());

        ImGuiThemeManager.Init();

        VSync = VSyncMode.Adaptive;

        SetWindowsDarkTitleBar();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GLContext.Current.CollectGarbage();

        _controller.WindowResized(ClientSize.X, ClientSize.Y);

        _controller.Update(this, (float)args.Time);

        GL.ClearColor(ClearColor);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        RenderLayout(args);

        _controller.Render();

        SetCursor();

        Util.CheckGLError("End of frame");

        SwapBuffers();
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);

        _controller.PressInputChar((char)e.Unicode);
    }

    protected void SetIcon(params byte[][] iconFiles)
    {
        Icon = new WindowIcon(iconFiles.Select(GetImage).ToArray());
    }

    private void SetWindowsDarkTitleBar()
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            return;

        try
        {
            unsafe
            {
                if (Win32Util.ShouldSystemUseDarkMode())
                    Win32Util.ApplyImmersiveDarkModeOnWindow(GLFW.GetWin32Window(WindowPtr));
            }
        }
        catch
        {

        }
    }

    private bool TryGetImageBytes(byte[] pngFile, out byte[] imageBytes, out int width, out int height)
    {
        try
        {
            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(pngFile))
            {
                imageBytes = new byte[image.Width * image.Height * 4];

                width = image.Width;
                height = image.Height;

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        var pixel = image[x, y];
                        int offset = x * 4 + (y * image.Width * 4);
                        imageBytes[0 + offset] = pixel.R;
                        imageBytes[1 + offset] = pixel.G;
                        imageBytes[2 + offset] = pixel.B;
                        imageBytes[3 + offset] = pixel.A;
                    }
                }

                return true;
            }
        }
        catch
        {
            width = 0;
            height = 0;
            imageBytes = null;
            return false;
        }
    }

    private Image GetImage(byte[] pngFile)
    {
        if (TryGetImageBytes(pngFile, out var data, out int width, out int height))
            return new Image(width, height, data);
        return null;
    }

    private void SetCursor()
    {
        var cursor = ImGui.GetMouseCursor();
        switch (cursor)
        {
            case ImGuiMouseCursor.None:
                Cursor = MouseCursor.Empty;
                break;
            case ImGuiMouseCursor.Arrow:
                Cursor = MouseCursor.Default;
                break;
            case ImGuiMouseCursor.TextInput:
                Cursor = MouseCursor.IBeam;
                break;
            case ImGuiMouseCursor.ResizeAll:
                Cursor = MouseCursor.Crosshair;
                break;
            case ImGuiMouseCursor.ResizeNS:
                Cursor = MouseCursor.VResize;
                break;
            case ImGuiMouseCursor.ResizeEW:
                Cursor = MouseCursor.HResize;
                break;
            // case ImGuiMouseCursor.ResizeNESW:
            //     break;
            // case ImGuiMouseCursor.ResizeNWSE:
            //     break;
            case ImGuiMouseCursor.Hand:
                Cursor = MouseCursor.Hand;
                break;
            // case ImGuiMouseCursor.NotAllowed:
            // break;
            // case ImGuiMouseCursor.COUNT:
            // break;
            default:
                Cursor = MouseCursor.Default;
                break;
        }
    }
}
