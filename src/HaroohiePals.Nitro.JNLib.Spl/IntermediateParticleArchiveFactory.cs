#nullable enable
using HaroohiePals.Graphics;
using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.Gx;

namespace HaroohiePals.Nitro.JNLib.Spl;

public class IntermediateParticleArchiveFactory
{
    private const string INTERMEDIATE_EMITTER_EXTENSION = ".ispl";
    private const string INTERMEDIATE_ARCHIVE_EXTENSION = ".ispa";
    private const string DEFAULT_ARCHIVE_NAME = $"archive{INTERMEDIATE_ARCHIVE_EXTENSION}";

    public async Task<IntermediateParticleArchive> CreateAsync(SplArchive splArchive, string outputDir,
        string fileName = DEFAULT_ARCHIVE_NAME, bool dumpNoRef = false)
    {
        var arc = new IntermediateParticleArchive();

        for (int i = 0; i < splArchive.Textures.Length; i++)
        {
            var ntga = SplTextureToNitroTga(splArchive, i);
            await File.WriteAllBytesAsync(Path.Combine(outputDir, $"tex{i:D3}.tga"), ntga.Write());
            if (dumpNoRef && splArchive.Textures[i].Params.RefTexData && splArchive.Textures[i].TexData?.Length > 0)
            {
                var ntga2 = SplTextureToNitroTga(splArchive, i, true);
                await File.WriteAllBytesAsync(Path.Combine(outputDir, $"tex{i:D3}_noref.tga"), ntga2.Write());
            }

            var texture = new IntermediateParticleArchive.ParticleTexture
            {
                Name = $"tex{i:D3}",
                Path = $"tex{i:D3}.tga"
            };

            if ((splArchive.Textures[i].Params.Repeat & 1) == 0)
                texture.S = IntermediateParticleArchive.ParticleTexture.TexMode.Clamp;
            else if ((splArchive.Textures[i].Params.Flip & 1) == 0)
                texture.S = IntermediateParticleArchive.ParticleTexture.TexMode.Repeat;
            else
                texture.S = IntermediateParticleArchive.ParticleTexture.TexMode.Mirror;

            if ((splArchive.Textures[i].Params.Repeat & 2) == 0)
                texture.T = IntermediateParticleArchive.ParticleTexture.TexMode.Clamp;
            else if ((splArchive.Textures[i].Params.Flip & 2) == 0)
                texture.T = IntermediateParticleArchive.ParticleTexture.TexMode.Repeat;
            else
                texture.T = IntermediateParticleArchive.ParticleTexture.TexMode.Mirror;

            arc.Textures.Add(texture);
        }

        for (int i = 0; i < splArchive.Emitters.Length; i++)
        {
            var emitter = splArchive.Emitters[i];

            emitter.Name = $"emitter{i:D3}";
            emitter.TextureName = $"tex{emitter.TextureId:D3}";

            if (emitter.Child is not null)
            {
                emitter.Child.TextureName = $"tex{emitter.Child.TextureId:D3}";
            }

            if (emitter.TexAnim is not null)
            {
                emitter.TexAnim.FrameTextureNames = [];
                foreach (byte t in emitter.TexAnim.Frames)
                    emitter.TexAnim.FrameTextureNames.Add($"tex{t:D3}");
            }

            string emitterFileName = $"{emitter.Name}{INTERMEDIATE_EMITTER_EXTENSION}";

            await File.WriteAllBytesAsync(Path.Combine(outputDir, emitterFileName), emitter.ToXml());
            arc.Emitters.Add(emitterFileName);
        }

        await File.WriteAllBytesAsync(Path.Combine(outputDir, fileName), arc.ToXml());

        return arc;
    }

    private static NitroTga SplTextureToNitroTga(SplArchive spa, int texId, bool noRef = false)
    {
        var tex = spa.Textures[texId];
        if (!tex.Params.RefTexData || noRef)
            return DecodeTexture(tex);

        var refTex = spa.Textures[tex.Params.RefTexId];
        return DecodeTexture(tex, refTex);
    }

    private static NitroTga DecodeTexture(SplArchive.Texture tex, SplArchive.Texture? refTex = null)
    {
        var ntga = new NitroTga();

        int width = 8 << tex.Params.Width;
        int height = 8 << tex.Params.Height;

        ntga.NitroData.SetNitroFormat(tex.Params.Format);
        ntga.NitroData.Color0Transparent = tex.Params.Pltt0Transparent;

        Rgba8Bitmap bitmap;

        tex = refTex ?? tex;

        ntga.NitroData.TexelData = tex.TexData;
        if (tex.Params.Format == ImageFormat.Comp4x4 && tex.PlttIdxData is not null)
        {
            ntga.NitroData.PlttIdxData = tex.PlttIdxData;
            ntga.NitroData.Palette = new byte[tex.PlttData.Length * 2];

            for (int i = 0; i < tex.PlttData.Length; i++)
            {
                IOUtil.WriteU16Le(ntga.NitroData.Palette, i * 2, tex.PlttData[i]);
            }

            bitmap = GxUtil.DecodeBmp(tex.TexData, ImageFormat.Comp4x4, width, height, tex.PlttData,
                tex.Params.Pltt0Transparent, tex.PlttIdxData);
        }
        else
        {
            bool color0Transparent = false;

            if (tex.Params.Format != ImageFormat.Direct)
            {
                ntga.NitroData.Palette = new byte[tex.PlttData.Length * 2];
                for (int i = 0; i < tex.PlttData.Length; i++)
                    IOUtil.WriteU16Le(ntga.NitroData.Palette, i * 2, tex.PlttData[i]);

                color0Transparent = tex.Params.Format != ImageFormat.A3I5 &&
                                    tex.Params.Format != ImageFormat.A5I3 &&
                                    ntga.NitroData.Color0Transparent;
            }

            bitmap = GxUtil.DecodeBmp(tex.TexData, tex.Params.Format, width, height, tex.PlttData, color0Transparent);
        }

        ntga.Header.ImageWidth = (ushort)width;
        ntga.Header.ImageHeight = (ushort)height;
        ntga.ImageData = new byte[width * height * 4];

        for (int i = 0; i < bitmap.Pixels.Length; i++)
        {
            IOUtil.WriteU32Le(ntga.ImageData, i * 4, bitmap.Pixels[i]);
        }

        return ntga;
    }
}