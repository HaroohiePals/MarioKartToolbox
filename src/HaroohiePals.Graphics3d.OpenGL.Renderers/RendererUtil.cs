using OpenTK.Mathematics;

namespace HaroohiePals.Graphics3d.OpenGL.Renderers;

internal static class RendererUtil
{
    public static VertexData[] GetVertexDataFromObj(byte[] objData)
    {
        var box = new Obj(objData);
        var vertices = new VertexData[box.Faces.Count * 3];
        int idx = 0;
        foreach (var face in box.Faces)
        {
            for (int i = 0; i < 3; i++)
            {
                var pos = box.Vertices[face.VertexIndices[i]];
                var tex = box.TexCoords[face.TexCoordIndices[i]];
                vertices[idx++] = new VertexData
                {
                    Position = (Vector3)pos,
                    TexCoord = (Vector2)tex
                };
            }
        }
        return vertices;
    }
}
