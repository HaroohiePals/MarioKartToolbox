using HaroohiePals.Mathematics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Vector3d = OpenTK.Mathematics.Vector3d;

namespace HaroohiePals.Graphics3d
{
    public class Obj
    {
        public Obj() { }

        public Obj(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Obj(Stream stream)
        {
            string curMat = null;
            using (var tr = new StreamReader(stream))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length < 1 || line.StartsWith("#")) continue;

                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1) continue;
                    switch (parts[0])
                    {
                        case "mtllib":
                            if (parts.Length < 2) continue;
                            MtlPath = line.Substring(parts[0].Length + 1).Trim();
                            break;
                        case "usemtl":
                            if (parts.Length < 2) continue;
                            curMat = parts[1];
                            break;
                        case "v":
                        {
                            if (parts.Length < 4) continue;
                            double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            Vertices.Add(new(x, y, z));
                            break;
                        }
                        case "vn":
                        {
                            if (parts.Length < 4) continue;
                            double x = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double y = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double z = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            Normals.Add(new(x, y, z));
                            break;
                        }
                        case "vt":
                        {
                            if (parts.Length < 3) continue;
                            double s = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double t = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            TexCoords.Add(new(s, t));
                            break;
                        }
                        case "f":
                        {
                            if (parts.Length < 4) continue;
                            var f = new ObjFace();
                            f.Material = curMat;
                            var vtx = new List<int>();
                            var tex = new List<int>();
                            var nrm = new List<int>();
                            for (int i = 0; i < parts.Length - 1; i++)
                            {
                                var parts2 = parts[i + 1].Split('/');
                                vtx.Add(int.Parse(parts2[0]) - 1);
                                if (parts2.Length > 1)
                                {
                                    if (parts2[1] != "")
                                        tex.Add(int.Parse(parts2[1]) - 1);
                                    if (parts2.Length > 2 && parts2[2] != "")
                                        nrm.Add(int.Parse(parts2[2]) - 1);
                                }
                            }

                            f.VertexIndices   = vtx.ToArray();
                            f.TexCoordIndices = tex.ToArray();
                            f.NormalIndices   = nrm.ToArray();

                            Faces.Add(f);
                            break;
                        }
                    }
                }
            }
        }

        public byte[] Write()
        {
            using (var m = new MemoryStream())
            {
                Write(m);

                return m.ToArray();
            }
        }

        public void Write(Stream stream)
        {
            using (var b = new StreamWriter(stream))
            {
                if (MtlPath != null)
                    b.WriteLine("mtllib {0}", MtlPath);
                b.WriteLine();
                foreach (var vertex in Vertices)
                {
                    b.WriteLine("v {0} {1} {2}",
                        vertex.X.ToString(CultureInfo.InvariantCulture),
                        vertex.Y.ToString(CultureInfo.InvariantCulture),
                        vertex.Z.ToString(CultureInfo.InvariantCulture));
                }

                b.WriteLine();
                foreach (var normal in Normals)
                {
                    b.WriteLine("vn {0} {1} {2}",
                        normal.X.ToString(CultureInfo.InvariantCulture),
                        normal.Y.ToString(CultureInfo.InvariantCulture),
                        normal.Z.ToString(CultureInfo.InvariantCulture));
                }

                b.WriteLine();
                foreach (var texCoord in TexCoords)
                {
                    b.WriteLine("vt {0} {1}",
                        texCoord.X.ToString(CultureInfo.InvariantCulture),
                        texCoord.Y.ToString(CultureInfo.InvariantCulture));
                }

                b.WriteLine();
                string curMat = null;
                foreach (var c in Faces)
                {
                    bool vertex = c.VertexIndices?.Length != 0;
                    bool normal = c.NormalIndices?.Length != 0 && c.NormalIndices?.Length == c.VertexIndices?.Length;
                    bool tex = c.TexCoordIndices?.Length != 0 &&
                               c.TexCoordIndices?.Length == c.VertexIndices?.Length;
                    if (!vertex)
                        throw new Exception("Face has no vertex entries!");
                    if (curMat != c.Material)
                    {
                        b.WriteLine("usemtl {0}", c.Material);
                        b.WriteLine();
                        curMat = c.Material;
                    }

                    b.Write("f");

                    int count = c.VertexIndices.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (vertex && normal && tex)
                            b.Write(" {0}/{1}/{2}", c.VertexIndices[i] + 1, c.TexCoordIndices[i] + 1,
                                c.NormalIndices[i] + 1);
                        else if (vertex && tex)
                            b.Write(" {0}/{1}", c.VertexIndices[i] + 1, c.TexCoordIndices[i] + 1);
                        else if (vertex && normal)
                            b.Write(" {0}//{1}", c.VertexIndices[i] + 1, c.NormalIndices[i] + 1);
                        else
                            b.Write(" {0}", c.VertexIndices[i] + 1);
                    }

                    b.WriteLine();
                }
            }
        }

        public string MtlPath; //Relative to this OBJ file

        public readonly List<Vector3d> Vertices  = new();
        public readonly List<Vector3d> Normals   = new();
        public readonly List<Vector2d> TexCoords = new();

        public readonly List<ObjFace> Faces = new();

        public class ObjFace
        {
            public ObjFace() { }

            public ObjFace(params int[] vtxIndices)
            {
                VertexIndices = vtxIndices;
            }

            public ObjFace(string material, params int[] vtxIndices)
            {
                Material      = material;
                VertexIndices = vtxIndices;
            }

            public int[]  VertexIndices;
            public int[]  NormalIndices;
            public int[]  TexCoordIndices;
            public String Material;
        }

        public static Obj FromTriangles(IEnumerable<Triangle> triangles)
        {
            var obj = new Obj();
            foreach (var tri in triangles)
            {
                int idx = obj.Vertices.Count;
                obj.Vertices.Add(tri.PointA);
                obj.Vertices.Add(tri.PointB);
                obj.Vertices.Add(tri.PointC);
                obj.Faces.Add(new ObjFace(idx, idx + 1, idx + 2));
            }

            return obj;
        }
    }
}