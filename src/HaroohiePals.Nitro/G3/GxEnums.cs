namespace HaroohiePals.Nitro.G3
{
    public enum GxPolygonMode
    {
        Modulate      = 0,
        Decal         = 1,
        ToonHighlight = 2,
        Shadow        = 3
    }

    public enum GxCull
    {
        All   = 0,
        Front = 1,
        Back  = 2,
        None  = 3
    }

    public enum GxBegin
    {
        Triangles     = 0,
        Quads         = 1,
        TriangleStrip = 2,
        QuadStrip     = 3
    }

    public enum GxTexGen
    {
        None     = 0,
        TexCoord = 1,
        Normal   = 2,
        Vertex   = 3
    }

    public enum GxMtxMode
    {
        Projection     = 0,
        Position       = 1,
        PositionVector = 2,
        Texture        = 3
    }

    public enum GxSortMode
    {
        Auto = 0,
        Manual = 1
    }

    public enum GxBufferMode
    {
        Z = 0,
        W = 1
    }
}