using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.Nitro.NitroSystem.G3d;

public delegate void SbcCallbackFunction(RenderContext context);

public sealed class Sbc
{
    public const int NoCmd = 0x1f;

    public const int CommandNum = 0x20;

    public const int SbcFlg000 = 0x00;
    public const int SbcFlg001 = 0x20;
    public const int SbcFlg010 = 0x40;
    public const int SbcFlg011 = 0x60;
    public const int SbcFlg100 = 0x80;
    public const int SbcFlg101 = 0xa0;
    public const int SbcFlg110 = 0xc0;
    public const int SbcFlg111 = 0xe0;

    public const int SbcCmdMask = 0x1f;
    public const int SbcFlgMask = 0xe0;

    public const int MtxStackSys  = 30;
    public const int MtxStackUser = 29;

    [Flags]
    public enum SbcNodeDescFlag
    {
        MayaSscApply  = 0x01,
        MayaSscParent = 0x02
    }

    public delegate void SbcFunction(G3dRenderState renderState, uint opt);

    public SbcFunction[] SbcFunctionTable;

    public delegate void SbcShapeFunction(G3dRenderState renderState, uint opt, G3dShape shp, uint idxShp);

    public SbcShapeFunction[] FuncSbcShpTable;

    public delegate void SbcMaterialFunction(G3dRenderState renderState, uint opt, G3dMaterial mat, uint idxMat);

    public SbcMaterialFunction[] FuncSbcMatTable;

    private readonly RenderContext _context;

    public Sbc(RenderContext context)
    {
        _context = context;
        SbcFunctionTable = new SbcFunction[CommandNum]
        {
            SbcNop,
            SbcRet,
            SbcNode,
            SbcMtx,
            SbcMat,
            SbcShp,
            SbcNodeDesc,
            SbcBB,
            SbcBBY,
            SbcNodeMix,
            SbcCallDl,
            SbcPosScale,
            SbcEnvMap,
            SbcPrjMap,
            null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null
        };
        FuncSbcShpTable =
        [
            SbcShpDefault,
            null, null, null
        ];
        FuncSbcMatTable =
        [
            SbcMatDefault,
            null, null, null
        ];
    }

    private void DrawInternal(G3dRenderState renderState, G3dRenderObject renderObj)
    {
        renderState.Clear();
        renderState.IsScaleCacheOne[0] = true;

        renderState.Flag = G3dRenderStateFlag.NodeVisible;

        renderState.SbcData = renderObj.UserSbc ?? renderObj.ModelResource.Sbc;

        renderState.RenderObject = renderObj;

        renderState.NodeResource  = renderObj.ModelResource.Nodes;
        renderState.MaterialResource       = renderObj.ModelResource.Materials;
        renderState.ShapeResource       = renderObj.ModelResource.Shapes;
        renderState.GetJointScale = _context.GetJointScaleFuncArray[renderObj.ModelResource.Info.ScalingRule];
        renderState.SendJointSrt   = _context.SendJointSrtFuncArray[renderObj.ModelResource.Info.ScalingRule];
        renderState.SendTexSrt   = _context.SendTexSrtFuncArray[renderObj.ModelResource.Info.TextureMatrixMode];
        renderState.PosScale     = renderObj.ModelResource.Info.PosScale;
        renderState.InversePosScale  = renderObj.ModelResource.Info.InversePosScale;

        if (renderObj.CallbackFunction != null && (int)renderObj.CallbackCmd < CommandNum)
            renderState.SetCallback(renderObj.CallbackFunction, renderObj.CallbackCmd, renderObj.CallbackTiming);

        if ((renderObj.Flag & G3dRenderObjectFlag.Record) != 0)
            renderState.Flag |= G3dRenderStateFlag.OptRecord;

        if ((renderObj.Flag & G3dRenderObjectFlag.NoGeCmd) != 0)
            renderState.Flag |= G3dRenderStateFlag.OptNoGeCmd;

        if ((renderObj.Flag & G3dRenderObjectFlag.SkipSbcDraw) != 0)
            renderState.Flag |= G3dRenderStateFlag.OptSkipSbcDraw;

        if ((renderObj.Flag & G3dRenderObjectFlag.SkipSbcMtxCalc) != 0)
            renderState.Flag |= G3dRenderStateFlag.OptSkipSbcMtxCalc;

        renderObj.CallbackInitFunction?.Invoke(_context);

        do
        {
            renderState.Flag &= ~G3dRenderStateFlag.Skip;
            SbcFunctionTable[renderState.SbcData[0] & SbcCmdMask](renderState, (uint)(renderState.SbcData[0] & SbcFlgMask));
        } while ((renderState.Flag & G3dRenderStateFlag.Return) == 0);

        renderObj.Flag &= ~G3dRenderObjectFlag.Record;
    }

