using HaroohiePals.KCollision.Formats;
using HaroohiePals.MarioKartToolbox.KCollision;
using HaroohiePals.Mathematics;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.KCollision;

internal sealed class KclViewerContext
{
    public MkdsKcl Collision { get; }

    public Triangle[] Triangles { get; }

    public MkdsKclPrism[] PrismData { get; }

    public KclViewerContext(MkdsKcl collision)
    {
        Collision = collision;
        Triangles = collision.PrismData.Select(p => p.ToTriangle(Collision)).ToArray();
        PrismData = collision.PrismData.Select(x => new MkdsKclPrism(x)).ToArray();
    }
}