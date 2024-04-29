using HaroohiePals.NitroKart.MapData;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Scenery
{
    [MapObj(MkdsMapObjectId.Chandelier, new[] { typeof(Chandelier.RenderPart) }, typeof(Chandelier.LogicPart))]
    public class Chandelier : MObjInstance
    {
        private enum ChandelierState
        {
            Wait,
            Move
        }

        private ushort          _nsbcaFrame;
        private int             _counter;
        private ChandelierState _state;

        public Chandelier(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart) { }

        private void Update()
        {
            switch (_state)
            {
                case ChandelierState.Wait:
                    if (_counter-- <= 0)
                        _state = ChandelierState.Move;
                    break;
                case ChandelierState.Move:
                {
                    // if (instance._nsbcaFrame == 0 || instance._nsbcaFrame == 125)
                    //     mobj_emitSfxIfAudible(&instance->mobj, SET_SHANDELIER_L_SU);
                    // else if (instance->nsbcaFrame == 70 || instance->nsbcaFrame == 175)
                    //     mobj_emitSfxIfAudible(&instance->mobj, SET_SHANDELIER_R_SU);
                    // else if (instance->nsbcaFrame == 250)
                    //     mobj_emitSfxIfAudible(&instance->mobj, SET_SHANDELIER_SU);

                    var renderPart = (RenderPart)_renderParts[0];

                    int nextFrame = _nsbcaFrame + 1;
                    if (nextFrame >= (int)renderPart.Model.NsbcaAnim.GetAnimLength())
                        nextFrame = 0;
                    _nsbcaFrame = (ushort)nextFrame;
                    if (_nsbcaFrame == 0)
                        _state = ChandelierState.Wait;
                    break;
                }
            }
        }

        private void Render(in Matrix4x3d camMtx, ushort alpha)
        {
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetYBillboardAtPos(Position, camMtx));
            _context.RenderContext.GeState.Scale(Scale);
            var renderPart = (RenderPart)_renderParts[0];
            renderPart.Model.NsbcaAnim.SetFrame(_nsbcaFrame);
            renderPart.Model.Render(this);
        }

        public class RenderPart : RenderPart<Chandelier>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadModel(_context, this, "chandelier.nsbmd");
                MObjUtil.LoadNsbcaAnim(_context, Model, "chandelier.nsbca");
                Model.SetAllPolygonIds(63);
            }

            protected override void Render(Chandelier instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx, alpha);
        }

        public class LogicPart : LogicPart<Chandelier>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                var rand = _context.MObjState.Random;
                foreach (var instance in _instances)
                    instance._counter = rand.Next(60) + 60;
            }

            protected override void Update(Chandelier instance)
                => instance.Update();
        }
    }
}