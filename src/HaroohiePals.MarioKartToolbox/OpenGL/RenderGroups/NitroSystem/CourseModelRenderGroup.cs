using HaroohiePals.Gui.Viewport;
using HaroohiePals.Nitro.NitroSystem.G3d;
using HaroohiePals.Nitro.NitroSystem.G3d.Animation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TexturePatternAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using HaroohiePals.Nitro.NitroSystem.G3d.OpenGL;
using HaroohiePals.NitroKart.Course;
using OpenTK.Graphics.OpenGL4;
using System;

namespace HaroohiePals.MarioKartToolbox.OpenGL.RenderGroups.NitroSystem
{
    public class CourseModelRenderGroup : RenderGroup, IDisposable
    {
        private IMkdsCourse _course;
        private G3dModelManager _modelManager;
        private GLG3dModelRenderer _renderer;

        private G3dRenderObject _renderObj;
        private G3dRenderObject _renderObjSky;

        private G3dAnimationObject _anmObj;
        private G3dAnimationObject _anmObjSky;

        private G3dAnimationObject _anmObjPat;

        private string _nsbmdPath = "course_model.nsbmd";
        private string _nsbtxPath = "course_model.nsbtx";
        private string _skyNsbmdPath = "course_model_V.nsbmd";
        private string _skyNsbtxPath = "course_model_V.nsbtx";
        private string _skyNsbtaPath = "course_model_V.nsbta";
        private string _nsbtaPath = "course_model.nsbta";
        private string _nsbtpPath = "course_model.nsbtp";


        public bool EnableCourseModel = true;
        public bool EnableCourseModelV = true;
        public bool WireframeCourseModel = false;
        public bool WireframeCourseModelV = false;

        public CourseModelRenderGroup()
        {
            _modelManager = new GLG3dModelManager();
        }

        public void Load(IMkdsCourse course)
        {
            _course = course;

            InitModel();
        }

        public void Load()
        {
            InitModel();
        }

        public override void Update(float deltaTime)
        {
            if (_anmObj != null)
                _anmObj.Frame = (_anmObj.Frame + deltaTime * 60) % _anmObj.AnimationResource.NrFrames;

            if (_anmObjPat != null)
                _anmObjPat.Frame = (_anmObjPat.Frame + deltaTime * 60) % _anmObjPat.AnimationResource.NrFrames;

            if (_anmObjSky != null)
                _anmObjSky.Frame = (_anmObjSky.Frame + deltaTime * 60) % _anmObjSky.AnimationResource.NrFrames;
        }

        public override void Render(ViewportContext context)
        {
            if (_renderer is null)
                return;

            SetupRendererLight(context);

            if (EnableCourseModel && _renderObj is not null)
            {
                _renderer.RenderObj = _renderObj;
                _renderer.EnableWireframe = WireframeCourseModel;
                _renderer.Render(context.ViewMatrix, context.ProjectionMatrix, ViewportContext.InvalidPickingId, context.TranslucentPass);
            }

            if (EnableCourseModelV && _renderObjSky is not null)
            {
                _renderer.RenderObj = _renderObjSky;
                _renderer.EnableWireframe = WireframeCourseModel;
                _renderer.Render(context.ViewMatrix, context.ProjectionMatrix, ViewportContext.InvalidPickingId, context.TranslucentPass);
            }
        }

        public void Unload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _renderer?.Dispose();
            _modelManager.CleanupRenderObject(_renderObj);
        }

        private void InitCourseModel()
        {
            var nsbmd = _course.GetMainFileOrDefault<Nsbmd>(_nsbmdPath);
            if (nsbmd != null)
            {
                var nsbtx = _course.GetTexFileOrDefault<Nsbtx>(_nsbtxPath);
                Nsbta nsbta;
                try
                {
                    nsbta = _course.GetMainFileOrDefault<Nsbta>(_nsbtaPath);
                }
                catch
                {
                    nsbta = null;
                }

                Nsbtp nsbtp;
                try
                {
                    nsbtp = _course.GetMainFileOrDefault<Nsbtp>(_nsbtpPath);
                }
                catch
                {
                    nsbtp = null;
                }

                var model = nsbmd.ModelSet.Models[0];

                model.SetAllPolygonId(8);
                model.SetAllTranslucentDepthUpdate(true);

                switch (_course.MapData.StageInfo.CourseId)
                {
                    case 22: //12: //COURSE_OLD_KOOPA_AGB
                        if (model.Materials.MaterialDictionary.Contains("yogan"))
                        {
                            var mat = model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("yogan")];
                            mat.SetAlpha(30);
                            mat.SetTranslucentDepthUpdate(false);
                        }

                        break;
                    case 38: //22: //COURSE_BANK_COURSE
                        if (model.Materials.MaterialDictionary.Contains("falls_01"))
                        {
                            model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("falls_01")]
                                .SetTranslucentDepthUpdate(false);
                        }

                        if (model.Materials.MaterialDictionary.Contains("falls_02"))
                        {
                            model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("falls_02")]
                                .SetTranslucentDepthUpdate(false);
                        }