    public void Draw(G3dRenderObject renderObj)
    {
        if (renderObj.TestFlag(G3dRenderObjectFlag.HintObsolete))
        {
            Array.Clear(renderObj.MaterialAnimationMayExist, 0, renderObj.MaterialAnimationMayExist.Length);
            Array.Clear(renderObj.JointAnimationMayExist, 0, renderObj.JointAnimationMayExist.Length);
            Array.Clear(renderObj.VisibilityAnimationMayExist, 0, renderObj.VisibilityAnimationMayExist.Length);

            if (renderObj.MaterialAnimations != null)
                G3dRenderObject.UpdateAnimationExistence(renderObj.MaterialAnimationMayExist, renderObj.MaterialAnimations);
            if (renderObj.JointAnimations != null)
                G3dRenderObject.UpdateAnimationExistence(renderObj.JointAnimationMayExist, renderObj.JointAnimations);
            if (renderObj.VisibilityAnimations != null)
                G3dRenderObject.UpdateAnimationExistence(renderObj.VisibilityAnimationMayExist, renderObj.VisibilityAnimations);

            renderObj.ResetFlag(G3dRenderObjectFlag.HintObsolete);
        }

        if (_context.RenderState != null)
        {
            DrawInternal(_context.RenderState, renderObj);
        }
        else
        {
            _context.RenderState = new G3dRenderState();
            DrawInternal(_context.RenderState, renderObj);
            _context.RenderState = null;
        }
    }

    private void SbcNop(G3dRenderState renderState, uint opt)
    {
        renderState.SbcData++;
    }

    private void SbcRet(G3dRenderState renderState, uint opt)
    {
        renderState.Flag |= G3dRenderStateFlag.Return;
    }

    private void SbcNode(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0)
        {
            uint curNode = renderState.CurrentNode = renderState.SbcData[1];
            renderState.Flag         |= G3dRenderStateFlag.CurrentNodeValid;
            renderState.VisibilityAnimation =  renderState.TmpVisAnmResult;

            if (!renderState.PerformCallbackA(_context, SbcCommand.Node))
            {
                if (renderState.RenderObject.VisibilityAnimations == null || !renderState.RenderObject.VisibilityAnimationMayExist[curNode] ||
                    !renderState.RenderObject.BlendVisibilityFunction(renderState.VisibilityAnimation, renderState.RenderObject.VisibilityAnimations, curNode))
                {
                    renderState.VisibilityAnimation.IsVisible = (renderState.SbcData[2] & 1) == 1;
                }
            }

            if (!renderState.PerformCallbackB(_context, SbcCommand.Node))
            {
                if (renderState.VisibilityAnimation.IsVisible)
                    renderState.Flag |= G3dRenderStateFlag.NodeVisible;
                else
                    renderState.Flag &= ~G3dRenderStateFlag.NodeVisible;
            }

            renderState.PerformCallbackC(_context, SbcCommand.Node);
        }

