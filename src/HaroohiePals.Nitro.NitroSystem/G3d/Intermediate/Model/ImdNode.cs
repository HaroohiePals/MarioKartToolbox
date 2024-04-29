using HaroohiePals.Nitro.Fx;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;
using System;
using System.Linq;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Model;

public class ImdNode
{
    public enum NodeKind
    {
        Null,
        Mesh,
        Joint,
        Chain,
        Effector
    }

    public enum BillboardType
    {
        Off,
        On,
        YOn
    }

    public ImdNode(XmlElement element)
    {
        Index = IntermediateUtil.GetIntAttribute(element, "index");
        Name  = IntermediateUtil.GetStringAttribute(element, "name");
        Kind = element.GetAttribute("kind") switch
        {
            "null"     => NodeKind.Null,
            "mesh"     => NodeKind.Mesh,
            "joint"    => NodeKind.Joint,
            "chain"    => NodeKind.Chain,
            "effector" => NodeKind.Effector,
            _          => throw new Exception()
        };
        Parent          = IntermediateUtil.GetIntAttribute(element, "parent");
        Child           = IntermediateUtil.GetIntAttribute(element, "child");
        BrotherNext     = IntermediateUtil.GetIntAttribute(element, "brother_next");
        BrotherPrev     = IntermediateUtil.GetIntAttribute(element, "brother_prev");
        DrawMtx         = IntermediateUtil.GetOnOffAttribute(element, "draw_mtx");
        ScaleCompensate = IntermediateUtil.GetOnOffAttribute(element, "scale_compensate");
        Billboard = element.GetAttribute("billboard") switch
        {
            "off"  => BillboardType.Off,
            "on"   => BillboardType.On,
            "y_on" => BillboardType.YOn,
            _      => throw new Exception()
        };
        Scale        = IntermediateUtil.GetVec3Attribute(element, "scale");
        Rotate       = IntermediateUtil.GetVec3Attribute(element, "rotate");
        Translate    = IntermediateUtil.GetVec3Attribute(element, "translate");
        Visibility   = IntermediateUtil.GetOnOffAttribute(element, "visibility");
        DisplaySize  = IntermediateUtil.GetIntAttribute(element, "display_size");
        VertexSize   = IntermediateUtil.GetIntAttribute(element, "vertex_size");
        PolygonSize  = IntermediateUtil.GetIntAttribute(element, "polygon_size");
        TriangleSize = IntermediateUtil.GetIntAttribute(element, "triangle_size");
        QuadSize     = IntermediateUtil.GetIntAttribute(element, "quad_size");
        VolumeMin    = IntermediateUtil.GetVec3Attribute(element, "volume_min");
        VolumeMax    = IntermediateUtil.GetVec3Attribute(element, "volume_max");
        VolumeR      = IntermediateUtil.GetDoubleAttribute(element, "volume_r");

        var xmlDisplayElements = element.ChildNodes.OfType<XmlElement>().ToArray();
        if (xmlDisplayElements.Length != DisplaySize)
            throw new Exception();
        Displays = new Display[xmlDisplayElements.Length];
        for (int i = 0; i < xmlDisplayElements.Length; i++)
            Displays[i] = new Display(xmlDisplayElements[i]);

        Array.Sort(Displays, (a, b) => a.Index.CompareTo(b.Index));
    }

    public int           Index;
    public string        Name;
    public NodeKind      Kind;
    public int           Parent;
    public int           Child;
    public int           BrotherNext;
    public int           BrotherPrev;
    public bool          DrawMtx;
    public bool          ScaleCompensate;
    public BillboardType Billboard;
    public Vector3d      Scale;
    public Vector3d      Rotate;
    public Vector3d      Translate;
    public bool          Visibility;
    public int           DisplaySize;
    public int           VertexSize;
    public int           PolygonSize;
    public int           TriangleSize;
    public int           QuadSize;
    public Vector3d      VolumeMin;
    public Vector3d      VolumeMax;
    public double        VolumeR;

    public Display[] Displays;

