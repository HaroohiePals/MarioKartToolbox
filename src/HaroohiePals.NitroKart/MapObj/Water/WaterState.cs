using HaroohiePals.Graphics;
using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Model;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj.Water
{
    public class WaterState
    {
        public enum TideState
        {
            State0,
            State1,
            State2,
            State3
        }

        private readonly MkdsContext _context;

        public Vector3d   WaterAPosition;
        public Vector3d   WaterCPosition;
        public Vector3d   BasePosition;
        public TideState  State;
        public double     TideAmplitude;
        public ushort     TidePhase;
        public ushort     TideSpeed;
        public double     TideProgress;
        public ushort     Field34;
        public ushort     Field36;
        public ushort     Field38;
        public ushort     Field3A;
        public ushort     WaterMovePhase;
        public ushort     Field3E;
        public ushort     WaterMoveSpeed;
        public double     WaterAMoveDistance;
        public double     WaterCMoveDistance;
        public double     Field4C;
        public ushort     WaterCMovePhaseDifference;
        public ushort     Field52;
        public bool       WaterAFirst;
        public ushort     WaterAAlpha;
        public ushort     WaterCAlpha;
        public ushort     Field5C;
        public Nsbmd      WaterANsbmd;
        public Nsbmd      WaterCNsbmd;
        public Model      WaterAModel;
        public Model      WaterCModel;
        public Matrix4x3d TransformMtx;
        public bool       IsDiveable;

        public WaterState(MkdsContext context, double upTide, double downTide, double waterAMoveDistance,
            double waterCMoveDistance, double a5, ushort waterCMovePhaseDifference, ushort a7, ushort tideSpeed,
            ushort a9, ushort a10, bool waterAFirst, ushort waterAAlpha, ushort waterCAlpha, ushort a14,
            bool isDiveable)
        {
            _context = context;

            BasePosition.Y            = (upTide + downTide) / 2;
            WaterAPosition.Y          = BasePosition.Y;
            WaterCPosition.Y          = WaterAPosition.Y;
            TideAmplitude             = (upTide - downTide) / 2;
            TidePhase                 = 0;
            Field3A                   = 0;
            WaterMovePhase            = 0;
            WaterCMovePhaseDifference = waterCMovePhaseDifference;
            Field52                   = a7;
            TideSpeed                 = (ushort)(0x10000 / tideSpeed);
            WaterMoveSpeed            = (ushort)(TideSpeed / 4);
            State                     = 0;
            Field34                   = a9;
            Field36                   = a10;
            TideProgress              = 0;
            BasePosition.X            = context.MObjState.WaterObject.Position.X;
            WaterAPosition.X          = BasePosition.X;
            BasePosition.Z            = context.MObjState.WaterObject.Position.Z;
            WaterAPosition.Z          = BasePosition.Z;
            WaterAMoveDistance        = waterAMoveDistance;
            WaterCMoveDistance        = waterCMoveDistance;
            Field4C                   = a5;
            WaterAFirst               = waterAFirst;
            WaterAAlpha               = waterAAlpha;
            WaterCAlpha               = waterCAlpha;
            Field5C                   = a14;
            TransformMtx              = Matrix4x3d.CreateScale(1);
            IsDiveable                = isDiveable;
        }

        public void InitModels()
        {
            if (WaterANsbmd != null)
            {
                WaterAModel = new Model(_context, WaterANsbmd);
                WaterAModel.RenderObj.ModelResource.SetAllEmission(new Rgb555(10, 10, 10));
                WaterAModel.RenderObj.ModelResource.SetAllPolygonId(1);
            }

            WaterCModel = new Model(_context, WaterCNsbmd);
            WaterCModel.RenderObj.ModelResource.SetAllEmission(new Rgb555(10, 10, 10));
            WaterCModel.RenderObj.ModelResource.SetAllPolygonId(2);

            if (WaterANsbmd != null)
                WaterAModel.RenderObj.ModelResource.SetAllAlpha((byte)WaterAAlpha);
            WaterCModel.RenderObj.ModelResource.Materials
                .Materials[WaterCModel.RenderObj.ModelResource.Materials.MaterialDictionary.IndexOf("waterC")].SetAlpha((byte)WaterCAlpha);
        }

        public void Update()
        {
            // fx32                   volume;
            // NNSSndHandle*          sndHandle;
            ushort waterAPhase;
            ushort waterCPhase;
            // snd_unkstruct_field0_t sndUnkDown;
            // snd_unkstruct_field0_t sndUnkUp;

            // sndHandle = sub_2108E6C(SND_HANDLE_10);
            // if (IsDiveable)
            // {
            //     volume = gRaceMultiConfig->current.course == COURSE_BEACH_COURSE ? bsfx_getVolume() : FX32_CONST(127);
            //     if (volume >= 0)
            //         sub_2105B28(sndHandle, volume);
            // }
            switch (State)
            {
                case TideState.State0:
                    TideProgress     =  MObjUtil.SinIdx(TidePhase);
                    WaterAPosition.Y =  BasePosition.Y + TideAmplitude * TideProgress;
                    WaterCPosition.Y =  WaterAPosition.Y;
                    TidePhase        += TideSpeed;
                    Field3A          += TideSpeed;
                    if (TidePhase > 0x3FFF && TidePhase < 0x7FFF)
                    {
                        State   = TideState.State1;
                        Field38 = 0;
                        Field3E = TideSpeed;
                    }

                    break;
                case TideState.State1:
                    Field38++;
                    if (Field38 > Field34)
                    {
                        State = TideState.State2;
                        // if (IsDiveable && volume >= 0)
                        // {
                        //     sndUnkDown.sfxId = SET_NAMI_DOWN_SU;
                        //     sndUnkDown.position = NULL;
                        //     sndUnkDown.sfxParamsId = 4;
                        //     sndUnkDown.distance = mobj_calcXZCamDist(NULL);
                        //     sub_210B760(&sndUnkDown, sndHandle);
                        // }
                    }
                    else if (Field38 > Field34 >> 1)
                    {
                        Field3E++;
                        Field3A += Field3E;
                    }
                    else
                    {
                        Field3E--;
                        Field3A += Field3E;
                    }

                    break;
                case TideState.State2:
                    TideProgress     =  MObjUtil.SinIdx(TidePhase);
                    WaterAPosition.Y =  BasePosition.Y + TideAmplitude * TideProgress;
                    WaterCPosition.Y =  WaterAPosition.Y;
                    TidePhase        += TideSpeed;
                    Field3A          += TideSpeed;
                    if (TidePhase > 0xBFFF)
                    {
                        State   = TideState.State3;
                        Field38 = 0;
                        Field3E = TideSpeed;
                    }

                    break;
                case TideState.State3:
                    Field38++;
                    if (Field38 > Field36)
                    {
                        State = TideState.State0;
                        // if (isDiveable && volume >= 0)
                        // {
                        //     sndUnkUp.sfxId = SET_NAMI_UP_SU;
                        //     sndUnkUp.position = NULL;
                        //     sndUnkUp.sfxParamsId = 4;
                        //     sndUnkUp.distance = mobj_calcXZCamDist(NULL);
                        //     sub_210B760(&sndUnkUp, sndHandle);
                        // }
                    }
                    else if (Field38 > Field36 >> 1)
                    {
                        Field3E++;
                        Field3A += Field3E;
                    }
                    else
                    {
                        Field3E--;
                        Field3A += Field3E;
                    }

                    break;
            }

            WaterMovePhase += WaterMoveSpeed;
            waterAPhase    =  WaterMovePhase;
            waterCPhase    =  (ushort)(waterAPhase - WaterCMovePhaseDifference);
            if (WaterANsbmd != null)
            {
                WaterAPosition.X
                    = BasePosition.X + WaterAMoveDistance * MObjUtil.SinIdx(waterAPhase);
                WaterAPosition.Z
                    = BasePosition.Z + WaterAMoveDistance * MObjUtil.CosIdx(waterAPhase);
            }

            WaterCPosition.X = BasePosition.X + WaterCMoveDistance * MObjUtil.SinIdx(waterCPhase);
            WaterCPosition.Z = BasePosition.Z + WaterCMoveDistance * MObjUtil.CosIdx(waterCPhase);
        }

        public void Render()
        {
            // if (gWaterObject->mobj.flags & (MOBJ_INST_FLAGS_CLIPPED | MOBJ_INST_FLAGS_DISABLE_VISIBILITY_UPDATES))
            // return;

            if (WaterAFirst)
            {
                if (WaterANsbmd != null)
                {
                    TransformMtx.Row3 = WaterAPosition / 16.0;
                    WaterAModel.Render(TransformMtx);
                }

                TransformMtx.Row3 = WaterCPosition / 16.0;
                WaterCModel.Render(TransformMtx);
            }
            else
            {
                TransformMtx.Row3 = WaterCPosition / 16.0;
                WaterCModel.Render(TransformMtx);
                if (WaterANsbmd != null)
                {
                    TransformMtx.Row3 = WaterAPosition / 16.0;
                    WaterAModel.Render(TransformMtx);
                }
            }
        }
    }
}