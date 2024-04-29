using HaroohiePals.Graphics;
using HaroohiePals.Graphics3d;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.Gx;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public abstract class G3dModelManager : IDisposable
{
    private class BufferCacheEntry
    {
        public int UseCount = 0;

        public readonly DisplayListBuffer[] ShapeProxies;

        public BufferCacheEntry(DisplayListBuffer[] shapeProxies)
        {
            ShapeProxies = shapeProxies;
        }
    }

    private class TextureCacheEntry
    {
        public int UseCount = 0;

        public readonly MaterialTextureInfo[] MatTexInfos;

        public TextureCacheEntry(MaterialTextureInfo[] matTexInfos)
        {
            MatTexInfos = matTexInfos;
        }
    }

    private class AnimTextureCacheEntry
    {
        public int UseCount = 0;

        public readonly MaterialTextureInfo[][] FrameTexInfos;

        public AnimTextureCacheEntry(MaterialTextureInfo[][] frameTexInfos)
        {
            FrameTexInfos = frameTexInfos;
        }
    }

    private readonly Dictionary<G3dModel, BufferCacheEntry> _bufferCache = [];
    private readonly Dictionary<(G3dModel, G3dTextureSet), TextureCacheEntry> _textureCache = [];
    private readonly Dictionary<(G3dTexturePatternAnimation, G3dTextureSet), AnimTextureCacheEntry> _animTextureCache = [];

    public void InitializeRenderObject(G3dRenderObject renderObject, G3dTextureSet textures)
    {
        if (renderObject.ModelResource is null)
            return;

        if (!_bufferCache.TryGetValue(renderObject.ModelResource, out var bufferCacheEntry))
        {
            var shapeProxies = new DisplayListBuffer[renderObject.ModelResource.Shapes.Shapes.Length];
            for (int i = 0; i < renderObject.ModelResource.Shapes.Shapes.Length; i++)
            {
                shapeProxies[i] = CreateDisplayListBuffer(renderObject.ModelResource.Shapes.Shapes[i].DisplayList);
            }

            bufferCacheEntry = new BufferCacheEntry(shapeProxies);
            _bufferCache.Add(renderObject.ModelResource, bufferCacheEntry);
        }

        bufferCacheEntry.UseCount++;
        renderObject.ShapeProxies = bufferCacheEntry.ShapeProxies;

        if (textures is not null)
        {
            renderObject.Textures = textures;

            var textureCacheKey = (renderObject.ModelResource, textures);
            if (!_textureCache.ContainsKey(textureCacheKey))
            {
                BindTextures(renderObject.ModelResource, textures);
            }

            _textureCache[textureCacheKey].UseCount++;
            renderObject.MaterialTextureInfos = _textureCache[textureCacheKey].MatTexInfos;
        }
    }

    public void CleanupRenderObject(G3dRenderObject renderObject)
    {
        if (renderObject.ModelResource is not null && renderObject.ShapeProxies is not null &&
            _bufferCache.TryGetValue(renderObject.ModelResource, out var bufferCacheEntry))
        {
            renderObject.ShapeProxies = null;
            if (--bufferCacheEntry.UseCount == 0)
            {
                foreach (var shapeProxy in bufferCacheEntry.ShapeProxies)
                {
                    shapeProxy.Dispose();
                }

                _bufferCache.Remove(renderObject.ModelResource);
            }
        }

        var textureCacheKey = (renderObject.ModelResource, renderObject.Textures);
        if (renderObject.ModelResource is not null && renderObject.Textures is not null &&
            renderObject.MaterialTextureInfos is not null &&
            _textureCache.TryGetValue(textureCacheKey, out var textureCacheEntry))
        {
            renderObject.Textures = null;
            renderObject.MaterialTextureInfos = null;
            if (--textureCacheEntry.UseCount == 0)
            {
                foreach (var matTexInfo in textureCacheEntry.MatTexInfos)
                {
                    matTexInfo?.Texture.Dispose();
                }

                _textureCache.Remove(textureCacheKey);
            }
        }
    }

    public void InitializeTexturePatternAnimationObject(G3dAnimationObject animationObject)
    {
        if (animationObject.AnimationResource is not G3dTexturePatternAnimation texPatAnm ||
            animationObject.TextureResource is null)
        {
            return;
        }

        var key = (texPatAnm, animationObject.TextureResource);
        if (!_animTextureCache.TryGetValue(key, out var animTextureCacheEntry))
        {
            animTextureCacheEntry = CreateAnimTextureCacheEntry(animationObject, texPatAnm);
            _animTextureCache.Add(key, animTextureCacheEntry);
        }

        animTextureCacheEntry.UseCount++;
        animationObject.FrameTextureInfos = animTextureCacheEntry.FrameTexInfos;
    }

    public void CleanupTexturePatternAnimationObject(G3dAnimationObject animationObject)
    {
        if (animationObject.AnimationResource is not G3dTexturePatternAnimation texturePatternAnimation ||
            animationObject.TextureResource is null || animationObject.FrameTextureInfos is null)
        {
            return;
        }

        var key = (texturePatternAnimation, animationObject.TextureResource);
        if (!_animTextureCache.TryGetValue(key, out var animTextureCacheEntry))
        {
            return;
        }

        animationObject.FrameTextureInfos = null;
        if (--animTextureCacheEntry.UseCount == 0)
        {
            foreach (var mat in animTextureCacheEntry.FrameTexInfos)
            {
                foreach (var frame in mat)
                {
                    frame.Texture.Dispose();
                }
            }

            _animTextureCache.Remove(key);
        }
    }

    public abstract DisplayListBuffer CreateDisplayListBuffer(ReadOnlySpan<byte> dl);
    public abstract Texture CreateTexture(Rgba8Bitmap bitmap);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        foreach (var entry in _bufferCache.Values)
        {
            foreach (var shapeProxy in entry.ShapeProxies)
            {
                shapeProxy?.Dispose();
            }
        }

        _bufferCache.Clear();

        foreach (var entry in _textureCache.Values)
        {
            foreach (var matTexInfo in entry.MatTexInfos)
            {
                matTexInfo?.Texture?.Dispose();
            }
        }

        _textureCache.Clear();

        foreach (var entry in _animTextureCache.Values)
        {
            foreach (var mat in entry.FrameTexInfos)
            {
                foreach (var frame in mat)
                {
                    frame?.Texture?.Dispose();
                }
            }
        }

        _animTextureCache.Clear();
    }

    private AnimTextureCacheEntry CreateAnimTextureCacheEntry(G3dAnimationObject animationObject,
        G3dTexturePatternAnimation texturePatternAnimation)
    {
        var frameTexInfos = new MaterialTextureInfo[texturePatternAnimation.Dictionary.Count][];
        for (int i = 0; i < texturePatternAnimation.Dictionary.Count; i++)
        {
            var entry = texturePatternAnimation.Dictionary[i].Data;
            frameTexInfos[i] = new MaterialTextureInfo[entry.FrameValues.Length];
            for (int j = 0; j < entry.FrameValues.Length; j++)
            {
                var frameValue = entry.FrameValues[j];
                try
                {
                    string textureName = texturePatternAnimation.TextureNames[frameValue.TextureIndex];
                    var texture = animationObject.TextureResource.TextureDictionary[textureName];

                    PaletteDictionaryData palette = null;
                    if (texture.TexImageParam.Format != ImageFormat.Direct && frameValue.PaletteIndex != 255)
                    {
                        string paletteName = texturePatternAnimation.PaletteNames[frameValue.PaletteIndex];
                        palette = animationObject.TextureResource.PaletteDictionary[paletteName];
                    }

                    frameTexInfos[i][j] = new MaterialTextureInfo
                    {
                        Texture = CreateTexture(animationObject.TextureResource.ToBitmap(texture, palette)),
                        TexImageParam = texture.TexImageParam
                    };
                }
                catch { }
            }
        }

        return new AnimTextureCacheEntry(frameTexInfos);
    }

    private void BindTextures(G3dModel model, G3dTextureSet textures)
    {
        var matTexInfos = new MaterialTextureInfo[model.Materials.Materials.Length];
        for (int j = 0; j < model.Materials.Materials.Length; j++)
        {
            var material = model.Materials.Materials[j];

            TextureDictionaryData textureDictionaryData = null;
            for (int k = 0; k < model.Materials.TextureToMaterialListDictionary.Count; k++)
            {
                if (!model.Materials.TextureToMaterialListDictionary[k].Data.Materials.Contains((byte)j))
                    continue;

                int texid = k;
                for (int l = 0; l < textures.TextureDictionary.Count; l++)
                {
                    if (textures.TextureDictionary[l].Name == model.Materials.TextureToMaterialListDictionary[k].Name)
                    {
                        texid = l;
                        break;
                    }
                }

                try
                {
                    textureDictionaryData = textures.TextureDictionary[texid].Data;
                }
                catch { }

                break;
            }

            if (textureDictionaryData == null)
                continue;

            PaletteDictionaryData paletteDictionaryData = null;
            if (textureDictionaryData.TexImageParam.Format != ImageFormat.Direct)
            {
                for (int k = 0; k < model.Materials.PaletteToMaterialListDictionary.Count; k++)
                {
                    if (!model.Materials.PaletteToMaterialListDictionary[k].Data.Materials.Contains((byte)j))
                        continue;

                    int palid = k;
                    for (int l = 0; l < textures.PaletteDictionary.Count; l++)
                    {
                        if (textures.PaletteDictionary[l].Name == model.Materials.PaletteToMaterialListDictionary[k].Name)
                        {
                            palid = l;
                            break;
                        }
                    }

                    paletteDictionaryData = textures.PaletteDictionary[palid].Data;
                    break;
                }
            }

            matTexInfos[j] = new MaterialTextureInfo
            {
                Texture = CreateTexture(textures.ToBitmap(textureDictionaryData, paletteDictionaryData)),
                TexImageParam = textureDictionaryData.TexImageParam,
                MagW = ((textureDictionaryData.ExtraParam & TextureDictionaryData.ParamExOrigWMask) >>
                    TextureDictionaryData.ParamExOrigWShift) / (double)material.OriginalWidth,
                MagH = ((textureDictionaryData.ExtraParam & TextureDictionaryData.ParamExOrigHMask) >>
                    TextureDictionaryData.ParamExOrigHShift) / (double)material.OriginalHeight
            };
        }

        _textureCache[(model, textures)] = new TextureCacheEntry(matTexInfos);
    }
}