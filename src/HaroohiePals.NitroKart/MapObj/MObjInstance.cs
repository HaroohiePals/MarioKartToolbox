using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.MapObj
{
    public abstract class MObjInstance
    {
        [Flags]
        public enum InstanceFlags : ushort
        {
            Hidden                   = 1 << 0,
            DisableVisibilityUpdates = 1 << 1,
            Suspended                = 1 << 2,
            Bit3                     = 1 << 3,
            Clipped                  = 1 << 4,
            MapIconTranslucent       = 1 << 5,
            DisablePushback          = 1 << 6,
            DisabledBeforeStart      = 1 << 7,
            Free                     = 1 << 8,
            Bit9                     = 1 << 9,
            MapIconMirror            = 1 << 10,
            UseSimpleHitResponse     = 1 << 11,

            PrevHidden                   = 1 << 12,
            PrevDisableVisibilityUpdates = 1 << 13,
            PrevSuspended                = 1 << 14,
            PrevBit3                     = 1 << 15
        }

        protected readonly MkdsContext  _context;
        protected readonly RenderPart[] _renderParts;
        protected readonly LogicPart    _logicPart;

        public MkdsMapObjectId   ObjectId;
        public InstanceFlags Flags;
        public Vector3d      Position;
        public Vector3d      Velocity;
        public Vector3d      Scale;
        public Matrix4x3d    Mtx;
        public Vector3d      Size;
        public ushort        Alpha;
        public ushort        RotY;
        public MkdsMapObject     ObjiEntry;

        public MObjInstance(MkdsContext context, RenderPart[] renderParts, LogicPart logicPart)
        {
            _context     = context;
            _renderParts = renderParts;
            _logicPart   = logicPart;
        }

        public virtual void Init(MkdsMapObject obji, object arg) { }

        public void SetInvisible()
        {
            Flags &= ~(InstanceFlags.PrevHidden | InstanceFlags.PrevBit3 | InstanceFlags.PrevDisableVisibilityUpdates |
                       InstanceFlags.PrevSuspended);
            Flags |= (InstanceFlags)((int)(Flags & (InstanceFlags.Hidden | InstanceFlags.DisableVisibilityUpdates |
                                                    InstanceFlags.Suspended | InstanceFlags.Bit3)) << 12);
            Flags |= InstanceFlags.Hidden;
            Flags |= InstanceFlags.DisableVisibilityUpdates;
            Flags |= InstanceFlags.Bit3;
            Flags |= InstanceFlags.Suspended;
            Flags |= InstanceFlags.Free;
        }

        public void SetVisible()
        {
            Flags &= ~(InstanceFlags.Hidden | InstanceFlags.DisableVisibilityUpdates |
                       InstanceFlags.Suspended | InstanceFlags.Bit3);
            Flags |= (InstanceFlags)((int)(Flags &
                                           (InstanceFlags.PrevHidden | InstanceFlags.PrevBit3 |
                                            InstanceFlags.PrevDisableVisibilityUpdates |
                                            InstanceFlags.PrevSuspended)) >> 12);
            if ((Flags & InstanceFlags.DisableVisibilityUpdates) == 0)
                Alpha = 31;
            Flags &= ~InstanceFlags.Free;
        }
    }
}