    public G3dNodeData ToNodeData()
    {
        var result = new G3dNodeData();
        if (Scale == Vector3d.One)
            result.Flags |= G3dNodeData.FLAGS_SCALE_ONE;
        else
        {
            result.Scale    = Scale;
            result.InverseScale = new(1.0 / Scale.X, 1.0 / Scale.Y, 1.0 / Scale.Z);
        }

        if (Translate == Vector3d.Zero)
            result.Flags |= G3dNodeData.FLAGS_TRANSLATION_ZERO;
        else
            result.Translation = Translate;

        var rotX   = Matrix3d.CreateRotationX(MathHelper.DegreesToRadians(Rotate.X));
        var rotY   = Matrix3d.CreateRotationY(MathHelper.DegreesToRadians(Rotate.Y));
        var rotZ   = Matrix3d.CreateRotationZ(MathHelper.DegreesToRadians(Rotate.Z));
        var rotMtx = rotX * rotY * rotZ;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                rotMtx[i, j] = Fx32Util.Fix(rotMtx[i, j]);

        if (rotMtx == Matrix3d.Identity)
        {
            result.Flags |= G3dNodeData.FLAGS_ROTATION_ZERO;
            result._00  =  1;
        }
        else
        {
            int unitRow = -1;
            int unitCol = -1;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (rotMtx[i, j] != 1 && rotMtx[i, j] != -1)
                        continue;
                    bool ok = true;
                    for (int k = 1; k <= 2; k++)
                    {
                        if (rotMtx[i, (j + k) % 3] != 0)
                        {
                            ok = false;
                            break;
                        }

                        if (rotMtx[(i + k) % 3, j] != 0)
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                    {
                        unitRow = i;
                        unitCol = j;
                        break;
                    }
                }

                if (unitRow != -1)
                    break;
            }

            result._00 = rotMtx[0, 0];
            if (unitRow >= 0 && unitCol >= 0)
            {
                int pivot = unitRow * 3 + unitCol;

                result.Flags |= G3dNodeData.FLAGS_ROTATION_PIVOT;
                result.Flags |= (ushort)(pivot << G3dNodeData.FLAGS_ROTATION_PIVOT_INDEX_SHIFT);

                if (rotMtx[pivot / 3, pivot % 3] == -1)
                    result.Flags |= G3dNodeData.FLAGS_ROTATION_PIVOT_NEGATIVE;

                int aIdx = G3dUtil.PivotUtil[pivot, 0];
                int bIdx = G3dUtil.PivotUtil[pivot, 1];
                int cIdx = G3dUtil.PivotUtil[pivot, 2];
                int dIdx = G3dUtil.PivotUtil[pivot, 3];

                result.A = rotMtx[aIdx / 3, aIdx % 3];
                result.B = rotMtx[bIdx / 3, bIdx % 3];

                if (System.Math.Sign(rotMtx[cIdx / 3, cIdx % 3]) != System.Math.Sign(result.B))
                    result.Flags |= G3dNodeData.FLAGS_ROTATION_PIVOT_SIGN_REVERSE_C;

                if (System.Math.Sign(rotMtx[dIdx / 3, dIdx % 3]) != System.Math.Sign(result.A))
                    result.Flags |= G3dNodeData.FLAGS_ROTATION_PIVOT_SIGN_REVERSE_D;
            }
            else
            {
                result._01 = rotMtx[0, 1];
                result._02 = rotMtx[0, 2];
                result._10 = rotMtx[1, 0];
                result._11 = rotMtx[1, 1];
                result._12 = rotMtx[1, 2];
                result._20 = rotMtx[2, 0];
                result._21 = rotMtx[2, 1];
                result._22 = rotMtx[2, 2];
            }
        }

        result.Flags |= 0x1F << G3dNodeData.FLAGS_MATRIX_STACK_INDEX_SHIFT;

        return result;
    }

    public class Display
    {
        public Display(XmlElement element)
        {
            Index    = IntermediateUtil.GetIntAttribute(element, "index");
            Material = IntermediateUtil.GetIntAttribute(element, "material");
            Polygon  = IntermediateUtil.GetIntAttribute(element, "polygon");
            Priority = IntermediateUtil.GetIntAttribute(element, "priority");
        }

        public int Index;
        public int Material;
        public int Polygon;
        public int Priority;
    }
}