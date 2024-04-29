using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;

public sealed class G3dTextureSrtAnimation : G3dAnimation
{
    public G3dTextureSrtAnimation(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        Header = new G3dAnimationHeader(er, 'M', "AT");
        NrFrames = er.Read<ushort>();
        Flags = (G3dTextureSrtAnimationFlags)er.Read<byte>();
        TexMtxMode = er.Read<byte>();
        Dictionary = new G3dDictionary<TextureSrtAnimationDictionaryData>(er);
        //resize frame data arrays to the right size
        foreach (var dictionaryItem in Dictionary)
        {
            var entry = dictionaryItem.Data;
            if (entry.ScaleSValues != null)
            {
                int stepSize = ((AnimationStep)((entry.ScaleS & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >>
                                                                TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT)).GetStepSize();
                int lastInterp = (int)(entry.ScaleS & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK);
                Array.Resize(ref entry.ScaleSValues, lastInterp / stepSize + (NrFrames - lastInterp));
            }

            if (entry.ScaleTValues != null)
            {
                int stepSize = ((AnimationStep)((entry.ScaleT & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >>
                                                                TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT)).GetStepSize();
                int lastInterp = (int)(entry.ScaleT & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK);
                Array.Resize(ref entry.ScaleTValues, lastInterp / stepSize + (NrFrames - lastInterp));
            }

            if (entry.RotationSinValues != null && entry.RotationCosValues != null)
            {
                int stepSize = ((AnimationStep)((entry.Rotation & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >>
                                                                  TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT)).GetStepSize();
                int lastInterp = (int)(entry.Rotation & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK);
                Array.Resize(ref entry.RotationSinValues, lastInterp / stepSize + (NrFrames - lastInterp));
                Array.Resize(ref entry.RotationCosValues, lastInterp / stepSize + (NrFrames - lastInterp));
            }

            if (entry.TranslationSValues != null)
            {
                int stepSize = ((AnimationStep)((entry.TranslationS & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >>
                                                                      TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT)).GetStepSize();
                int lastInterp = (int)(entry.TranslationS & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK);
                Array.Resize(ref entry.TranslationSValues, lastInterp / stepSize + (NrFrames - lastInterp));
            }

            if (entry.TranslationTValues != null)
            {
                int stepSize = ((AnimationStep)((entry.TranslationT & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >>
                                                                      TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT)).GetStepSize();
                int lastInterp = (int)(entry.TranslationT & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK);
                Array.Resize(ref entry.TranslationTValues, lastInterp / stepSize + (NrFrames - lastInterp));
            }
        }

        er.EndChunk();
    }

    public void Write(EndianBinaryWriterEx er)
    {
        er.BeginChunk();
        {
            Header.Write(er);
            er.Write(NrFrames);
            er.Write(Flags);
            er.Write(TexMtxMode);
            Dictionary.Write(er);

            foreach (var item in Dictionary)
            {
                item.Data.WriteData(er);
            }
        }
        er.EndChunk();
    }

    public G3dTextureSrtAnimationFlags Flags;
    public byte TexMtxMode;
    public G3dDictionary<TextureSrtAnimationDictionaryData> Dictionary;

    public override void InitializeAnimationObject(G3dAnimationObject animationObject, G3dModel model)
    {
        animationObject.AnimationFunction = (AnimateMaterialFunction)CalculateNsbtaAnimation;
        animationObject.MapData = new ushort[model.Info.MaterialCount];

        for (int i = 0; i < Dictionary.Count; i++)
        {
            if (model.Materials.MaterialDictionary.Contains(Dictionary[i].Name))
            {
                animationObject.MapData[model.Materials.MaterialDictionary.IndexOf(Dictionary[i].Name)] =
                    (ushort)(i | G3dAnimationObject.MapDataExist);
            }
        }
    }

    public static void CalculateNsbtaAnimation(MaterialAnimationResult result, G3dAnimationObject anmObj, uint dataIdx)
    {
        var animationResource = (G3dTextureSrtAnimation)anmObj.AnimationResource;
        animationResource.GetTexSRTAnm((ushort)dataIdx, (uint)Math.Floor(anmObj.Frame), result);
        result.PrmTexImage &= ~0xC0000000;
        result.PrmTexImage |= (uint)GxTexGen.TexCoord << 30;
        result.Flag |= MaterialAnimationResultFlag.TextureMatrixSet;
    }

    private void GetTexSRTAnm(ushort idx, uint frame, MaterialAnimationResult result)
    {
        var anmData = Dictionary[idx].Data;
        var flag = result.Flag;

        double translationS = GetTranslationScaleValue(anmData.TranslationS,
            anmData.TranslationSEx, anmData.TranslationSValues, frame);
        double translationT = GetTranslationScaleValue(anmData.TranslationT,
            anmData.TranslationTEx, anmData.TranslationTValues, frame);

        if (translationS == 0 && translationT == 0)
        {
            flag |= MaterialAnimationResultFlag.TextureMatrixTranslationZero;
        }
        else
        {
            flag &= ~MaterialAnimationResultFlag.TextureMatrixTranslationZero;
            result.TranslationS = translationS;
            result.TranslationT = translationT;
        }

        var (rotationSin, rotationCos) = GetRotationValue(anmData.Rotation, anmData.RotationEx,
            anmData.RotationSinValues, anmData.RotationCosValues, frame);

        if (rotationSin == 0 && rotationCos == 1.0)
        {
            flag |= MaterialAnimationResultFlag.TextureMatrixRotationZero;
        }
        else
        {
            result.RotationSin = rotationSin;
            result.RotationCos = rotationCos;
            flag &= ~MaterialAnimationResultFlag.TextureMatrixRotationZero;
        }

        double scaleS = GetTranslationScaleValue(anmData.ScaleS, anmData.ScaleSEx, anmData.ScaleSValues, frame);
        double scaleT = GetTranslationScaleValue(anmData.ScaleT, anmData.ScaleTEx, anmData.ScaleTValues, frame);

        if (scaleS == 1 && scaleT == 1)
        {
            flag |= MaterialAnimationResultFlag.TextureMatrixScaleOne;
        }
        else
        {
            flag &= ~MaterialAnimationResultFlag.TextureMatrixScaleOne;
            result.ScaleS = scaleS;
            result.ScaleT = scaleT;
        }

        result.Flag = flag;
    }

    private static double GetTranslationScaleValue(uint info, uint data, double[] arrayData, uint frame)
    {
        if ((info & TextureSrtAnimationDictionaryData.ELEM_CONST) != 0)
        {
            return (int)data / 4096.0;
        }

        return G3dAnimationUtil.CalculateAnimationValueFrame(frame, arrayData,
            (AnimationStep)((info & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >> TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT),
            info & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK);
    }

    private static (double sin, double cos) GetRotationValue(uint info, uint data, double[] sinData,
        double[] cosData, uint frame)
    {
        if ((info & TextureSrtAnimationDictionaryData.ELEM_CONST) != 0)
        {
            return ((short)data / 4096.0, (short)(data >> 16) / 4096.0);
        }

        return (
            G3dAnimationUtil.CalculateAnimationValueFrame(frame, sinData,
                (AnimationStep)((info & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >> TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT),
                info & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK),
            G3dAnimationUtil.CalculateAnimationValueFrame(frame, cosData,
                (AnimationStep)((info & TextureSrtAnimationDictionaryData.ELEM_STEP_MASK) >> TextureSrtAnimationDictionaryData.ELEM_STEP_SHIFT),
                info & TextureSrtAnimationDictionaryData.ELEM_LAST_INTERP_MASK));
    }
}
