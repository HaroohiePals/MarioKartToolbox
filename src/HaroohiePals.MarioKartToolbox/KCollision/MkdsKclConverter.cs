using HaroohiePals.Graphics3d;
using HaroohiePals.KCollision;
using HaroohiePals.KCollision.Formats;
using HaroohiePals.Mathematics;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace HaroohiePals.MarioKartToolbox.KCollision
{
    public static class MkdsKclConverter
    {
        public static MkdsKcl FromObj(Obj obj, Dictionary<string, ushort> materialAttributes,
            KclOctreeGenerator.Params octreeParams, KclOctree.CompressionMethod octreeCompressionMethod,
            double areaThreshold = 0.1)
        {
            var kcl = new MkdsKcl();

            // Collect prism data
            var prisms = new List<KclPrismData>();
            var triangles = new List<Triangle>();

            foreach (var face in obj.Faces)
            {
                var triangle = new Triangle(obj.Vertices[face.VertexIndices[0]], obj.Vertices[face.VertexIndices[1]],
                    obj.Vertices[face.VertexIndices[2]]);

                if (triangle.Area > areaThreshold && materialAttributes.ContainsKey(face.Material))
                {
                    triangles.Add(triangle);
                    prisms.Add(KclPrismData.GenerateFx32(triangle, materialAttributes[face.Material]));
                }
            }

            // Collect vertex and normal data
            var vertices = new Dictionary<Vector3d, ushort>();
            var normals = new Dictionary<Vector3d, ushort>();

            ushort vertexId = 0;
            ushort normalId = 0;

            foreach (var prism in prisms)
            {
                if (!vertices.ContainsKey(prism.Position))
                    vertices.Add(prism.Position, vertexId++);
                if (!normals.ContainsKey(prism.FaceNormal))
                    normals.Add(prism.FaceNormal, normalId++);
                if (!normals.ContainsKey(prism.EdgeNormal1))
                    normals.Add(prism.EdgeNormal1, normalId++);
                if (!normals.ContainsKey(prism.EdgeNormal2))
                    normals.Add(prism.EdgeNormal2, normalId++);
                if (!normals.ContainsKey(prism.EdgeNormal3))
                    normals.Add(prism.EdgeNormal3, normalId++);
            }

            // Convert prisms to Fx32 prisms
            var fx32Prisms = prisms.Select(x => new Fx32KclPrism
            {
                Attribute = x.Attribute,
                Height = x.Height,
                PosIdx = vertices[x.Position],
                FNrmIdx = normals[x.FaceNormal],
                ENrm1Idx = normals[x.EdgeNormal1],
                ENrm2Idx = normals[x.EdgeNormal2],
                ENrm3Idx = normals[x.EdgeNormal3]
            });

            kcl.PrismData = fx32Prisms.ToArray();
            kcl.PosData = vertices.Keys.ToArray();
            kcl.NrmData = normals.Keys.ToArray();

            // Generate octree
            var octree = KclOctreeGenerator.Generate(triangles, octreeParams);

            kcl.SetOctree(octree, octreeCompressionMethod);

            return kcl;
        }
    }
}