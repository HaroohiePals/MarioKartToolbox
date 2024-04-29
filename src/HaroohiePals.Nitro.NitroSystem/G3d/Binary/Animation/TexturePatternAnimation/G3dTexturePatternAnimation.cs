using HaroohiePals.IO;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System;
using System.Text;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;

public sealed class G3dTexturePatternAnimation : G3dAnimation
{
    public G3dTexturePatternAnimation(EndianBinaryReaderEx er)
    {
        er.BeginChunk();
        Header = new G3dAnimationHeader(er, 'M', "PT");
        NrFrames = er.Read<ushort>();

        byte textureCount = er.Read<byte>();
        byte paletteCount = er.Read<byte>();

        ushort textureNamesOffset = er.Read<ushort>();
        ushort paletteNamesOffset = er.Read<ushort>();

        TextureNames = new string[textureCount];
        PaletteNames = new string[paletteCount];

        Dictionary = new G3dDictionary<TexturePatternAnimationDictionaryData>(er);

        er.JumpRelative(textureNamesOffset);
        for (int i = 0; i < textureCount; i++)
            TextureNames[i] = er.ReadString(Encoding.ASCII, 16).TrimEnd('\0');

        er.JumpRelative(paletteNamesOffset);
        for (int i = 0; i < paletteCount; i++)
            PaletteNames[i] = er.ReadString(Encoding.ASCII, 16).TrimEnd('\0');

        er.EndChunk();
    }

    public G3dDictionary<TexturePatternAnimationDictionaryData> Dictionary;
    public string[] TextureNames;
    public string[] PaletteNames;

    public override void InitializeAnimationObject(G3dAnimationObject animationObject, G3dModel model)
    {
        animationObject.AnimationFunction = (AnimateMaterialFunction)CalculateNsbtpAnimation;
        animationObject.MapData = new ushort[model.Info.MaterialCount];

        for (int i = 0; i < Dictionary.Count; i++)
        {
            if (model.Materials.MaterialDictionary.Contains(Dictionary[i].Name))
            {
                animationObject.MapData[model.Materials.MaterialDictionary.IndexOf(Dictionary[i].Name)]
                    = (ushort)(i | G3dAnimationObject.MapDataExist);
            }
        }
    }

    private static void CalculateNsbtpAnimation(MaterialAnimationResult result, G3dAnimationObject animationObject, uint dataIndex)
    {
        var animationResource = (G3dTexturePatternAnimation)animationObject.AnimationResource;

        double frame = Math.Clamp(animationObject.Frame, 0, animationResource.NrFrames - 1);

        var (frameValue, index) = animationResource.GetFrameValue((ushort)dataIndex, (uint)Math.Floor(frame));

        if (frameValue is not null)
        {
            SetTexParameters(animationObject, animationResource.TextureNames[frameValue.TextureIndex], result);

            result.TextureInfo = animationObject.FrameTextureInfos[dataIndex][index];

            // if (texFv.PlttIndex != 255)
            // SetPlttParameters(anmObj.ResTex, patAnm.PlttNames[texFv.PlttIndex], result);
        }
    }

    private static void SetTexParameters(G3dAnimationObject animationObject, string texName, MaterialAnimationResult result)
    {
        var tex = animationObject.TextureResource.TextureDictionary[texName];

        result.PrmTexImage.Address = 0;
        result.PrmTexImage.Format = 0;
        result.PrmTexImage.Address = 0;
        result.PrmTexImage.Color0Transparent = false;
        result.PrmTexImage.Width = 0;
        result.PrmTexImage.Height = 0;
        result.PrmTexImage |= tex.TexImageParam;

        result.OriginalWidth
            = (ushort)((tex.ExtraParam & TextureDictionaryData.ParamExOrigWMask) >> TextureDictionaryData.ParamExOrigWShift);
        result.OriginalHeight
            = (ushort)((tex.ExtraParam & TextureDictionaryData.ParamExOrigHMask) >> TextureDictionaryData.ParamExOrigHShift);

        result.MagW = 1.0;
        result.MagH = 1.0;
    }

    // private static void SetPaletteParameters(Tex0 textures, string paletteName, MatAnmResult result)
    // {
    //     //todo
    //     throw new NotImplementedException();
    // }

    private (TexturePatternFrameValue frameValue, uint index) GetFrameValue(ushort dataIdx, uint frame)
    {
        var animationData = Dictionary[dataIdx].Data;

        // initial guess
        uint frameValueIndex = (uint)Math.Floor(animationData.RatioDataFrame * frame);

        while (frameValueIndex > 0 && animationData.FrameValues[frameValueIndex].FrameNumber >= frame)
        {
            frameValueIndex--;
        }

        while (frameValueIndex + 1 < animationData.FrameValues.Length &&
                animationData.FrameValues[frameValueIndex + 1].FrameNumber <= frame)
        {
            frameValueIndex++;
        }

        return (animationData.FrameValues[frameValueIndex], frameValueIndex);
    }
}
