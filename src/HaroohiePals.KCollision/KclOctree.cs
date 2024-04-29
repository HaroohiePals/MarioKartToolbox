using HaroohiePals.IO;
using OpenTK.Mathematics;

namespace HaroohiePals.KCollision
{
    public class KclOctree
    {
        public enum CompressionMethod
        {
            Equal,
            Merge
        }

        private class PrismList
        {
            public long Address;

            public readonly List<KclOctreeNode> Nodes  = new();
            public readonly HashSet<ushort>     Prisms = new();

            public void Add(KclOctreeNode node)
            {
                Nodes.Add(node);
                Prisms.UnionWith(node.Prisms);
            }

            public void Add(PrismList list)
            {
                Nodes.AddRange(list.Nodes);
                Prisms.UnionWith(list.Prisms);
            }

            public void Remove(KclOctreeNode node)
            {
                Nodes.Remove(node);
                Prisms.Clear();
                foreach (var n in Nodes)
                    Prisms.UnionWith(n.Prisms);
            }
        }

        public KclOctree() { }

        public KclOctree(Kcl kcl)
        {
            using var er = new EndianBinaryReaderEx(new MemoryStream(kcl.Octree), kcl.Endianness);

            PrismThickness = kcl.PrismThickness;
            MinPos         = kcl.AreaMinPos;
            RootCubeSize   = kcl.OctreeRootCubeSize;
            XNodes         = kcl.OctreeXNodes;
            YNodes         = kcl.OctreeYNodes;
            ZNodes         = kcl.OctreeZNodes;
            SphereRadius   = kcl.SphereRadius;

            int nodeCount = XNodes * YNodes * ZNodes;
            RootNodes = new KclOctreeNode[nodeCount];
            for (int i = 0; i < nodeCount; i++)
                RootNodes[i] = new KclOctreeNode(er);
        }

        public double   PrismThickness;
        public Vector3d MinPos;
        public int      RootCubeSize;
        public int      XNodes;
        public int      YNodes;
        public int      ZNodes;
        public double   SphereRadius;

        public KclOctreeNode[] RootNodes;

        public byte[] Write(Endianness endianness, CompressionMethod compressionMethod)
        {
            var       m  = new MemoryStream();
            using var er = new EndianBinaryWriterEx(m, endianness);

            switch (compressionMethod)
            {
                case CompressionMethod.Equal:
                    WriteEqual(er);
                    break;
                case CompressionMethod.Merge:
                    WriteMerge(er, 10); // todo: this limit should be configurable
                    break;
            }

            return m.ToArray();
        }

        private IReadOnlyDictionary<KclOctreeNode, (long baseOffs, long ptr)> WriteNodes(EndianBinaryWriter er)
        {
            var nodes     = new Queue<(KclOctreeNode node, long baseOffs, long ptr)>();
            var nodeAddrs = new Dictionary<KclOctreeNode, (long baseOffs, long ptr)>();

            //write cubes (breadth first order)
            for (var i = 0; i < RootNodes.Length; i++)
                nodes.Enqueue((RootNodes[i], er.BaseStream.Position, i * 4));

            er.Write(new uint[RootNodes.Length], 0, RootNodes.Length);
            while (nodes.Count > 0)
            {
                var (node, baseOffs, ptr) = nodes.Dequeue();
                if (node.IsLeaf)
                    nodeAddrs.Add(node, (baseOffs, ptr));
                else
                {
                    long curpos = er.BaseStream.Position;
                    er.BaseStream.Position = baseOffs + ptr;
                    er.Write((uint)(curpos - baseOffs));
                    er.BaseStream.Position = curpos;
                    for (var i = 0; i < 8; i++)
                        nodes.Enqueue((node.Children[i], er.BaseStream.Position, i * 4));
                    er.Write(new uint[8], 0, 8);
                }
            }

            return nodeAddrs;
        }

        private static void WriteLeafData(EndianBinaryWriter er, IEnumerable<PrismList> lists)
        {
            int idx = 0;
            foreach (var data in lists)
            {
                data.Address = er.BaseStream.Position - 2;
                if (data.Prisms.Count > 0 || idx == 0)
                {
                    foreach (var v in data.Prisms.OrderBy(t => t))
                        er.Write((ushort)(v + 1));
                    er.Write((ushort)0);
                }
                else
                    data.Address -= 2;

                idx++;
            }
        }

        private static void WriteLeafPointers(EndianBinaryWriter er,
            IReadOnlyDictionary<KclOctreeNode, PrismList> listForNode,
            IReadOnlyDictionary<KclOctreeNode, (long baseOffs, long ptr)> nodeAddrs)
        {
            foreach (var pair in listForNode)
            {
                var (baseOffs, ptr) = nodeAddrs[pair.Key];
                long curpos = er.BaseStream.Position;
                er.BaseStream.Position = baseOffs + ptr;
                er.Write((uint)(pair.Value.Address - baseOffs) | 0x80000000u);
                er.BaseStream.Position = curpos;
            }
        }

