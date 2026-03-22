using HaroohiePals.Graphics3d.OpenGL;
using HaroohiePals.Gui;
using HaroohiePals.Gui.View.Modal;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Reflection;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main;

class AboutModalView : ModalView
{
    private const string WINDOW_TITLE = "About Mario Kart Toolbox";
    private const string CAPTION = "Mario Kart Toolbox";
    private const string GITHUB_LINK = "https://github.com/HaroohiePals/MarioKartToolbox";
    private const string COPYRIGHT_INFO = "© 2015-2026 HaroohiePals";
    
    private GLTexture _iconTexture;
    private bool _autoResized;

    public AboutModalView()
        : base(WINDOW_TITLE, new System.Numerics.Vector2(ImGuiEx.CalcUiScaledValue(400), ImGuiEx.CalcUiScaledValue(470)))
    {
        LoadIconTexture();
    }

    private void LoadIconTexture()
    {
        using var texImage = Image.Load<Rgba32>(Resources.Icons.main);
        byte[] data = new byte[texImage.Width * texImage.Height * 8];
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

    protected override void DrawContent()
    {
        //Auto-resize
        if (!_autoResized)
        {
            ImGui.SetWindowSize(new System.Numerics.Vector2(0, 0));
            _autoResized = true;
        }

        string informationalVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "No version";

        string buildInfo = $"Version: {informationalVersion}";

        float availX = ImGui.GetWindowSize().X;

        float scale = ImGuiEx.GetUiScale();
        var imageSize = new System.Numerics.Vector2(128 * scale, 128 * scale);

        ImGui.SetCursorPosX((availX - imageSize.X) / 2);
        ImGui.Image(_iconTexture.Handle, imageSize);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(CAPTION).X) / 2);
        ImGui.TextUnformatted(CAPTION);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(COPYRIGHT_INFO).X) / 2);
        ImGui.TextUnformatted(COPYRIGHT_INFO);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(buildInfo).X) / 2);
        ImGui.TextUnformatted(buildInfo);

        ImGui.SetCursorPosX((availX - ImGui.CalcTextSize(GITHUB_LINK).X) / 2);
        ImGui.TextLinkOpenURL(GITHUB_LINK);

        ImGui.SeparatorText("Staff");
        {
            DrawSocialMediaBulletText("Gericom", "@gericom", "https://github.com/Gericom", 
                "Programming");
            DrawSocialMediaBulletText("Ermelber", "@ermiisoft.net", "https://bsky.app/profile/ermiisoft.net",
                "Programming");
            DrawSocialMediaBulletText("Rocoloco", "@rocoloco321", "https://bsky.app/profile/rocoloco321.bsky.social",
                "Programming");
            DrawSocialMediaBulletText("SuperGameCube", "@supergamecube", "https://bsky.app/profile/supergamecube.bsky.social",
                "Ideas");
            DrawSocialMediaBulletText("Daniel", "@kaasiand.cool", "https://bsky.app/profile/kaasiand.cool",
                "Icons");
            DrawSocialMediaBulletText("Jacanapes", "@jacanapes_", "https://x.com/jacanapes_",
                "Item Box Models");
            ImGui.BulletText($"Mario Kart DS Modding Discord - Testing");
        }

        ImGui.SeparatorText("Software Credits");
        {
            ImGui.BulletText($"dear imgui ({ImGui.GetVersion()}): ");
            ImGui.SameLine(0, 0);
            ImGui.TextLinkOpenURL("https://github.com/ocornut/imgui");

            ImGui.BulletText($"ImGuizmo: ");
            ImGui.SameLine(0, 0);
            ImGui.TextLinkOpenURL("https://github.com/CedricGuillemet/ImGuizmo");

            ImGui.BulletText($"A complete Credits list can be found on the GitHub page.");
        }
    }

    private void DrawSocialMediaBulletText(string name, string socialMediaName, string socialMediaUrl, string role)
    {
        ImGui.BulletText($"{name} (");
        ImGui.SameLine(0, 0);
        ImGui.TextLinkOpenURL(socialMediaName, socialMediaUrl);
        ImGui.SameLine(0, 0);
        ImGui.Text($") - {role}");
    }

    protected override void OnClose()
    {
        _iconTexture?.Dispose();
    }
}