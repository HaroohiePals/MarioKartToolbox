using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections.MobjSettings;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Enemies
{
    [MapObj(MkdsMapObjectId.MkdEfBurner, new[] { typeof(MkdEfBurner.RenderPart) }, typeof(MkdEfBurner.LogicPart))]
    public class MkdEfBurner : MObjInstance
    {
        private const double SizeY = 140; // from grpconf.tbl

        private enum EfbnrState
        {
            Wait,
            BurnStart,
            Burn,
            BurnStop,
            BurnContinuous
        }

        private int _counter;
        private ushort _nsbtaFrame;
        private double _scale;
        private EfbnrState _state;
        private Pathwalker _pathwalker;

        public MkdEfBurner(MkdsContext context, MapObj.RenderPart[] renderParts, MapObj.LogicPart logicPart)
            : base(context, renderParts, logicPart)
        {
            Size.Y = SizeY;
        }

        private void Update()
        {
            var settings = (MkdEfBurnerSettings)ObjiEntry.Settings;

            switch (_state)
            {
                case EfbnrState.Wait:
                    if (++_counter >= settings.OffTime)
                    {
                        _counter = 0;
                        _state = EfbnrState.BurnStart;
                        Flags &= ~InstanceFlags.Hidden;
                    }

                    break;
                case EfbnrState.BurnStart:
                    _scale += 0.03;
                    if (_scale >= 1)
                    {
                        _scale = 1;
                        _state = EfbnrState.Burn;
                    }

                    Size.Y = SizeY * _scale;
                    break;
                case EfbnrState.Burn:
                    if (++_counter >= settings.OnTime)
                    {
                        _counter = 0;
                        _state = EfbnrState.BurnStop;
                    }

                    break;
                case EfbnrState.BurnStop:
                    _scale -= 0.01;
                    if (_scale <= 0)
                    {
                        _scale = 0;
                        _state = EfbnrState.Wait;
                        Flags |= InstanceFlags.Hidden;
                    }

                    Size.Y = SizeY * _scale;
                    break;
                case EfbnrState.BurnContinuous:
                    break;
            }

            if (++_nsbtaFrame >= ((RenderPart)_renderParts[0]).Model.NsbtaAnim.GetAnimLength())
                _nsbtaFrame = 0;

            if (_pathwalker != null)
            {
                _pathwalker.Update();
                Position = _pathwalker.CalcCurrentPointLinearXYZ();
                Position.Y -= Size.Y;
            }
            else
                Position.Y = ObjiEntry.Position.Y - Size.Y;
        }

        private void Render(in Matrix4x3d camMtx)
        {
            if (_scale <= 0)
                return;

            var renderPart = (RenderPart)_renderParts[0];
            renderPart.Model.SetAllPolygonIds(_context.MObjState.GetCyclicPolygonId());
            _context.RenderContext.GeState.RestoreMatrix(30);
            _context.RenderContext.GeState.MultMatrix(MObjUtil.GetYBillboardAtPos(Position, camMtx));
            _context.RenderContext.GeState.Translate(new(0, Size.Y / 16.0, 0));
            _context.RenderContext.GeState.Scale(new(_scale));
            _context.RenderContext.GeState.Scale(Scale);
            renderPart.Model.NsbtaAnim.SetFrame(_nsbtaFrame);
            renderPart.Model.Render(this);
        }

        public class RenderPart : RenderPart<MkdEfBurner>
        {
            public MObjModel Model { get; private set; }

            public RenderPart(MkdsContext context)
                : base(context, RenderPartType.Normal, false, true) { }

            protected override void GlobalInit()
            {
                Model = MObjUtil.LoadModel(_context, this, "mkd_ef_burner.nsbmd");
                MObjUtil.LoadNsbtaAnim(_context, Model, "mkd_ef_burner.nsbta");
                Model.SetAllPolygonIds(63);
            }

            protected override void Render(MkdEfBurner instance, in Matrix4x3d camMtx, ushort alpha)
                => instance.Render(camMtx);
        }

        public class LogicPart : LogicPart<MkdEfBurner>
        {
            public LogicPart(MkdsContext context)
                : base(context, LogicPartType.Type0) { }

            protected override void GlobalInit()
            {
                foreach (var instance in _instances)
                {
                    try
                    {
                        var settings = (MkdEfBurnerSettings)instance.ObjiEntry.Settings;

                        instance.Position.Y = instance.ObjiEntry.Position.Y - instance.Size.Y;
                        if (instance.ObjiEntry.Path?.Target != null)
                        {
                            instance._pathwalker = Pathwalker.FromPath(instance.ObjiEntry.Path.Target, 0);
                            instance._pathwalker.SetSpeed(settings.PathSpeed / 100.0);
                        }

                        if (settings.BurnContinuously)
                        {
                            instance._scale = 1;
                            instance._state = EfbnrState.BurnContinuous;
                            instance.Flags &= ~InstanceFlags.Hidden;
                        }
                        else
                        {
                            if (++instance._counter >= settings.BrokenInitialDelay)
                            {
                                instance._counter = 0;
                                instance._state = EfbnrState.BurnStart;
                                instance.Flags |= InstanceFlags.Hidden;
                                instance.Size.Y = 0;
                            }
                        }
                    }
                    catch { }
                }
            }

            protected override void Update(MkdEfBurner instance)
                => instance.Update();
        }
    }
}