        renderState.SbcData += 3;
    }

    private void SbcMtx(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0 && (renderState.Flag & G3dRenderStateFlag.NodeVisible) != 0)
        {
            if (!renderState.PerformCallbackA(_context, SbcCommand.Matrix))
            {
                if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
                {
                    _context.GeState.RestoreMatrix(renderState.SbcData[1]);
                }
            }

            renderState.PerformCallbackC(_context, SbcCommand.Matrix);
        }

        renderState.SbcData += 2;
    }

    private readonly uint[] _materialColorMask = new uint[8]
    {
        0x00000000,
        0x00007fff,
        0x7fff0000,
        0x7fff7fff,
        0x00008000,
        0x0000ffff,
        0x7fff8000,
        0x7fffffff
    };

    private void SbcMatDefault(G3dRenderState renderState, uint opt, G3dMaterial mat, uint idxMat)
    {
        renderState.CurrentMaterial =  (byte)idxMat;
        renderState.Flag       |= G3dRenderStateFlag.CurrentMaterialValid;
        renderState.MaterialAnimation = renderState.TmpMatAnmResult;

        if (!renderState.PerformCallbackA(_context, SbcCommand.Material))
        {
            MaterialAnimationResult result;

            if (renderState.RenderObject.RecordedMaterialAnimations != null && (renderState.Flag & G3dRenderStateFlag.OptRecord) == 0)
                result = renderState.RenderObject.RecordedMaterialAnimations[idxMat];
            else
            {
                if ((opt == SbcFlg001 || opt == SbcFlg010) && renderState.IsMaterialCached[idxMat])
                {
                    if (renderState.RenderObject.RecordedMaterialAnimations != null)
                        result = renderState.RenderObject.RecordedMaterialAnimations[idxMat];
                    else
                        result = _context.GlobalRenderState.MaterialCache[idxMat];
                }
                else
                {
                    if (renderState.RenderObject.RecordedMaterialAnimations != null)
                    {
                        renderState.IsMaterialCached[idxMat] = true;
                        result = renderState.RenderObject.RecordedMaterialAnimations[idxMat];
                    }
                    else if (opt == SbcFlg010)
                    {
                        renderState.IsMaterialCached[idxMat] = true;
                        result = _context.GlobalRenderState.MaterialCache[idxMat];
                    }
                    else
                    {
                        result = renderState.TmpMatAnmResult;
                    }

                    result.Flag = 0;
                    if ((renderState.MaterialResource.Materials[idxMat].Flags &
                         G3dMaterialFlags.Wireframe) != 0)
                    {
                        result.Flag |= MaterialAnimationResultFlag.Wireframe;
                    }

                    uint diffAmbMask = _materialColorMask[((uint)mat.Flags >> 6) & 7];
                    result.PrmMatColor0 = (_context.GlobalState.MaterialColor0 & ~diffAmbMask) | (mat.DiffuseAmbient & diffAmbMask);

                    uint specEmiMask = _materialColorMask[((uint)mat.Flags >> 9) & 7];
                    result.PrmMatColor1 = (_context.GlobalState.MaterialColor1 & ~specEmiMask) | (mat.SpecularEmission & specEmiMask);

                    result.PrmPolygonAttr = (_context.GlobalState.PolygonAttr & ~mat.PolygonAttributeMask) |
                                            (mat.PolygonAttribute & mat.PolygonAttributeMask);

                    result.PrmTexImage = mat.TexImageParam;
                    result.PrmTexPltt  = mat.TexPlttBase;

                    result.TextureInfo = renderState.RenderObject.MaterialTextureInfos?[idxMat];
                    if (result.TextureInfo != null)
                        result.PrmTexImage |= result.TextureInfo.TexImageParam & 0xFFFF0000u;

                    if ((mat.Flags & G3dMaterialFlags.TexMtxUse) != 0)
                    {
                        if ((mat.Flags & G3dMaterialFlags.TexMtxScaleOne) == 0)
                        {
                            result.ScaleS = mat.ScaleS;
                            result.ScaleT = mat.ScaleT;
                        }
                        else
                            result.Flag |= MaterialAnimationResultFlag.TextureMatrixScaleOne;

                        if ((mat.Flags & G3dMaterialFlags.TexMtxRotZero) == 0)
                        {
                            result.RotationSin = mat.RotationSin;
                            result.RotationCos = mat.RotationCos;
                        }
                        else
                            result.Flag |= MaterialAnimationResultFlag.TextureMatrixRotationZero;

                        if ((mat.Flags & G3dMaterialFlags.TexMtxTransZero) == 0)
                        {
                            result.TranslationS = mat.TranslationS;
                            result.TranslationT = mat.TranslationT;
                        }
                        else
                            result.Flag |= MaterialAnimationResultFlag.TextureMatrixTranslationZero;

                        result.Flag |= MaterialAnimationResultFlag.TextureMatrixSet;
                    }

                    if (renderState.RenderObject.MaterialAnimations != null && renderState.RenderObject.MaterialAnimationMayExist[idxMat])
                    {
                        renderState.RenderObject.BlendMaterialFunction(result, renderState.RenderObject.MaterialAnimations, idxMat);
                    }

                    if ((result.Flag & (MaterialAnimationResultFlag.TextureMatrixSet |
                                        MaterialAnimationResultFlag.TextureMatrixMult)) != 0)
                    {
                        result.OriginalWidth  = mat.OriginalWidth;
                        result.OriginalHeight = mat.OriginalHeight;
                        if (renderState.RenderObject.MaterialTextureInfos?[idxMat] != null)
                        {
                            result.MagW = renderState.RenderObject.MaterialTextureInfos[idxMat].MagW;
                            result.MagH = renderState.RenderObject.MaterialTextureInfos[idxMat].MagH;
                        }
                        else
                        {
                            result.MagW = mat.MagW;
                            result.MagH = mat.MagH;
                        }
                    }
                }
            }

            renderState.MaterialAnimation = result;
        }

        if (!renderState.PerformCallbackB(_context, SbcCommand.Material))
        {
            var result = renderState.MaterialAnimation;

            if (result.PrmPolygonAttr.Alpha == 0)
                renderState.Flag |= G3dRenderStateFlag.MaterialTransparent;
            else
            {
                if ((result.Flag & MaterialAnimationResultFlag.Wireframe) != 0)
                    result.PrmPolygonAttr.Alpha = 0;

                renderState.Flag &= ~G3dRenderStateFlag.MaterialTransparent;

                if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
                {
                    _context.GeState.MaterialColor0 = result.PrmMatColor0;
                    _context.GeState.MaterialColor1 = result.PrmMatColor1;
                    _context.GeState.PolygonAttr    = result.PrmPolygonAttr;
                    _context.GeState.TexImageParam  = result.PrmTexImage;
                    // _context.GeState.TexPlttBase = result.PrmTexPltt;

                    _context.ApplyTexParams(result.TextureInfo?.Texture);

                    if ((result.Flag & (MaterialAnimationResultFlag.TextureMatrixSet |
                                        MaterialAnimationResultFlag.TextureMatrixMult)) != 0)
                    {
                        renderState.SendTexSrt(result, _context);
                    }
                }
            }
        }

        renderState.PerformCallbackC(_context, SbcCommand.Material);
    }

    private void SbcMat(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0)
        {
            uint idxMat = renderState.SbcData[1];
            if ((renderState.Flag & G3dRenderStateFlag.NodeVisible) != 0 ||
                !((renderState.Flag & G3dRenderStateFlag.CurrentMaterialValid) != 0 && idxMat == renderState.CurrentMaterial))
            {
                var mat = renderState.MaterialResource.Materials[idxMat];
                FuncSbcMatTable[mat.ItemTag](renderState, opt, mat, idxMat);
            }
        }

        renderState.SbcData += 2;
    }

    private void SbcShpDefault(G3dRenderState renderState, uint opt, G3dShape shp, uint shpIdx)
    {
        if (!renderState.PerformCallbackA(_context, SbcCommand.Shape) && (renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
        {
            _context.RenderShp(shp, renderState.RenderObject.ShapeProxies[shpIdx]);
        }

        renderState.PerformCallbackB(_context, SbcCommand.Shape);
        renderState.PerformCallbackC(_context, SbcCommand.Shape);
    }

    private void SbcShp(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0 &&
            (renderState.Flag & G3dRenderStateFlag.NodeVisible) != 0 &&
            (renderState.Flag & G3dRenderStateFlag.MaterialTransparent) == 0)
        {
            uint idxShp = renderState.SbcData[1];
            var  shp    = renderState.ShapeResource.Shapes[idxShp];
            FuncSbcShpTable[shp.ItemTag](renderState, opt, shp, idxShp);
        }

        renderState.SbcData += 2;
    }

    private void SbcNodeDesc(G3dRenderState renderState, uint opt)
    {
        uint cmdLen = 4;

        uint idxNode = renderState.SbcData[1];
        renderState.CurrentNodeDescription =  (byte)idxNode;
        renderState.Flag            |= G3dRenderStateFlag.CurrentNodeDescriptionValid;

        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcMtxCalc) != 0)
        {
            if (opt == SbcFlg010 || opt == SbcFlg011)
            {
                cmdLen++;
            }

            if (opt == SbcFlg001 || opt == SbcFlg011)
            {
                cmdLen++;
                if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
                {
                    _context.GeState.RestoreMatrix(renderState.SbcData[4]);
                }
            }

            renderState.SbcData += (int)cmdLen;
            return;
        }

        if (opt == SbcFlg010 || opt == SbcFlg011)
        {
            cmdLen++;
            if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
            {
                _context.GeState.RestoreMatrix(renderState.SbcData[opt == SbcFlg010 ? 4 : 5]);
            }
        }

        renderState.JointAnimation = renderState.TmpJntAnmResult;

        if (!renderState.PerformCallbackA(_context, SbcCommand.NodeDescription))
        {
            JointAnimationResult anmResult;
            bool         isUseRecordData;

            if (renderState.RenderObject.RecordedJointAnimations != null)
            {
                anmResult       = renderState.RenderObject.RecordedJointAnimations[idxNode];
                isUseRecordData = (renderState.Flag & G3dRenderStateFlag.OptRecord) == 0;
            }
            else
            {
                isUseRecordData = false;
                anmResult       = renderState.TmpJntAnmResult;
            }

            if (!isUseRecordData)
            {
                anmResult.Flag = 0;

                if (renderState.RenderObject.JointAnimations == null ||
                    !renderState.RenderObject.BlendJointFunction(_context, anmResult, renderState.RenderObject.JointAnimations, idxNode))
                {
                    var nodeData = renderState.NodeResource.Data[idxNode];
                    nodeData.GetTranslation(anmResult);
                    nodeData.GetRotation(anmResult);
                    renderState.GetJointScale(anmResult, nodeData, renderState.SbcData, _context);
                }
            }

            renderState.JointAnimation = anmResult;
        }

        if (!renderState.PerformCallbackB(_context, SbcCommand.NodeDescription) && (renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
        {
            renderState.SendJointSrt(renderState.JointAnimation, _context);
        }

        renderState.JointAnimation = null;

        bool callbackFlag = renderState.PerformCallbackC(_context, SbcCommand.NodeDescription);
        if (opt == SbcFlg001 || opt == SbcFlg011)
        {
            cmdLen++;

            if (!callbackFlag && (renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
            {
                _context.GeState.StoreMatrix(renderState.SbcData[4]);
            }
        }

        renderState.SbcData += (int)cmdLen;
    }

    private void SbcBB(G3dRenderState renderState, uint opt)
    {
        uint cmdLen = 2;

        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) != 0)
        {
            if (opt == SbcFlg010 || opt == SbcFlg011)
            {
                cmdLen++;
            }

            if (opt == SbcFlg001 || opt == SbcFlg011)
            {
                cmdLen++;
            }

            renderState.SbcData += (int)cmdLen;
            return;
        }

        if (opt == SbcFlg010 || opt == SbcFlg011)
        {
            cmdLen++;

            if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
            {
                _context.GeState.RestoreMatrix(renderState.SbcData[opt == SbcFlg010 ? 2 : 3]);
            }
        }

        bool callbackFlag = renderState.PerformCallbackA(_context, SbcCommand.Billboard);

        if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0 && !callbackFlag)
        {
            var m = _context.GeState.PositionMatrix;

            var scale = m.ExtractScale();

            _context.GeState.LoadMatrix(Matrix4x3d.CreateTranslation(m.ExtractTranslation()));
            _context.GeState.Scale(scale);
        }

        callbackFlag = renderState.PerformCallbackC(_context, SbcCommand.Billboard);
        if (opt == SbcFlg001 || opt == SbcFlg011)
        {
            cmdLen++;

            if (!callbackFlag && (renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
            {
                _context.GeState.StoreMatrix(renderState.SbcData[2]);
            }
        }

        renderState.SbcData += (int)cmdLen;
    }

    private void SbcBBY(G3dRenderState renderState, uint opt)
    {
        int cmdLen = 2;
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) != 0)
        {
            if (opt == SbcFlg010 || opt == SbcFlg011)
            {
                cmdLen++;
            }

            if (opt == SbcFlg001 || opt == SbcFlg011)
            {
                cmdLen++;
            }

            renderState.SbcData += cmdLen;
            return;
        }

        if (opt == SbcFlg010 || opt == SbcFlg011)
        {
            cmdLen++;
            if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
            {
                _context.GeState.RestoreMatrix(renderState.SbcData[opt == SbcFlg010 ? 2 : 3]);
            }
        }

        bool callbackFlag = renderState.PerformCallbackA(_context, SbcCommand.BillboardY);
        if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0 && !callbackFlag)
        {
            var m = _context.GeState.PositionMatrix;
            var mtx = Matrix4x3d.CreateScale(1);
            mtx.Row3 = m.ExtractTranslation();
            var scale = m.ExtractScale();

            if (m[1, 1] != 0 || m[1, 2] != 0)
            {
                mtx.Row1 = m.Row1.Xyz.Normalized();

                mtx[2, 1] = -mtx[1, 2];
                mtx[2, 2] = mtx[1, 1];
            }
            else
            {
                mtx.Row2 = m.Row2.Xyz.Normalized();

                mtx[1, 2] = -mtx[2, 1];
                mtx[1, 1] = mtx[2, 2];
            }

            _context.GeState.LoadMatrix(mtx);
            _context.GeState.Scale(scale);
        }

        callbackFlag = renderState.PerformCallbackC(_context, SbcCommand.BillboardY);

        if (opt == SbcFlg001 || opt == SbcFlg011)
        {
            cmdLen++;

            if (!callbackFlag && (renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0)
            {
                _context.GeState.StoreMatrix(renderState.SbcData[2]);
            }
        }

        renderState.SbcData += cmdLen;
    }

    private void SbcNodeMix(G3dRenderState renderState, uint opt)
    {
        double w      = 0;
        var    evpMtx = renderState.RenderObject.ModelResource.EnvelopeMatrices;
        uint   numMtx = renderState.SbcData[2];
        int    p      = 3;

        var sumM = new Matrix4x3d();
        var sumN = new Matrix3d();

        G3dGlobalRenderState.EnvelopeCacheEntry y = null;

        for (uint i = 0; i < numMtx; i++)
        {
            uint idxJnt    = renderState.SbcData[p + 1];
            bool evpCached = renderState.IsEnvelopeCached[idxJnt];

            var x = _context.GlobalRenderState.EnvelopeCache[idxJnt];
            if (!evpCached)
            {
                renderState.IsEnvelopeCached[idxJnt] = true;

                _context.GeState.RestoreMatrix(renderState.SbcData[p]);
                _context.GeState.MatrixMode = GxMtxMode.Position;
                _context.GeState.MultMatrix(evpMtx.Envelopes[idxJnt].InversePositionMatrix);
            }

            if (i != 0)
            {
                sumN.Row0 += w * y.DirectionMtx.Row0;
                sumN.Row1 += w * y.DirectionMtx.Row1;
                sumN.Row2 += w * y.DirectionMtx.Row2;
            }

            if (!evpCached)
            {
                x.PositionMtx = _context.GeState.PositionMatrix;
                _context.GeState.MatrixMode = GxMtxMode.PositionVector;
                _context.GeState.MultMatrix(evpMtx.Envelopes[idxJnt].InverseDirectionMatrix);
            }

            w = renderState.SbcData[p + 2] / 256.0;

            sumM.Row0 += w * x.PositionMtx.Row0.Xyz;
            sumM.Row1 += w * x.PositionMtx.Row1.Xyz;
            sumM.Row2 += w * x.PositionMtx.Row2.Xyz;
            sumM.Row3 += w * x.PositionMtx.Row3.Xyz;

            p += 3;
            y =  _context.GlobalRenderState.EnvelopeCache[idxJnt];

            if (!evpCached)
                y.DirectionMtx = _context.GeState.DirectionMatrix;
        }

        sumN.Row0 += w * y.DirectionMtx.Row0;
        sumN.Row1 += w * y.DirectionMtx.Row1;
        sumN.Row2 += w * y.DirectionMtx.Row2;

        _context.GeState.LoadMatrix(new Matrix4d(sumN));
        _context.GeState.MatrixMode = GxMtxMode.Position;
        _context.GeState.LoadMatrix(sumM);
        _context.GeState.MatrixMode = GxMtxMode.PositionVector;

        _context.GeState.StoreMatrix(renderState.SbcData[1]);
        renderState.SbcData += 3 + renderState.SbcData[2] * 3;
    }

    private void SbcCallDl(G3dRenderState renderState, uint opt)
    {
        bool cbFlag = renderState.PerformCallbackA(_context, SbcCommand.CallDisplayList);

        if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0 && !cbFlag)
        {
            uint relAddr = IOUtil.ReadU32Le(renderState.SbcData + 1);
            uint size    = IOUtil.ReadU32Le(renderState.SbcData + 5);

            byte[] dl = new byte[size];
            Array.Copy(renderState.SbcData.Array, renderState.SbcData.Index + relAddr, dl, 0, size);

            //todo: not supported anymore, probably won't fix because it's never used anyway
            // _context.CommandContext.RunDL(dl);
            throw new NotImplementedException("CallDl command not supported!");
        }

        renderState.PerformCallbackC(_context, SbcCommand.CallDisplayList);

        renderState.SbcData += 9;
    }

    private void SbcPosScale(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptNoGeCmd) == 0 && (renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0)
        {
            _context.GeState.Scale(new Vector3d(opt == SbcFlg000 ? renderState.PosScale : renderState.InversePosScale));
        }

        renderState.SbcData += 1;
    }

    private void SbcEnvMap(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0 && (renderState.Flag & G3dRenderStateFlag.NodeVisible) != 0)
        {
            if (renderState.MaterialAnimation.PrmTexImage.TexGen != GxTexGen.Normal)
            {
                renderState.MaterialAnimation.PrmTexImage.TexGen = GxTexGen.Normal;
                _context.GeState.TexImageParam     = renderState.MaterialAnimation.PrmTexImage;
            }

            _context.GeState.MatrixMode = GxMtxMode.Texture;

            if (!renderState.PerformCallbackA(_context, SbcCommand.EnvironmentMap))
            {
                int width  = renderState.MaterialAnimation.OriginalWidth;
                int height = renderState.MaterialAnimation.OriginalHeight;
                _context.GeState.Scale((width / 2.0, -height / 2.0, 1));
                _context.GeState.TexCoord = (width / 2.0, height / 2.0);
            }

            if (!renderState.PerformCallbackB(_context, SbcCommand.EnvironmentMap))
            {
                var mat = renderState.MaterialResource.Materials[renderState.SbcData[1]];
                if ((mat.Flags & G3dMaterialFlags.EffectMtx) != 0)
                {
                    _context.GeState.MultMatrix(mat.EffectMtx);
                }
            }

            if (!renderState.PerformCallbackC(_context, SbcCommand.EnvironmentMap))
            {
                _context.GeState.MultMatrix(_context.GeState.DirectionMatrix);
            }

            _context.GeState.MatrixMode = GxMtxMode.PositionVector;
        }

        renderState.SbcData += 3;
    }

    private void SbcPrjMap(G3dRenderState renderState, uint opt)
    {
        if ((renderState.Flag & G3dRenderStateFlag.OptSkipSbcDraw) == 0 && (renderState.Flag & G3dRenderStateFlag.NodeVisible) != 0)
        {
            var m = new Matrix4x3d(
                _context.GeState.PositionMatrix.Row0.Xyz,
                _context.GeState.PositionMatrix.Row1.Xyz,
                _context.GeState.PositionMatrix.Row2.Xyz,
                _context.GeState.PositionMatrix.Row3.Xyz);
            _context.GeState.StoreMatrix(MtxStackSys);

            if (renderState.MaterialAnimation.PrmTexImage.TexGen != GxTexGen.Vertex)
            {
                renderState.MaterialAnimation.PrmTexImage.TexGen = GxTexGen.Vertex;
                _context.GeState.TexImageParam     = renderState.MaterialAnimation.PrmTexImage;
            }

            if (!renderState.PerformCallbackA(_context, SbcCommand.ProjectionMap))
            {
                int width  = renderState.MaterialAnimation.OriginalWidth;
                int height = renderState.MaterialAnimation.OriginalHeight;
                _context.GeState.LoadMatrix(new Matrix4d(
                    width << 3, 0, 0, 0,
                    0, -height << 3, 0, 0,
                    0, 0, 1 << 4, 0,
                    width << 3, height << 3, 0, 1 << 4
                ));
            }

            if (!renderState.PerformCallbackB(_context, SbcCommand.ProjectionMap))
            {
                var mat = renderState.MaterialResource.Materials[renderState.SbcData[1]];

                if ((mat.Flags & G3dMaterialFlags.EffectMtx) != 0)
                    _context.GeState.LoadMatrix(mat.EffectMtx);
            }

            if (!renderState.PerformCallbackC(_context, SbcCommand.ProjectionMap))
            {
                _context.GeState.MultMatrix(_context.GlobalState.GetInvertedCameraMatrix());
                _context.GeState.MultMatrix(m);

                var texMtx = _context.GeState.PositionMatrix;
                _context.GeState.MatrixMode = GxMtxMode.Texture;

                _context.GeState.LoadMatrix(texMtx);
                _context.GeState.TexCoord = (texMtx[3, 0] / 16f, texMtx[3, 1] / 16f);
            }

            _context.GeState.MatrixMode = GxMtxMode.PositionVector;
            _context.GeState.RestoreMatrix(MtxStackSys);
        }

        renderState.SbcData += 3;
    }
}