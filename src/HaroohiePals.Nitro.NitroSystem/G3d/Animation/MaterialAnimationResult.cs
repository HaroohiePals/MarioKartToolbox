using HaroohiePals.Nitro.G3;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Animation;

public sealed class MaterialAnimationResult
{
    public MaterialAnimationResultFlag Flag;
    public uint PrmMatColor0;
    public uint PrmMatColor1;
    public GxPolygonAttr PrmPolygonAttr;
    public GxTexImageParam PrmTexImage;
    public uint PrmTexPltt;

    public MaterialTextureInfo TextureInfo;

    public double ScaleS, ScaleT;
    public double RotationSin, RotationCos;
    public double TranslationS, TranslationT;

    public ushort OriginalWidth, OriginalHeight;
    public double MagW, MagH;

    public void Clear()
    {
        Flag = 0;
        PrmMatColor0 = 0;
        PrmMatColor1 = 0;
        PrmPolygonAttr = 0;
        PrmTexImage = 0;
        PrmTexPltt = 0;
        ScaleS = 0;
        ScaleT = 0;
        RotationSin = 0;
        RotationCos = 0;
        TranslationS = 0;
        TranslationT = 0;
        OriginalWidth = 0;
        OriginalHeight = 0;
        MagW = 0;
        MagH = 0;

        TextureInfo = null;
    }
}