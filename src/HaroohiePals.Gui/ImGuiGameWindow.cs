#nullable enable
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
using System.Linq;
using System.Runtime.InteropServices;
using Image = OpenTK.Windowing.Common.Input.Image;

namespace HaroohiePals.Gui;

public abstract class ImGuiGameWindow(ImGuiGameWindowSettings settings) : GameWindow(GameWindowSettings.Default,
    new NativeWindowSettings
    {
        Title = settings.Title,
        ClientSize = settings.Size,
        APIVersion = new Version(4, 0),
        Profile = ContextProfile.Core,
        Flags = ContextFlags.ForwardCompatible
    })
{
    private static readonly Color4 ClearColor = new Color4(0, 32, 48, 255);
    private ImGuiController? _controller;

    protected ImGuiGameWindow() : this(ImGuiGameWindowSettings.Default) { }

    protected abstract void RenderLayout(FrameEventArgs args);

    protected override void OnLoad()
    {
        base.OnLoad();

        GLContext.Current = new GLContext();

        if (!TryGetCurrentMonitorScale(out float currentMonitorScaleX, out float currentMonitorScaleY))
        {
            currentMonitorScaleX = currentMonitorScaleY = 1f;
        }

        _controller = new ImGuiController(FramebufferSize.X, FramebufferSize.Y, 
            currentMonitorScaleX, currentMonitorScaleY, settings);

        ImGuiThemeManager.Init();

        VSync = VSyncMode.Adaptive;

        SetWindowsDarkTitleBar();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GLContext.Current.CollectGarbage();

        if (_controller is null)
            return;

        _controller.WindowResized(FramebufferSize.X, FramebufferSize.Y);

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

        _controller?.PressInputChar((char)e.Unicode);
    }

    protected void SetIcon(params byte[][] iconFiles)
    {
        // Workaround: Detect Wayland session on Linux
        // Wayland explicitly does not allow clients to set window icons like they can on X11 or Windows.
        // Instead, icons are typically handled by the desktop environment based on application metadata
        // (like .desktop files on Linux).
        if (Environment.OSVersion.Platform == PlatformID.Unix &&
            Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLowerInvariant() == "wayland")
            return;
        
        // Similarly to Wayland, macOS doesn't allow clients to set window icons
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return;
        
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
            // ignored
        }
    }

    private bool TryGetImageBytes(byte[] pngFile, out byte[] imageBytes, out int width, out int height)
    {
        try
        {
            using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(pngFile);
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
        catch
        {
            width = 0;
            height = 0;
            imageBytes = [];
            return false;
        }
    }

    private Image? GetImage(byte[] pngFile)
        => TryGetImageBytes(pngFile, out byte[] imageBytes, out int width, out int height)
            ? new Image(width, height, imageBytes)
            : null;

    private void SetCursor()
    {
        Cursor = ImGui.GetMouseCursor() switch
        {
            ImGuiMouseCursor.None => MouseCursor.Empty,
            ImGuiMouseCursor.Arrow => MouseCursor.Default,
            ImGuiMouseCursor.TextInput => MouseCursor.IBeam,
            ImGuiMouseCursor.ResizeAll => MouseCursor.Crosshair,
            ImGuiMouseCursor.ResizeNS => MouseCursor.ResizeNS,
            ImGuiMouseCursor.ResizeEW => MouseCursor.ResizeEW,
            ImGuiMouseCursor.ResizeNESW => MouseCursor.ResizeNESW,
            ImGuiMouseCursor.ResizeNWSE => MouseCursor.ResizeNWSE,
            ImGuiMouseCursor.Hand => MouseCursor.PointingHand,
            ImGuiMouseCursor.NotAllowed => MouseCursor.NotAllowed,
            _ => MouseCursor.Default
        };
    }
}