        private void WriteEqual(EndianBinaryWriterEx er)
        {
            var nodeAddrs = WriteNodes(er);

            //traverse leaves (depth first order)
            var indexData   = new LinkedList<PrismList>();
            var nodeIdxData = new Dictionary<KclOctreeNode, PrismList>();
            var nodes2      = new Stack<KclOctreeNode>();
            for (int i = RootNodes.Length - 1; i >= 0; i--)
                nodes2.Push(RootNodes[i]);
            while (nodes2.Count > 0)
            {
                var node = nodes2.Pop();
                if (node.IsLeaf)
                {
                    //check for existing index data
                    bool done     = false;
                    var  dataNode = indexData.First;
                    while (dataNode != null)
                    {
                        var  data  = dataNode.Value;
                        bool equal = data.Prisms.SetEquals(node.Prisms);

                        if (equal)
                        {
                            indexData.Remove(dataNode);
                            indexData.AddLast(data);
                            nodeIdxData.Add(node, data);
                            done = true;
                            break;
                        }

                        dataNode = dataNode.Next;
                    }

                    if (!done)
                    {
                        var data = new PrismList();
                        data.Add(node);
                        indexData.AddLast(data);
                        nodeIdxData.Add(node, data);
                    }
                }
                else
                {
                    for (var i = 7; i >= 0; i--)
                        nodes2.Push(node.Children[i]);
                }
            }

            //move the empty list to the end
            var cur = indexData.First;
            while (cur != null)
            {
                if (cur.Value.Prisms.Count == 0)
                    break;
                cur = cur.Next;
            }

            if (cur != null)
            {
                indexData.Remove(cur);
                indexData.AddLast(cur.Value);
            }

            WriteLeafData(er, indexData);
            WriteLeafPointers(er, nodeIdxData, nodeAddrs);
        }

        private void WriteMerge(EndianBinaryWriterEx er, int prismLimit)
        {
            var leafNodes   = new List<KclOctreeNode>();
            var listForNode = new Dictionary<KclOctreeNode, PrismList>();
            var queue       = new Queue<KclOctreeNode>();
            foreach (var node in RootNodes)
                queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (node.Children == null)
                    leafNodes.Add(node);
                else
                {
                    foreach (var subNode in node.Children)
                        queue.Enqueue(subNode);
                }
            }

            var lists = leafNodes.Select(n =>
            {
                var l = new PrismList();
                l.Add(n);
                listForNode[n] = l;
                return l;
            }).ToList();

            bool change = true;
            while (change)
            {
                change = false;
                for (int i = 0; i < lists.Count; i++)
                {
                    for (int j = i + 1; j < lists.Count; j++)
                    {
                        if (lists[i].Prisms.Count == 0 || lists[j].Prisms.Count == 0)
                        {
                            if (lists[i].Prisms.Count == lists[j].Prisms.Count)
                            {
                                lists[i].Add(lists[j]);
                                foreach (var node in lists[j].Nodes)
                                    listForNode[node] = lists[i];
                                lists.RemoveAt(j);

                                change = true;
                                j--;
                            }

                            continue;
                        }

                        if (lists[i].Prisms.SetEquals(lists[j].Prisms))
                        {
                            lists[i].Add(lists[j]);
                            foreach (var node in lists[j].Nodes)
                                listForNode[node] = lists[i];
                            lists.RemoveAt(j);

                            change = true;
                            j--;
                        }
                        else if (lists[j].Prisms.IsSubsetOf(lists[i].Prisms) && lists[i].Prisms.Count <= prismLimit)
                        {
                            lists[i].Add(lists[j]);
                            foreach (var node in lists[j].Nodes)
                                listForNode[node] = lists[i];
                            lists.RemoveAt(j);

                            change = true;
                            j--;
                        }
                    }
                }
            }

            for (int i = 0; i < lists.Count; i++)
            {
                for (int j = i + 1; j < lists.Count; j++)
                {
                    if (lists[i].Prisms.Count == 0 || lists[j].Prisms.Count == 0)
                        continue;

                    int limit         = prismLimit;
                    int togetherLimit = Math.Min(lists[i].Prisms.Count, lists[j].Prisms.Count);
                    if (togetherLimit > limit)
                        limit = togetherLimit;
                    if (lists[i].Prisms.Union(lists[j].Prisms).Count() <= limit)
                    {
                        lists[i].Add(lists[j]);
                        foreach (var node in lists[j].Nodes)
                            listForNode[node] = lists[i];
                        lists.RemoveAt(j);

                        j = i;
                    }
                }
            }

            var nodeAddrs = WriteNodes(er);

            // var listSizes = new Dictionary<int, int>();
            // foreach (var data in lists)
            // {
            //     if (!listSizes.ContainsKey(data.Triangles.Count))
            //         listSizes[data.Triangles.Count] = 1;
            //     else
            //         listSizes[data.Triangles.Count]++;
            // }

            WriteLeafData(er, lists);
            WriteLeafPointers(er, listForNode, nodeAddrs);
        }
    }
}