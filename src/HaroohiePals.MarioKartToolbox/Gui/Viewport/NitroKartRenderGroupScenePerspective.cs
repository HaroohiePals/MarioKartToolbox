using HaroohiePals.Gui.Viewport;
using HaroohiePals.MarioKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using System;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.Gui.Viewport;

class NitroKartRenderGroupScenePerspective : NitroRenderGroupScenePerspective
{
    public MkdsStageInfo StageInfo { get; set; }

    public override void RenderPostProcessing(ViewportContext context)
    {
        float mkdsFov = (0x157C + 5) * MathF.PI / 32768;
        float mkdsNear = 0.25f;
        float mkdsFar = StageInfo.FrustumFar < 1.0 ? 1600f : (float)StageInfo.FrustumFar;
        float mkdsFrustTop = mkdsNear / MathF.Tan(mkdsFov);
        float mkdsFrustBot = -mkdsFrustTop;
        float mkdsFrustLeft = -mkdsFrustTop / (context.ViewportSize.X / (float)context.ViewportSize.Y);
        float mkdsFrustRight = -mkdsFrustLeft;
        NitroFog.FogProjectionMatrix = OpenTK.Mathematics.Matrix4.CreatePerspectiveOffCenter(mkdsFrustLeft,
            mkdsFrustRight,
            mkdsFrustBot,
            mkdsFrustTop, mkdsNear, mkdsFar);

        UpdateFogTable();

        NitroFog.FogColor = StageInfo.FogColor;
        NitroFog.FogShift = StageInfo.FogShift;
        NitroFog.FogOffset = StageInfo.FogOffset / 512;
        NitroFog.FogEnabled = StageInfo.FogEnabled;

        base.RenderPostProcessing(context);
    }

    private void UpdateFogTable()
    {
        switch (StageInfo.FogTableGenMode)
        {
            case 0:
                for (int i = 0; i < 32; i++)
                    NitroFog.FogTable[i] = i * 4;
                break;
            case 1:
                for (int i = 0; i < 32; i++)
                {
                    NitroFog.FogTable[i] = (i + 1) * (i + 1) / 8;
                    if (NitroFog.FogTable[i] > 127)
                        NitroFog.FogTable[i] = 127;
                }

                NitroFog.FogTable[0] = 0;
                break;
            case 2:
                for (int i = 0; i < 32; i++)
                {
                    int inv = 32 - (i + 1);
                    NitroFog.FogTable[i] = 128 - inv * inv / 8;
                    if (NitroFog.FogTable[i] > 127)
                        NitroFog.FogTable[i] = 127;
                }

                NitroFog.FogTable[0] = 0;
                break;
        }
    }

    public override void FrameSelection(ViewportContext context)
    {
        if (context.SceneObjectHolder.SelectionSize == 0)
            return;

        // todo: multiple selection with camera frustum and box stuff

        var first = context.SceneObjectHolder.GetSelection().FirstOrDefault();

        if (first is IPoint && RenderGroups.TryGetObjectTransform(first, -1, out var transform))
        {
            float distance = 100f;
            float angle = OpenTK.Mathematics.MathHelper.DegreesToRadians(30);

            float cos = MathF.Cos(angle);
            float sin = MathF.Sin(angle);

            var target = (OpenTK.Mathematics.Vector3)transform.Translation;

            var matrix = OpenTK.Mathematics.Matrix4.LookAt(
                (target.X, target.Y + sin * distance, target.Z + cos * distance), target,
                OpenTK.Mathematics.Vector3.UnitY);

            context.ViewMatrix = matrix;
        }
    }
}