                        break;
                }

                if (model.Materials.MaterialDictionary.Contains("ShdwOff"))
                    model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("ShdwOff")].SetPolygonId(7);
                if (model.Materials.MaterialDictionary.Contains("ShdwOff1"))
                    model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("ShdwOff1")].SetPolygonId(7);
                if (model.Materials.MaterialDictionary.Contains("ShdwOff2"))
                    model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("ShdwOff2")].SetPolygonId(7);
                if (model.Materials.MaterialDictionary.Contains("ShdwOff3"))
                    model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("ShdwOff3")].SetPolygonId(7);
                if (model.Materials.MaterialDictionary.Contains("ShdwOff4"))
                    model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("ShdwOff4")].SetPolygonId(7);
                if (model.Materials.MaterialDictionary.Contains("ShdwOff5"))
                    model.Materials.Materials[model.Materials.MaterialDictionary.IndexOf("ShdwOff5")].SetPolygonId(7);

                bool modelHasPartialFog = false;
                switch (_course.MapData.StageInfo.CourseId)
                {
                    case 35: //18: //COURSE_MANSION_COURSE
                    case 42: //27: //COURSE_DESERT_COURSE
                    case 10: //45: //COURSE_MINI_STAGE2
                        modelHasPartialFog = true;
                        break;
                }

                if (!modelHasPartialFog)
                    model.SetAllFogEnable(true);

                var textures = nsbtx == null ? nsbmd.TextureSet : nsbtx.TextureSet;

                _renderObj = new G3dRenderObject(model);
                _modelManager.InitializeRenderObject(_renderObj, textures);

                if (nsbta != null)
                {
                    _anmObj = new G3dAnimationObject(nsbta.TextureSrtAnimationSet.TextureSrtAnimations[0], model, textures);
                    _renderObj.AddAnimationObject(_anmObj);
                }

                if (nsbtp != null)
                {
                    _anmObjPat = new G3dAnimationObject(nsbtp.TexturePatternAnimationSet.TexturePatternAnimations[0], model, textures);
                    _modelManager.InitializeTexturePatternAnimationObject(_anmObjPat);
                    _renderObj.AddAnimationObject(_anmObjPat);
                }
            }

        }

        private void InitSkyModel()
        {
            var nsbmd = _course.GetMainFileOrDefault<Nsbmd>(_skyNsbmdPath);
            if (nsbmd != null)
            {
                var nsbtx = _course.GetTexFileOrDefault<Nsbtx>(_skyNsbtxPath);
                Nsbta nsbta;
                try
                {
                    nsbta = _course.GetMainFileOrDefault<Nsbta>(_skyNsbtaPath);
                }
                catch
                {
                    nsbta = null;
                }

                if (nsbmd != null)
                {
                    var model = nsbmd.ModelSet.Models[0];

                    bool modelVHasPartialFog = false;
                    switch (_course.MapData.StageInfo.CourseId)
                    {
                        case 44: //29: //COURSE_RAINBOW_COURSE
                            modelVHasPartialFog = true;
                            break;
                    }

                    //if (!modelVHasPartialFog)
                    //    model.SetAllFogEnable(true);

                    var textures = nsbtx == null ? nsbmd.TextureSet : nsbtx.TextureSet;

                    model.SetAllPolygonId(8);

                    _renderObjSky = new G3dRenderObject(model);
                    _modelManager.InitializeRenderObject(_renderObjSky, textures);

                    if (nsbta != null)
                    {
                        _anmObjSky = new G3dAnimationObject(nsbta.TextureSrtAnimationSet.TextureSrtAnimations[0], model, textures);
                        _renderObjSky.AddAnimationObject(_anmObjSky);
                    }
                }
                else
                {
                    _renderObjSky = null;
                }
            }

        }

        private void InitModel()
        {
            if (_renderer is not null)
            {
                Unload();
            }

            _renderer = new();

            InitCourseModel();
            InitSkyModel();
        }

        private void SetupRendererLight(ViewportContext context)
        {
            _renderer.LightVectors[0] = (0, -1, 0);
            _renderer.LightVectors[1] =
                (-context.ViewMatrix.Column2.Xyz - context.ViewMatrix.Column1.Xyz).Normalized();
            _renderer.LightVectors[2] = (0, -1, 0);
            _renderer.LightVectors[3] = (0, 1, 0);

            _renderer.LightColors[0] = new(31, 31, 31);
            _renderer.LightColors[1] = new(31, 31, 31);
            _renderer.LightColors[2] = new(31, 0, 0);
            _renderer.LightColors[3] = new(31, 31, 0); //this should be white in koopa_course
        }

        public void Dispose()
        {
            _renderer?.Dispose();
            _modelManager?.Dispose();
        }
    }
}