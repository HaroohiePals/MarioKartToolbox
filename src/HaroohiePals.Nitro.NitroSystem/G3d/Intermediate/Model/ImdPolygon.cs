using HaroohiePals.Nitro.G3;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;
using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdPolygon
{
    public ImdPolygon(XmlElement element)
    {
        Index        = IntermediateUtil.GetIntAttribute(element, "index");
        Name         = IntermediateUtil.GetStringAttribute(element, "name");
        VertexSize   = IntermediateUtil.GetIntAttribute(element, "vertex_size");
        PolygonSize  = IntermediateUtil.GetIntAttribute(element, "polygon_size");
        TriangleSize = IntermediateUtil.GetIntAttribute(element, "triangle_size");
        QuadSize     = IntermediateUtil.GetIntAttribute(element, "quad_size");
        VolumeMin    = IntermediateUtil.GetVec3Attribute(element, "volume_min");
        VolumeMax    = IntermediateUtil.GetVec3Attribute(element, "volume_max");
        VolumeR      = IntermediateUtil.GetDoubleAttribute(element, "volume_r");
        MtxPrimSize  = IntermediateUtil.GetIntAttribute(element, "mtx_prim_size");
        NrmFlag      = IntermediateUtil.GetOnOffAttribute(element, "nrm_flag");
        ClrFlag      = IntermediateUtil.GetOnOffAttribute(element, "clr_flag");
        TexFlag      = IntermediateUtil.GetOnOffAttribute(element, "tex_flag");

        var xmlMtxPrimElements = element.ChildNodes.OfType<XmlElement>().ToArray();
        if (xmlMtxPrimElements.Length != MtxPrimSize)
            throw new Exception();
        MtxPrims = new MtxPrim[xmlMtxPrimElements.Length];
        for (int i = 0; i < xmlMtxPrimElements.Length; i++)
            MtxPrims[i] = new MtxPrim(xmlMtxPrimElements[i]);

        Array.Sort(MtxPrims, (a, b) => a.Index.CompareTo(b.Index));
    }

    public int      Index;
    public string   Name;
    public int      VertexSize;
    public int      PolygonSize;
    public int      TriangleSize;
    public int      QuadSize;
    public Vector3d VolumeMin;
    public Vector3d VolumeMax;
    public double   VolumeR;
    public int      MtxPrimSize;
    public bool     NrmFlag;
    public bool     ClrFlag;
    public bool     TexFlag;

    public MtxPrim[] MtxPrims;

    public class MtxPrim
    {
        public MtxPrim(XmlElement element)
        {
            Index = IntermediateUtil.GetIntAttribute(element, "index");

            var mtxList = element["mtx_list"];
            if (mtxList != null)
            {
                int size = IntermediateUtil.GetIntAttribute(mtxList, "size");

                if (size <= 0 || size > 31)
                    throw new Exception();

                MtxList = IntermediateUtil.GetInnerTextParts(mtxList).Select(int.Parse).ToArray();

                if (MtxList.Length != size)
                    throw new Exception();
            }

            var primitiveArray = element["primitive_array"];
            if (primitiveArray != null)
            {
                int size = IntermediateUtil.GetIntAttribute(primitiveArray, "size");

                var xmlPrimitiveElements = primitiveArray.ChildNodes.OfType<XmlElement>().ToArray();
                if (xmlPrimitiveElements.Length != size)
                    throw new Exception();
                PrimitiveArray = new ImdPrimitive[xmlPrimitiveElements.Length];
                for (int i = 0; i < xmlPrimitiveElements.Length; i++)
                    PrimitiveArray[i] = new ImdPrimitive(xmlPrimitiveElements[i]);

                Array.Sort(PrimitiveArray, (a, b) => a.Index.CompareTo(b.Index));
            }
        }

        public int         Index;
        public int[]       MtxList;
        public ImdPrimitive[] PrimitiveArray;
    }

    public G3dShape ToShape(Imd imd, int[] mtxStackSlot)
    {
        if (MtxPrims.Length != 1)
            throw new Exception();

        var shp = new G3dShape();
        if (NrmFlag)
            shp.Flags |= G3dShapeFlags.UseNormal;
        if (ClrFlag)
            shp.Flags |= G3dShapeFlags.UseColor;
        if (TexFlag)
            shp.Flags |= G3dShapeFlags.UseTexCoord;
        if (MtxPrims[0].MtxList.Length > 1)
            shp.Flags |= G3dShapeFlags.UseRestoreMtx;

        var dlBuilder = new DisplayListBuilder();

        bool firstMtx = true;
        foreach (var prim in MtxPrims[0].PrimitiveArray)
        {
            dlBuilder.Begin(prim.Type);
            foreach (var element in prim.Elements)
            {
                if (element is ImdPrimitiveElement.Mtx mtx)
                {
                    if (firstMtx)
                    {
                        // don't emit the first matrix restore, this one should already be active
                        firstMtx = false;
                        continue;
                    }

                    // write the right id
                    dlBuilder.RestoreMatrix((uint)mtxStackSlot[MtxPrims[0].MtxList[mtx.Idx]]);
                    if (imd.ModelInfo.PosScale != 0)
                        dlBuilder.Scale(new(1 << imd.ModelInfo.PosScale));
                }
                else
                    element.WriteCommand(dlBuilder);
            }

            dlBuilder.End();
        }

        shp.DisplayList = dlBuilder.ToArray();

        return shp;
    }
}