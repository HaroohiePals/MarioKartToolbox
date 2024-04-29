using HaroohiePals.Gui.View;
using HaroohiePals.Gui.Viewport;
using HaroohiePals.Gui.Viewport.Framebuffers;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.KCollision;
using HaroohiePals.MarioKartToolbox.Gui.Viewport;
using OpenTK.Mathematics;
using NativeFileDialogs.Net;
using HaroohiePals.Graphics3d;
using HaroohiePals.Gui.View.Menu;
using ImGuiNET;
using System.Runtime.CompilerServices;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace HaroohiePals.KclViewer.Gui;

internal class KclViewerContentView : WindowContentView, IDisposable
{
    private static readonly Color4 DefaultBgColor = new(32, 44, 55, 255);

    private readonly KclViewerContext _context;
    private readonly InteractiveViewportPanel _viewportPanel;
    private readonly KclPrismRenderGroup _prismRenderGroup;
    private readonly KclOctreeRenderGroup _octreeRenderGroup;
    private readonly TestGLFramebufferProvider _framebufferProvider;

    private readonly DockSpaceView _dockSpaceView = new();

    private static readonly string[] MkdsColTypes =
    {
        "road",
        "slippery road",
        "weak off road",
        "off road",
        "sound trigger",
        "heavy off road",
        "slippery road 2",
        "boost pad",
        "wall",
        "wall (not for cameras)",
        "out of bounds",
        "fall boundary",
        "jump pad",
        "road 2",
        "wall (no knockback)",
        "cannon activator",
        "edge wall",
        "falls water",
        "boost pad 2",
        "looping",
        "sticky road",
        "wall 3",
        "force recalculate route"
    };

    public override MenuItem[] MenuItems =>
    [
        new("File")
        {
            Items = new()
            {
                new("Export to OBJ", ExportToObj)
            }
        }
    ];

    public KclViewerContentView(Kcl kcl)
    {
        _context = new KclViewerContext(kcl);
        _framebufferProvider = new TestGLFramebufferProvider(DefaultBgColor, true, true);
        var scene = new RenderGroupScenePerspective(_framebufferProvider);
        _viewportPanel = new InteractiveViewportPanel(scene);

        if (kcl is MkwiiKcl)
        {
            // mkwii has a larger scale
            scene.Projection.Near = 40;
            scene.Projection.Far = 256000;
            _viewportPanel.CameraControls.ControlSpeed = 10;
        }
        else if (kcl is SmgKcl)
        {
            scene.Projection.Near = 8;
            scene.Projection.Far = 51200;
            _viewportPanel.CameraControls.ControlSpeed = 2;
        }

        _prismRenderGroup = new KclPrismRenderGroup(_context);
        scene.RenderGroups.Add(_prismRenderGroup);
        _octreeRenderGroup = new KclOctreeRenderGroup(_context);
        _octreeRenderGroup.Enabled = false;
        scene.RenderGroups.Add(_octreeRenderGroup);

        _viewportPanel.Context.ViewMatrix = Matrix4.Identity;

    }

    private void RenderPerspectiveWindow()
    {
        if (ImGui.Begin("Perspective Settings"))
        {
            ImGui.ColorEdit4("Face", ref Unsafe.As<Color4, Vector4>(ref _prismRenderGroup.FaceColor));
            ImGui.ColorEdit4("Edge", ref Unsafe.As<Color4, Vector4>(ref _prismRenderGroup.EdgeColor));
            var color = _framebufferProvider.BgColor;
            ImGui.ColorEdit4("Bg", ref Unsafe.As<Color4, Vector4>(ref color));
            _framebufferProvider.BgColor = color;

            ImGui.PushItemWidth(100);
            int coloringMode = (int)_prismRenderGroup.ColoringMode;
            ImGui.Combo("Coloring mode", ref coloringMode, new[] { "Solid", "Heat Map" }, 2);
            _prismRenderGroup.ColoringMode = (KclPrismRenderGroup.PrismColoringMode)coloringMode;
            ImGui.PopItemWidth();

            bool octreeEnabled = _octreeRenderGroup.Enabled;
            ImGui.Checkbox("Show octree", ref octreeEnabled);
            _octreeRenderGroup.Enabled = octreeEnabled;

            ImGui.End();
        }

        if (_viewportPanel is not null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
            bool begin = ImGui.Begin("Perspective");
            ImGui.PopStyleVar();

            if (begin)
            {
                ImGui.SetWindowSize(new Vector2(600, 400), ImGuiCond.Once);

                _viewportPanel.Draw();

                if (_viewportPanel.Context.HoverObject?.Object is KclPrism prism)
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted($"0x{prism.Attribute:X4}");
                    if (_context.Collision is MkdsKcl)
                    {
                        int colType = (prism.Attribute >> 8) & 0x1F;
                        if (colType < MkdsColTypes.Length)
                            ImGui.TextUnformatted($"Col type {colType} ({MkdsColTypes[colType]})");
                        else
                            ImGui.TextUnformatted($"Col type {colType}");
                        ImGui.TextUnformatted($"Variant {(prism.Attribute >> 5) & 7}");
                        ImGui.TextUnformatted($"Light {(prism.Attribute >> 2) & 3}");
                        if ((prism.Attribute & (1 << 1)) != 0)
                            ImGui.TextUnformatted("Map shadow");
                        if ((prism.Attribute & (1 << 4)) != 0)
                            ImGui.TextUnformatted("Ignore drivers");
                        if ((prism.Attribute & (1 << 13)) != 0)
                            ImGui.TextUnformatted("Ignore items");
                        if ((prism.Attribute & (1 << 14)) != 0)
                            ImGui.TextUnformatted("Wall");
                        if ((prism.Attribute & (1 << 15)) != 0)
                            ImGui.TextUnformatted("Floor");
                    }

                    ImGui.EndTooltip();
                }

                ImGui.End();
            }
        }
    }

    private void RenderOctreeStatisticsWindow()
    {
        if (ImGui.Begin("Octree statistics"))
        {
            ImGui.Text("Prisms per leaf");
            ImGui.PushItemWidth(-1);
            ImGui.PlotHistogram("", ref _context.LeafHistogram[0], _context.LeafHistogram.Length, 0,
                null, float.MaxValue, float.MaxValue, new Vector2(0, 100));
            ImGui.PopItemWidth();

            ImGui.Text("Max: " + (_context.LeafHistogram.Length - 1));

            ImGui.Text("Min cube size: " + _context.MinCubeSize);
            ImGui.Text("Max cube size: " + _context.Collision.OctreeRootCubeSize);

            ImGui.End();
        }
    }

    public override bool Draw()
    {
        _dockSpaceView.Draw();

        RenderPerspectiveWindow();
        RenderOctreeStatisticsWindow();

        return true;
    }

    private void ExportToObj()
    {
        var result = Nfd.SaveDialog(out string outPath, new Dictionary<string, string> { { "Wavefront OBJ", "obj" } });

        if (result != NfdStatus.Ok)
            return;

        var obj = Obj.FromTriangles(_context.Triangles);
        for (int i = 0; i < _context.Collision.PrismData.Length; i++)
            obj.Faces[i].Material = $"{_context.Collision.PrismData[i].Attribute:X4}";

        obj.Write(File.Create(outPath));
    }

    public void Dispose()
    {
        _prismRenderGroup.Dispose();
        _octreeRenderGroup.Dispose();
    }

}
