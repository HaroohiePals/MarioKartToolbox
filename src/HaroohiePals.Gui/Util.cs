using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;

namespace HaroohiePals.Gui
{
    internal static class Util
    {
        [Conditional("DEBUG")]
        public static void CheckGLError(string title)
        {
            var error = GL.GetError();
            if (error != ErrorCode.NoError)
            {
                Debug.Print($"{title}: {error}");
            }
        }
    }
}
