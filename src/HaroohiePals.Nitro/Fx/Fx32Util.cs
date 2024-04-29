using OpenTK.Mathematics;

namespace HaroohiePals.Nitro.Fx
{
    public static class Fx32Util
    {
        /// <summary>
        /// Converts/rounds the given value to a valid fx32 value
        /// </summary>
        public static double Fix(double value)
            => ToDouble(FromDouble(value));

        /// <summary>
        /// Converts/rounds the given vector to valid fx32 values
        /// </summary>
        public static Vector2d Fix(Vector2d value)
            => new(Fix(value.X), Fix(value.Y));

        /// <summary>
        /// Converts/rounds the given vector to valid fx32 values
        /// </summary>
        public static Vector3d Fix(Vector3d value)
            => new(Fix(value.X), Fix(value.Y), Fix(value.Z));

        public static int FromDouble(double value)
            => (int)System.Math.Round(value * 4096d);

        public static double ToDouble(int value)
            => value / 4096d;
    }
}