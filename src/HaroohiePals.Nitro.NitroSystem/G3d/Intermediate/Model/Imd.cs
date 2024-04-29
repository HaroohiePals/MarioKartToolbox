using HaroohiePals.Graphics3d;
using HaroohiePals.IO;
using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class Imd
{
    public Imd(byte[] data)
        : this(new MemoryStream(data, false)) { }

    public Imd(Stream stream)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var doc = new XmlDocument();
        doc.Load(stream);

        var root = doc["imd"];
        Version = IntermediateUtil.GetStringAttribute(root, "version");

        Head = new Head(root["head"]);

        var body = root["body"];
        if (body == null)
            throw new Exception();

        ModelInfo = new ImdModelInfo(body["model_info"]);
        BoxTest = new ImdBoxTest(body["box_test"]);

        var texImageArray = body["tex_image_array"];
        if (texImageArray != null)
        {
            int size = IntermediateUtil.GetIntAttribute(texImageArray, "size");

            var elements = texImageArray.ChildNodes.OfType<XmlElement>().ToArray();
            if (elements.Length != size)
                throw new Exception();
            TexImageArray = new ImdTexImage[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                TexImageArray[i] = new ImdTexImage(elements[i]);

            Array.Sort(TexImageArray, (a, b) => a.Index.CompareTo(b.Index));
        }

        var texPaletteArray = body["tex_palette_array"];
        if (texPaletteArray != null)
        {
            int size = IntermediateUtil.GetIntAttribute(texPaletteArray, "size");

            var elements = texPaletteArray.ChildNodes.OfType<XmlElement>().ToArray();
            if (elements.Length != size)
                throw new Exception();
            TexPaletteArray = new ImdTexPalette[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                TexPaletteArray[i] = new ImdTexPalette(elements[i]);

            Array.Sort(TexPaletteArray, (a, b) => a.Index.CompareTo(b.Index));
        }

        var materialArray = body["material_array"];
        if (materialArray != null)
        {
            int size = IntermediateUtil.GetIntAttribute(materialArray, "size");

            var elements = materialArray.ChildNodes.OfType<XmlElement>().ToArray();
            if (elements.Length != size)
                throw new Exception();
            MaterialArray = new ImdMaterial[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                MaterialArray[i] = new ImdMaterial(elements[i]);

            Array.Sort(MaterialArray, (a, b) => a.Index.CompareTo(b.Index));
        }

        var envelope = body["envelope"];
        if (envelope != null)
            Envelope = new ImdEnvelope(envelope);

        var matrixArray = body["matrix_array"];
        if (matrixArray != null)
        {
            int size = IntermediateUtil.GetIntAttribute(matrixArray, "size");

            var elements = matrixArray.ChildNodes.OfType<XmlElement>().ToArray();
            if (elements.Length != size)
                throw new Exception();
            MatrixArray = new ImdMatrix[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                MatrixArray[i] = new ImdMatrix(elements[i]);

            Array.Sort(MatrixArray, (a, b) => a.Index.CompareTo(b.Index));
        }

        var polygonArray = body["polygon_array"];
        if (polygonArray != null)
        {
            int size = IntermediateUtil.GetIntAttribute(polygonArray, "size");

            var elements = polygonArray.ChildNodes.OfType<XmlElement>().ToArray();
            if (elements.Length != size)
                throw new Exception();
            PolygonArray = new ImdPolygon[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                PolygonArray[i] = new ImdPolygon(elements[i]);

            Array.Sort(PolygonArray, (a, b) => a.Index.CompareTo(b.Index));
        }

        var nodeArray = body["node_array"];
        if (nodeArray != null)
        {
            int size = IntermediateUtil.GetIntAttribute(nodeArray, "size");

            var elements = nodeArray.ChildNodes.OfType<XmlElement>().ToArray();
            if (elements.Length != size)
                throw new Exception();
            NodeArray = new ImdNode[elements.Length];
            for (int i = 0; i < elements.Length; i++)
                NodeArray[i] = new ImdNode(elements[i]);

            Array.Sort(NodeArray, (a, b) => a.Index.CompareTo(b.Index));
        }

        OutputInfo = new ImdOutputInfo(body["output_info"]);
    }

    public string Version;

    public Head Head;

    //OriginalCreate
    //OriginalGenerator
    public ImdModelInfo ModelInfo;

    public ImdBoxTest BoxTest;

    //VtxPosData
    //VtxColorData
    public ImdTexImage[] TexImageArray;
    public ImdTexPalette[] TexPaletteArray;
    public ImdMaterial[] MaterialArray;
    public ImdEnvelope Envelope;
    public ImdMatrix[] MatrixArray;
    public ImdPolygon[] PolygonArray;
    public ImdNode[] NodeArray;

    public ImdOutputInfo OutputInfo;
    //ExNns3dme

    public Nsbmd ToNsbmd(string modelName) => ToNsbmd(new[] { this }, new[] { modelName });

    public Nsbtx ToNsbtx(bool mergeSameData = true) => ToNsbtx(new[] { this }, mergeSameData);

    private static string LimitNameLength(string name)
    {
        if (name.Length <= 16)
            return name;

        return name[..16];
    }

    private G3dModelInfo ConvertModelInfo()
    {
        var info = new G3dModelInfo();
        info.PosScale = 1 << ModelInfo.PosScale;
        info.InversePosScale = 1.0 / info.PosScale;

        info.BoxPosScale = 1 << BoxTest.PosScale;
        info.BoxInversePosScale = 1.0 / info.BoxPosScale;
        info.BoxX = BoxTest.Xyz.X;
        info.BoxY = BoxTest.Xyz.Y;
        info.BoxZ = BoxTest.Xyz.Z;
        info.BoxW = BoxTest.Whd.X;
        info.BoxH = BoxTest.Whd.Y;
        info.BoxD = BoxTest.Whd.Z;

        info.NodeCount = (byte)ModelInfo.NodeSizeCompressed;
        info.MaterialCount = (byte)ModelInfo.MaterialSizeCompressed;
        info.ShapeCount = (byte)PolygonArray.Length;
        info.VertexCount = (ushort)OutputInfo.VertexSize;
        info.PolygonCount = (ushort)OutputInfo.PolygonSize;
        info.TriangleCount = (ushort)OutputInfo.TriangleSize;
        info.QuadCount = (ushort)OutputInfo.QuadSize;
        info.ScalingRule = ModelInfo.ScalingRule switch
        {
            ImdModelInfo.ModelScalingRule.Standard => 0,
            ImdModelInfo.ModelScalingRule.Maya => 1,
            ImdModelInfo.ModelScalingRule.Si3d => 2,
            _ => throw new Exception()
        };
        info.TextureMatrixMode = ModelInfo.TexMatrixMode switch
        {
            ImdModelInfo.ModelTexMatrixMode.Maya => 0,
            ImdModelInfo.ModelTexMatrixMode.Si3d => 1,
            ImdModelInfo.ModelTexMatrixMode._3dsmax => 2,
            ImdModelInfo.ModelTexMatrixMode.Xsi => 3,
            _ => throw new Exception()
        };

        return info;
    }

    private G3dNodeSet ConvertNodes()
    {
        var nodeSet = new G3dNodeSet
        {
            NodeDictionary = [],
            Data = new G3dNodeData[NodeArray.Length]
        };

        for (int i = 0; i < NodeArray.Length; i++)
        {
            nodeSet.NodeDictionary.Add(LimitNameLength(NodeArray[i].Name), new OffsetDictionaryData());
            nodeSet.Data[i] = NodeArray[i].ToNodeData();
        }

        return nodeSet;
    }

    private G3dMaterialSet ConvertMaterials()
    {
        var matSet = new G3dMaterialSet
        {
            MaterialDictionary = [],
            Materials = new G3dMaterial[MaterialArray.Length],
            TextureToMaterialListDictionary = [],
            PaletteToMaterialListDictionary = []
        };

        for (int i = 0; i < MaterialArray.Length; i++)
        {
            matSet.MaterialDictionary.Add(LimitNameLength(MaterialArray[i].Name), new OffsetDictionaryData());
            matSet.Materials[i] = MaterialArray[i].ToMaterial(this);
        }

        if (TexImageArray != null)
        {
            foreach (int i in TexImageArray.Select((t, i) => (t, i)).OrderBy(t => t.t.Name, StringComparer.Ordinal)
                         .Select(t => t.i))
            {
                var mats = new List<byte>();
                for (int j = 0; j < MaterialArray.Length; j++)
                {
                    if (MaterialArray[j].TexImageIdx == i)
                        mats.Add((byte)j);
                }

                if (mats.Count > 0)
                {
                    matSet.TextureToMaterialListDictionary.Add(LimitNameLength(TexImageArray[i].Name),
                        new TextureToMaterialDictionaryData
                        {
                            Materials = mats.ToArray(),
                            MaterialCount = (byte)mats.Count
                        });
                }
            }
        }

        if (TexPaletteArray != null)
        {
            foreach (int i in TexPaletteArray.Select((t, i) => (t, i))
                         .OrderBy(t => t.t.Name, StringComparer.Ordinal)
                         .Select(t => t.i))
            {
                var mats = new List<byte>();
                for (int j = 0; j < MaterialArray.Length; j++)
                {
                    if (MaterialArray[j].TexPaletteIdx == i)
                        mats.Add((byte)j);
                }

                if (mats.Count > 0)
                {
                    matSet.PaletteToMaterialListDictionary.Add(LimitNameLength(TexPaletteArray[i].Name),
                        new PaletteToMaterialDictionaryData
                        {
                            Materials = mats.ToArray(),
                            MaterialCount = (byte)mats.Count
                        });
                }
            }
        }

        return matSet;
    }

    private G3dShapeSet ConvertShapes(int[] mtxStackSlot)
    {
        var shpSet = new G3dShapeSet
        {
            ShapeDictionary = [],
            Shapes = new G3dShape[PolygonArray.Length]
        };
        for (int i = 0; i < PolygonArray.Length; i++)
        {
            shpSet.ShapeDictionary.Add(LimitNameLength(PolygonArray[i].Name), new OffsetDictionaryData());
            shpSet.Shapes[i] = PolygonArray[i].ToShape(this, mtxStackSlot);
        }

        return shpSet;
    }

    private class DummyRenderContext : RenderContext
    {
        public DummyRenderContext(GeometryEngineState geState) : base(geState) { }
        public override void ApplyTexParams(Texture texture) { }

        public override void RenderShp(G3dShape shp, DisplayListBuffer buffer) { }
    }

    private G3dEnvelopeMatrices ConvertEnvelopes(G3dModel model)
    {
        var geState = new GeometryEngineState();
        var context = new DummyRenderContext(geState);
        context.GlobalState.FlushP(geState);

        var renderObj = new G3dRenderObject(model);

        var nodeMtx = new Matrix4d[NodeArray.Length];
        var nodeDirMtx = new Matrix3d[NodeArray.Length];

        renderObj.CallbackFunction = renderContext =>
        {
            nodeMtx[renderContext.RenderState.CurrentNodeDescription] = renderContext.GeState.PositionMatrix;
            nodeDirMtx[renderContext.RenderState.CurrentNodeDescription] = renderContext.GeState.DirectionMatrix;
        };
        renderObj.CallbackCmd = SbcCommand.NodeDescription;
        renderObj.CallbackTiming = SbcCallbackTiming.TimingC;
        renderObj.Flag |= G3dRenderObjectFlag.SkipSbcDraw;
        context.Sbc.SbcFunctionTable[(int)SbcCommand.NodeMix] = (rs, opt) => { rs.SbcData += 3 + rs.SbcData[2] * 3; };
        context.Sbc.Draw(renderObj);

        var result = new G3dEnvelopeMatrices();
        result.Envelopes = new G3dEnvelope[NodeArray.Length];
        for (int i = 0; i < NodeArray.Length; i++)
        {
            result.Envelopes[i] = new G3dEnvelope();

            var invM = nodeMtx[i].Inverted();
            result.Envelopes[i].InversePositionMatrix = new Matrix4x3d(invM.Row0.Xyz, invM.Row1.Xyz, invM.Row2.Xyz, invM.Row3.Xyz);
            result.Envelopes[i].InverseDirectionMatrix = Matrix3d.Invert(nodeDirMtx[i]);
        }

        return result;
    }

    private G3dModel ToModel()
    {
        var model = new G3dModel();
        model.Info = ConvertModelInfo();
        model.Nodes = ConvertNodes();
        model.Materials = ConvertMaterials();

        (model.Sbc, int[] mtxStackSlot) = ImdSbcGenerator.GenerateSbc(this);

        model.Info.FirstUnusedMatrixStackId = (byte)(mtxStackSlot.Max() + 1);

        model.Shapes = ConvertShapes(mtxStackSlot);

        if (Envelope != null)
            model.EnvelopeMatrices = ConvertEnvelopes(model);

        return model;
    }

    private static void ConvertTextures(G3dTextureSet textures, ImdTexImage[] texImageArray, bool mergeSameData)
    {
        var texData = new List<byte>();
        var texPlttIdxData = new List<byte>();
        var texData4x4 = new List<byte>();

        var processedTexImages = new List<ImdTexImage>();
        var processedTexAddresses = new List<ushort>();

        ushort findTextureAddress(ImdTexImage texImage, out bool outputTexData)
        {
            outputTexData = true;

            ushort address = (ushort)((texImage.Format == Gx.ImageFormat.Comp4x4 ? texData4x4.Count : texData.Count) / 8);

            if (mergeSameData)
            {
                var texWithSameData = processedTexImages.FirstOrDefault(x => x.Bitmap.SequenceEqual(texImage.Bitmap));
                if (texWithSameData != null)
                {
                    outputTexData = false;
                    address = processedTexAddresses[processedTexImages.IndexOf(texWithSameData)];
                }
                processedTexAddresses.Add(address);
                processedTexImages.Add(texImage);
            }
            return address;
        }

        foreach (var texImage in texImageArray)
        {
            var data = new TextureDictionaryData();

            int width = GxTexImageParam.ToNitroSize(texImage.Width);
            int height = GxTexImageParam.ToNitroSize(texImage.Height);

            ushort address = findTextureAddress(texImage, out bool outputTexData);

            data.TexImageParam = new GxTexImageParam
            {
                Color0Transparent = texImage.Color0Mode == ImdTexImage.TexColor0Mode.Transparency,
                Format = texImage.Format,
                Width = width,
                Height = height,
                Address = address
            };

            bool whSame = texImage.OriginalHeight == texImage.Height && texImage.OriginalWidth == texImage.Width;

            data.ExtraParam = (uint)(((whSame ? 1 : 0) << TextureDictionaryData.ParamExWHSameShift) |
                ((texImage.OriginalHeight & 0x3FF) << TextureDictionaryData.ParamExOrigHShift) |
                ((texImage.OriginalWidth & 0x3FF) << TextureDictionaryData.ParamExOrigWShift));

            textures.TextureDictionary.Add(LimitNameLength(texImage.Name), data);

            if (outputTexData)
            {
                if (texImage.Format == Gx.ImageFormat.Comp4x4)
                {
                    texPlttIdxData.AddRange(texImage.Tex4x4PaletteIdx);
                    texData4x4.AddRange(texImage.Bitmap);
                }
                else
                {
                    texData.AddRange(texImage.Bitmap);
                }
            }
        }

        textures.TextureInfo.TextureData = texData.ToArray();
        textures.Texture4x4Info.TextureData = texData4x4.ToArray();
        textures.Texture4x4Info.TexturePaletteIndexData = texPlttIdxData.ToArray();
    }

    private static void ConvertPalettes(G3dTextureSet textures, ImdTexPalette[] texPaletteArray, bool mergeSameData)
    {
        using (var m = new MemoryStream())
        {
            var ew = new EndianBinaryWriter(m);

            var processedTexPalettes = new List<ImdTexPalette>();
            var processedPalAddresses = new List<ushort>();

            ushort findPaletteAddress(ImdTexPalette texPalette, out bool outputPalData)
            {
                outputPalData = true;

                ushort address = (ushort)(ew.BaseStream.Position / 8);

                if (mergeSameData)
                {
                    var palWithSameData = processedTexPalettes.FirstOrDefault(x => x.Colors.SequenceEqual(texPalette.Colors));
                    if (palWithSameData != null)
                    {
                        outputPalData = false;
                        address = processedPalAddresses[processedTexPalettes.IndexOf(palWithSameData)];
                    }
                    processedPalAddresses.Add(address);
                    processedTexPalettes.Add(texPalette);
                }
                return address;
            }

            foreach (var texPalette in texPaletteArray)
            {
                textures.PaletteDictionary.Add(LimitNameLength(texPalette.Name), new PaletteDictionaryData
                {
                    Offset = findPaletteAddress(texPalette, out bool outputPalData)
                });

                if (outputPalData)
                    ew.Write(texPalette.Colors.Select(x => x.Packed).ToArray());
            }

            textures.PaletteInfo.PaletteData = m.ToArray();
        }
    }

    private static G3dTextureSet ToTextures(ImdTexImage[] texImageArray, ImdTexPalette[] texPaletteArray, bool mergeSameData)
    {
        var textures = new G3dTextureSet();

        ConvertTextures(textures, texImageArray, mergeSameData);
        ConvertPalettes(textures, texPaletteArray, mergeSameData);

        return textures;
    }

    private static G3dTextureSet ToTextures(Imd[] models, bool mergeSameData)
    {
        var texImageArray = new List<ImdTexImage>();
        var texPaletteArray = new List<ImdTexPalette>();

        foreach (var model in models)
        {
            foreach (var texImage in model.TexImageArray)
            {
                if (texImageArray.FirstOrDefault(x => x.Name == texImage.Name) == null)
                {
                    texImageArray.Add(texImage);
                }
            }
            foreach (var texPalette in model.TexPaletteArray)
            {
                if (texPaletteArray.FirstOrDefault(x => x.Name == texPalette.Name) == null)
                {
                    texPaletteArray.Add(texPalette);
                }
            }
        }

        return ToTextures(texImageArray.ToArray(), texPaletteArray.ToArray(), mergeSameData);
    }

    public static Nsbmd ToNsbmd(Imd[] models, string[] names)
    {
        if (models == null)
            throw new ArgumentNullException(nameof(models));
        if (names == null)
            throw new ArgumentNullException(nameof(names));
        if (models.Length != names.Length)
            throw new ArgumentException("Number of models is not equal to the number of names.");
        if (names.Distinct().Count() != names.Length)
            throw new ArgumentException("Duplicate names");
        if (names.Any(n => n.Length > 16))
            throw new ArgumentException("Names can have a max length of 16");

        var nsbmd = new Nsbmd();
        nsbmd.ModelSet.Dictionary = [];
        nsbmd.ModelSet.Models = new G3dModel[models.Length];
        for (int i = 0; i < models.Length; i++)
        {
            nsbmd.ModelSet.Dictionary.Add(names[i], new OffsetDictionaryData());
            nsbmd.ModelSet.Models[i] = models[i].ToModel();
        }

        return nsbmd;
    }

    public static Nsbtx ToNsbtx(Imd[] models, bool mergeSameData = true)
    {
        var nsbtx = new Nsbtx();
        nsbtx.TextureSet = ToTextures(models, mergeSameData);
        return nsbtx;
    }
}