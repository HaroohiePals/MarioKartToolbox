namespace HaroohiePals.Nitro.JNLib.Spl
{
    internal static class FloatHelper
    {
        public static byte ToByte(double x, string fieldName = "")
        {
            int result = (int)System.Math.Round(x * 256);
            if (result < 0 || result >= 256)
                throw new Exception($"{fieldName} out of range (0 <= x <= 0.99609375)");
            return (byte)result;
        }
    }
}