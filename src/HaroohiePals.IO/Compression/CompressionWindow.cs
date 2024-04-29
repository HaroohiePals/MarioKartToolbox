using System.Collections.Generic;

namespace HaroohiePals.IO.Compression
{
    public class CompressionWindow
    {
        private readonly byte[]             _src;
        private readonly LinkedList<uint>[] _windowDict = new LinkedList<uint>[256];

        public uint Position { get; private set; }

        public int MinRun     { get; }
        public int MaxRun     { get; }
        public int WindowSize { get; }

        public CompressionWindow(byte[] src, int minRun, int maxRun, int windowSize)
        {
            _src       = src;
            MinRun     = minRun;
            MaxRun     = maxRun;
            WindowSize = windowSize;
            for (int i = 0; i < 256; i++)
                _windowDict[i] = new LinkedList<uint>();
        }

        public unsafe (uint pos, int len) FindRun()
        {
            fixed (byte* pSrc = &_src[0])
            {
                var  start   = _windowDict[pSrc[Position]].First;
                int  bestLen = -1;
                uint bestPos = 0;
                while (start != null)
                {
                    uint pos = start.Value;

                    if (Position - pos > WindowSize)
                    {
                        var old = start;
                        start = start.Next;
                        old.List.Remove(old);
                        continue;
                    }

                    int   len    = 1;
                    int   lenMax = System.Math.Min((int)(_src.Length - Position), MaxRun);
                    byte* a      = &pSrc[pos + len];
                    byte* b      = &pSrc[Position + len];
                    while (*a++ == *b++ && len < lenMax)
                        len++;
                    if (len >= MinRun && len > bestLen)
                    {
                        bestLen = len;
                        bestPos = pos;
                        if (len == MaxRun)
                            break;
                    }

                    start = start.Next;
                }

                return (bestPos, bestLen);
            }
        }

        public void Slide(int count)
        {
            for (uint i = Position; i < Position + count; i++)
                _windowDict[_src[i]].AddFirst(i);
            Position = (uint)(Position + count);
        }

        public void Reset()
        {
            Position = 0;
            for (int i = 0; i < 256; i++)
                _windowDict[i].Clear();
        }
    }
}