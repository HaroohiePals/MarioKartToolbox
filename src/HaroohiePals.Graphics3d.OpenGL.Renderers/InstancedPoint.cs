using OpenTK.Mathematics;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

public struct InstancedPoint
{
    public Matrix4 Transform;
    public Color4 Color;
    public bool UseTexture;
    public object Source;
    public float TexCoordAngle;
    public uint PickingId = 0xFFFFFFF;
    public bool IsHovered;
    public bool IsSelected;

    public InstancedPoint(Vector3 position, Vector3 rotation, Vector3 scale, Color4 color, bool useTexture, object source, uint pickingId, bool isHovered, bool isSelected, float texCoordAngle = 0f)
    {
        Transform = Matrix4.CreateScale(scale) *
                Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(rotation.X)) *
                Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(rotation.Y)) *
                Matrix4.CreateRotationZ((float)MathHelper.DegreesToRadians(rotation.Z)) *
                Matrix4.CreateTranslation(position);
        Color = color;
        UseTexture = useTexture;
        Source = source;
        TexCoordAngle = texCoordAngle;
        PickingId = pickingId;
        IsHovered = isHovered;
        IsSelected = isSelected;
    }

    public InstancedPoint(Matrix4 transform, Color4 color, bool useTexture, object source, uint pickingId, bool isHovered, bool isSelected, float texCoordAngle = 0f)
    {
        Transform = transform;
        Color = color;
        UseTexture = useTexture;
        Source = source;
        TexCoordAngle = texCoordAngle;
        PickingId = pickingId;
        IsHovered = isHovered;
        IsSelected = isSelected;
    }
}
