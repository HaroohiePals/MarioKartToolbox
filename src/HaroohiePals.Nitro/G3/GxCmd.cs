namespace HaroohiePals.Nitro.G3
{
    public enum GxCmd : byte
    {
        Nop = 0x00,

        MatrixMode    = 0x10,
        PushMatrix    = 0x11,
        PopMatrix     = 0x12,
        StoreMatrix   = 0x13,
        RestoreMatrix = 0x14,
        Identity      = 0x15,
        LoadMatrix44  = 0x16,
        LoadMatrix43  = 0x17,
        MultMatrix44  = 0x18,
        MultMatrix43  = 0x19,
        MultMatrix33  = 0x1A,
        Scale         = 0x1B,
        Translate     = 0x1C,

        Color         = 0x20,
        Normal        = 0x21,
        TexCoord      = 0x22,
        Vertex        = 0x23,
        VertexShort   = 0x24,
        VertexXY      = 0x25,
        VertexXZ      = 0x26,
        VertexYZ      = 0x27,
        VertexDiff    = 0x28,
        PolygonAttr   = 0x29,
        TexImageParam = 0x2A,
        TexPlttBase   = 0x2B,

        MaterialColor0 = 0x30,
        MaterialColor1 = 0x31,
        LightVector    = 0x32,
        LightColor     = 0x33,
        Shininess      = 0x34,

        Begin = 0x40,
        End   = 0x41,

        SwapBuffers = 0x50,

        Viewport = 0x60,

        BoxTest      = 0x70,
        PositionTest = 0x71,
        VectorTest   = 0x72
    }
}