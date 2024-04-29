using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

class AboutModalView : ModalView
{
    private const string WINDOW_TITLE = "About Mario Kart Toolbox";

    private GLTexture _iconTexture;
    private bool _autoResized = false;

    public AboutModalView()
        : base(WINDOW_TITLE, new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(400), ImGuiEx.CalcUiScaledValue(430)))
    {
        LoadIconTexture();
    }

    private void LoadIconTexture()
    {
        using (var texImage = Image.Load<Rgba32>(Resources.Icons.main))
        {
            var data = new byte[texImage.Width * texImage.Height * 8];
            texImage.CopyPixelDataTo(data);
            _iconTexture = new GLTexture(PixelInternalFormat.Rgba8, texImage.Width, texImage.Height, PixelFormat.Rgba,
                PixelType.UnsignedByte, data);
            _iconTexture.Use();
            _iconTexture.SetWrapMode(Graphics3d.TextureWrapMode.Clamp, Graphics3d.TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, -2.0f);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    protected override void DrawContent()
    {
        //Auto-resize
        if (!_autoResized)
        {
            ImGui.SetWindowSize(new System.Numerics.Vector2(0, 0));
            _autoResized = true;
        }

        string informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        string caption = "Mario Kart Toolbox";
        string githubLink = "https://github.com/HaroohiePals/MarioKartToolbox";
        string buildInfo = $"Version: {informationalVersion}";
        string madeByInfo = "© 2015-2023 HaroohiePals";

        float availX = ImGui.GetWindowSize().X;

        float scale = ImGuiEx.GetUiScale();
        var imageSize = new System.Numerics.Vector2(128 * scale, 128 * scale);

        ImGui.SetCursorPosX((availX - imageSize.X) / 2);
        ImGui.Image(_iconTexture.Handle, imageSize);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(caption).X) / 2);
        ImGui.TextUnformatted(caption);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(madeByInfo).X) / 2);
        ImGui.TextUnformatted(madeByInfo);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(buildInfo).X) / 2);
        ImGui.TextUnformatted(buildInfo);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(githubLink).X) / 2);
        RenderLink(githubLink);

        if (ImGui.CollapsingHeader("Staff", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BulletText($"Gericom - Programming");

            ImGui.BulletText($"Ermelber - Programming");

            ImGui.BulletText($"Daniel (");
            ImGui.SameLine(0, 0);
            RenderLink("https://twitter.com/kaasiand", "@kaasiand");
            ImGui.SameLine(0, 0);
            ImGui.Text(") - Icons");

            ImGui.BulletText($"SuperGameCube - Testing");

            ImGui.BulletText($"Mario Kart DS Modding Discord - Testing");
        }

        if (ImGui.CollapsingHeader("Software Credits", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.BulletText($"dear imgui ({ImGui.GetVersion()}): ");
            ImGui.SameLine(0, 0);
            RenderLink("https://github.com/ocornut/imgui");

            ImGui.BulletText($"ImGuizmo: ");
            ImGui.SameLine(0, 0);
            RenderLink("https://github.com/CedricGuillemet/ImGuizmo");


            ImGui.BulletText($"RiiStudio: ");
            ImGui.SameLine(0, 0);
            RenderLink("https://github.com/riidefi/RiiStudio");
        }


        //ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(" GitHub ").X) / 2);
        //if (ImGui.Button("GitHub##MKTB"))
        //    OpenBrowser("https://github.com/HaroohiePals/MarioKartToolbox");
    }

    private void OpenLink(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    private HashSet<string> _hoveredLinks = new HashSet<string>();

    private void RenderLink(string url, string text = null)
    {
        if (text == null)
            text = url;
        ImGui.PushStyleColor(ImGuiCol.Text, _hoveredLinks.Contains(url) ? ImGui.GetColorU32(ImGuiCol.ButtonHovered) : ImGui.GetColorU32(ImGuiCol.ButtonActive));
        ImGui.TextUnformatted(text);

        if (ImGui.IsItemHovered())
        {
            _hoveredLinks.Add(url);
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        else
        {
            _hoveredLinks.Remove(url);
        }

        if (ImGui.IsItemClicked())
            OpenLink(url);
        ImGui.PopStyleColor();
    }

    protected override void OnClose()
    {
        _iconTexture?.Dispose();
    }
}