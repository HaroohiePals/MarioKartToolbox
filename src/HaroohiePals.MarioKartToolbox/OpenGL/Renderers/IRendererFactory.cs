using HaroohiePals.Graphics3d.OpenGL.Renderers;

namespace HaroohiePals.MarioKartToolbox.OpenGL.Renderers;

interface IRendererFactory
{
    MeshRenderer CreateCircleRenderer();
    MeshRenderer CreateBoxRenderer();
    MeshRenderer CreateBoxAreaRenderer(bool render2d);
    MeshRenderer CreateCylinderAreaRenderer(bool render2d);
    MeshRenderer CreateKartRenderer(bool render2d);
}