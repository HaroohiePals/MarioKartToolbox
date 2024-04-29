using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.JointAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;
using System;

namespace HaroohiePals.NitroKart.MapObj
{
    public class AnimManager
    {
        public enum AnimKind
        {
            Jnt,
            Srt,
            Pat,
            Vis,
            Mat
        }

        public Animator Animator;
        public Model    Model;
        public int      AnimCount;
        public int      CurAnimIdx;
        public G3dAnimationObject[] AnmObjs;
        public bool[]   LoopFlags;
        public AnimKind Kind;
        public bool     IsBlending;
        public double   BlendSpeed;
        public G3dAnimationObject   BlendAnmObj;

        public AnimManager(AnimKind kind, Model model, int animCount)
        {
            AnimCount = animCount;
            AnmObjs   = new G3dAnimationObject[AnimCount];
            LoopFlags = new bool[AnimCount];
            Animator  = new Animator(0, true);

            Model                = model;
            model.Render1Mat1Shp = false; //because animations don't work otherwise

            CurAnimIdx  = 0;
            Kind        = kind;
            IsBlending  = false;
            BlendSpeed  = 0.1;
            BlendAnmObj = null;
        }

        public void RegisterAnim(int idx, G3dAnimation anim, bool loop)
        {
            var anmObj = new G3dAnimationObject(anim, Model.RenderObj.ModelResource, Model.RenderObj.Textures);
            AnmObjs[idx]   = anmObj;
            LoopFlags[idx] = loop;
        }

        public void RegisterAllAnims(Nsbca animRes)
        {
            if (Kind != AnimKind.Jnt)
                throw new Exception();

            for (int i = 0; i < AnimCount; i++)
                RegisterAnim(i, animRes.JointAnimationSet.JointAnimations[i], false);
        }

        public void RegisterAllAnims(Nsbtp animRes)
        {
            if (Kind != AnimKind.Pat)
                throw new Exception();

            for (int i = 0; i < AnimCount; i++)
                RegisterAnim(i, animRes.TexturePatternAnimationSet.TexturePatternAnimations[i], false);
        }

        public void RegisterAllAnims(Nsbta animRes)
        {
            if (Kind != AnimKind.Srt)
                throw new Exception();

            for (int i = 0; i < AnimCount; i++)
                RegisterAnim(i, animRes.TextureSrtAnimationSet.TextureSrtAnimations[i], false);
        }

        public void SetAnim(int idx)
        {
            if (AnmObjs[idx] == null)
                return;
            IsBlending = false;
            if (BlendAnmObj != null)
            {
                Model.RenderObj.RemoveAnimationObject(BlendAnmObj);
                BlendAnmObj = null;
            }

            Model.RenderObj.RemoveAnimationObject(AnmObjs[CurAnimIdx]);
            CurAnimIdx = idx;
            Model.RenderObj.AddAnimationObject(AnmObjs[CurAnimIdx]);
            AnmObjs[CurAnimIdx].Ratio = 1;

            Animator = new Animator(GetAnimLength(), LoopFlags[CurAnimIdx]);
        }

        public void SetAnimWithBlend(int idx)
        {
            if (AnmObjs[idx] == null || CurAnimIdx == idx)
                return;
            IsBlending = true;
            if (BlendAnmObj != null)
            {
                Model.RenderObj.RemoveAnimationObject(BlendAnmObj);
                BlendAnmObj = null;
            }

            BlendAnmObj       = AnmObjs[CurAnimIdx];
            BlendAnmObj.Ratio = 1;
            CurAnimIdx        = idx;
            Model.RenderObj.AddAnimationObject(AnmObjs[CurAnimIdx]);
            AnmObjs[CurAnimIdx].Ratio = 0;

            Animator = new Animator(GetAnimLength(), LoopFlags[CurAnimIdx]);
        }

        public void Update()
        {
            UpdateBlend();
            Animator.Update();
            AnmObjs[CurAnimIdx].Frame = Animator.Progress;
        }

        public void UpdateBlend()
        {
            if (!IsBlending)
                return;
            AnmObjs[CurAnimIdx].Ratio += BlendSpeed;
            BlendAnmObj.Ratio         -= BlendSpeed;
            if (AnmObjs[CurAnimIdx].Ratio >= 1)
            {
                IsBlending                = false;
                AnmObjs[CurAnimIdx].Ratio = 1;
                Model.RenderObj.RemoveAnimationObject(BlendAnmObj);
                BlendAnmObj = null;
            }
        }

        public double GetAnimLength()
        {
            return AnmObjs[CurAnimIdx].AnimationResource.NrFrames;
        }

        public void SetFrame(double frame)
        {
            AnmObjs[CurAnimIdx].Frame = Animator.Progress = frame;
        }

        public void SetLoop(int idx, bool loop)
        {
            LoopFlags[idx]    = loop;
            Animator.LoopMode = loop ? Animator.AnimLoopMode.InfiniteLoop : Animator.AnimLoopMode.Stop;
        }
    }
}