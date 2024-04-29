using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.MapObj
{
    public abstract class DColMObjInstance : MObjInstance
    {
        public enum DColShape
        {
            Box,
            Cylinder
        }

        protected Matrix3d  _lastMtx;
        protected Matrix3d  _baseMtx;
        protected Vector3d  _lastPosition;
        protected Vector3d  _basePos;
        protected Vector3d  _size;
        protected double    _sizeZ2;
        protected bool      _isFloorYZ;
        protected bool      _isFloorXZ;
        protected bool      _isFloorXY;
        protected bool      _isBoostPanel;
        protected double    _floorThreshold;
        protected Vector3d  _field124;
        protected uint      _field130;
        protected DColShape _shape;
        protected uint      _field138;
        protected uint      _field13C;
        protected Model     _model;

        protected DColMObjInstance(MkdsContext context, RenderPart[] renderParts, LogicPart logicPart)
            : base(context, renderParts, logicPart) { }
    }
}