namespace HaroohiePals.Nitro.G2
{
    public enum GxOamShape
    {
        Shape8x8   = (0 << GxOamAttr.OamAttr01ShapeShift) | (0 << GxOamAttr.OamAttr01SizeShift),
        Shape16x16 = (0 << GxOamAttr.OamAttr01ShapeShift) | (1 << GxOamAttr.OamAttr01SizeShift),
        Shape32x32 = (0 << GxOamAttr.OamAttr01ShapeShift) | (2 << GxOamAttr.OamAttr01SizeShift),
        Shape64x64 = (0 << GxOamAttr.OamAttr01ShapeShift) | (3 << GxOamAttr.OamAttr01SizeShift),
        Shape16x8  = (1 << GxOamAttr.OamAttr01ShapeShift) | (0 << GxOamAttr.OamAttr01SizeShift),
        Shape32x8  = (1 << GxOamAttr.OamAttr01ShapeShift) | (1 << GxOamAttr.OamAttr01SizeShift),
        Shape32x16 = (1 << GxOamAttr.OamAttr01ShapeShift) | (2 << GxOamAttr.OamAttr01SizeShift),
        Shape64x32 = (1 << GxOamAttr.OamAttr01ShapeShift) | (3 << GxOamAttr.OamAttr01SizeShift),
        Shape8x16  = (2 << GxOamAttr.OamAttr01ShapeShift) | (0 << GxOamAttr.OamAttr01SizeShift),
        Shape8x32  = (2 << GxOamAttr.OamAttr01ShapeShift) | (1 << GxOamAttr.OamAttr01SizeShift),
        Shape16x32 = (2 << GxOamAttr.OamAttr01ShapeShift) | (2 << GxOamAttr.OamAttr01SizeShift),
        Shape32x64 = (2 << GxOamAttr.OamAttr01ShapeShift) | (3 << GxOamAttr.OamAttr01SizeShift)
    }

    public enum GxOamColorMode
    {
        Color16  = 0,
        Color256 = 1
    }

    public enum GxOamEffect
    {
        None  = 0,
        FlipH = 1 << GxOamAttr.OamAttr01HFShift,
        FlipV = 1 << GxOamAttr.OamAttr01VFShift,

        FlipHV       = (1 << GxOamAttr.OamAttr01HFShift) | (1 << GxOamAttr.OamAttr01VFShift),
        Affine       = (1 << GxOamAttr.OamAttr01RSEnableShift),
        NoDisplay    = (2 << GxOamAttr.OamAttr01RSEnableShift),
        AffineDouble = (3 << GxOamAttr.OamAttr01RSEnableShift)
    }

    public enum GxOamMode
    {
        Normal    = 0,
        Xlu       = 1,
        ObjWnd    = 2,
        BitmapObj = 3
    }

    public enum GxObjVramModeChar
    {
        Mode2D      = 0,
        Mode1D_32K  = 16,
        Mode1D_64K  = 17,
        Mode1D_128K = 18,
        Mode1D_256K = 19
    };
}