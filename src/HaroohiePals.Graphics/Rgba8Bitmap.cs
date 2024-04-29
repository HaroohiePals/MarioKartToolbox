using System;

namespace HaroohiePals.Graphics
{
    public class Rgba8Bitmap
    {
        public int    Width  { get; }
        public int    Height { get; }
        public uint[] Pixels { get; }

        public Rgba8Bitmap(int width, int height)
        {
            Width  = width;
            Height = height;

            Pixels = new uint[width * height];
        }

        public Rgba8Bitmap(int width, int height, uint[] data)
        {
            Width  = width;
            Height = height;

            Pixels = new uint[width * height];
            Array.Copy(data, Pixels, Pixels.Length);
        }

        public uint this[int x, int y]
        {
            get => Pixels[y * Width + x];
            set => Pixels[y * Width + x] = value;
        }

        //todo: add utils here perhaps
    }
}