using System.Numerics;

namespace HaroohiePals.Gui;

public struct ImRect
{
    public ImRect(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    public Vector2 Min;
    public Vector2 Max;

    public bool Contains(Vector2 p) => p.X >= Min.X && p.Y >= Min.Y && p.X < Max.X && p.Y < Max.Y;
    public void Expand(float amount) { Min.X -= amount; Min.Y -= amount; Max.X += amount; Max.Y += amount; }
}
