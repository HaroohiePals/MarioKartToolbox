using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj
{
    public class MObjModel
    {
        private MkdsContext _context;

        public Vector3d       Scale;
        public BillboardModel BbModel;
        public Model          Model;
        public ShadowModel    ShadowModel;
        public Nsbmd          Nsbmd;
        public AnimManager    NsbcaAnim;
        public AnimManager    NsbtpAnim;
        public AnimManager    NsbmaAnim;
        public AnimManager    NsbtaAnim;

        public MObjModel(MkdsContext context)
        {
            _context = context;
        }

        public void sub_20EA7CC(byte alpha)
        {
            var  v3            = Model.RenderObj.ModelResource;
            uint dword_216C3D4 = v3.Materials.Materials[0].PolygonAttribute.Alpha;
            if (alpha < dword_216C3D4)
            {
                v3.SetAllPolygonId(_context.MObjState.GetCyclicPolygonId());
                v3.SetAllAlpha(alpha);
            }

            Model.Render();
            if (alpha < dword_216C3D4)
                v3.SetAllAlpha((byte)dword_216C3D4);
        }

        public void Render(MObjInstance instance)
        {
            _context.RenderContext.GeState.Scale(Scale);
            BbModel?.Render((byte)instance.Alpha);
            if (Model != null)
            {
                if (NsbmaAnim != null)
                {
                    //sub_20EA670(instance.Alpha);
                }
                else
                    sub_20EA7CC((byte)instance.Alpha);
            }

            ShadowModel?.Render((byte)instance.Alpha);
        }

        public void SetAllPolygonIds(byte id)
        {
            if (BbModel != null)
                BbModel.PolygonAttr.PolygonId = id;
            Model?.SetPolygonId(id);
            if (ShadowModel != null)
                ShadowModel.PolygonId = id;
        }

        public void SetNsbtpFrame(int frame)
        {
            if (BbModel != null)
                BbModel.TexIdx = (ushort)frame;
            else if (Model != null)
                NsbtpAnim.SetFrame(frame);
        }
    }
}