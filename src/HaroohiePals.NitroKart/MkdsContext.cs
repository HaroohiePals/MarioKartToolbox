using HaroohiePals.Nitro.NitroSystem.G3d;
using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapObj;

namespace HaroohiePals.NitroKart
{
    public class MkdsContext
    {
        public readonly G3dModelManager ModelManager;
        public readonly RenderContext   RenderContext;

        public IMkdsCourse   Course;
        public MObjState MObjState;

        public MkdsContext(G3dModelManager modelManager, RenderContext renderContext)
        {
            ModelManager  = modelManager;
            RenderContext = renderContext;
        }
    }
}