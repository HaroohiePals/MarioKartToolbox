namespace HaroohiePals.NitroKart.MapObj
{
    public class Animator
    {
        public enum AnimLoopMode
        {
            Stop,
            InfiniteLoop,
            CountedLoop
        }

        public AnimLoopMode LoopMode;
        public bool         HasEnded;
        public double       AnimLength;
        public double       Speed;
        public double       Progress;
        public ushort       LoopIteration;
        public ushort       LoopCount;

        public Animator(double animLength, bool loop)
        {
            LoopMode      = loop ? AnimLoopMode.InfiniteLoop : AnimLoopMode.Stop;
            HasEnded      = false;
            Speed         = 1;
            AnimLength    = animLength;
            Progress      = 0;
            LoopIteration = 0;
            LoopCount     = 0;
        }

        private void HandleLooping()
        {
            switch (LoopMode)
            {
                case AnimLoopMode.InfiniteLoop:
                    if (Progress >= AnimLength)
                        Progress -= AnimLength;
                    else if (Progress <= 0)
                        Progress = AnimLength - 1 + Progress;
                    break;
                case AnimLoopMode.Stop:
                    if (Progress >= AnimLength - 1)
                    {
                        Progress = AnimLength - 1;
                        Speed    = 0;
                        HasEnded = true;
                    }

                    break;
                case AnimLoopMode.CountedLoop:
                    if (LoopIteration >= LoopCount)
                        break;
                    if (Progress >= AnimLength)
                    {
                        Progress -= AnimLength;
                        LoopIteration++;
                    }
                    else if (Progress <= 0)
                    {
                        Progress = AnimLength - 1 + Progress;
                        LoopIteration++;
                    }

                    if (LoopIteration >= LoopCount)
                    {
                        Progress = AnimLength - 1;
                        Speed    = 0;
                        HasEnded = true;
                    }

                    break;
            }
        }

        public void Update()
        {
            HasEnded =  false;
            Progress += Speed;
            HandleLooping();
        }
    }
}