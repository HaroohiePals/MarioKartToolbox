using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData;
using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using HaroohiePals.NitroKart.MapObj;
using OpenTK.Mathematics;
using System;

namespace HaroohiePals.NitroKart.Race;

//camera.c
internal abstract class RaceCamera
{
    protected readonly IMkdsCourse _course;

    //camera_vecs.c
    private Vector3d cam_sVecRight = (1, 0, 0);
    private Vector3d cam_sVecUp = (0, 1, 0);
    private Vector3d cam_sVecForward = (0, 0, 1);
    private Vector3d cam_sVecNull = (0, 0, 0);

    private readonly CameraParams[] _camParams = new CameraParams[2]
    {
        new CameraParams(58, 34, 20),
        new CameraParams(98, 55, 34)
    };

    private bool _isMirror = false;
    private ushort _defaultFov = 0x157C;

    protected double _cameraDistance;
    protected double _cameraElevation;
    protected double _maxTargetElevation;
    protected double _cannonShakeAmount;

    //camera.h -> camera_t
    protected Vector3d _up;
    protected Vector3d _right;
    internal Vector3d Target;
    internal Vector3d Position;
    public Matrix4 View { get; protected set; }
    public Matrix4 Projection { get; protected set; }

    public float Fov => (float)MathHelper.RadiansToDegrees(Math.Atan2(FovSin, FovCos));

    //protected int _fov;
    protected int _targetFov;
    internal double FovSin;
    internal double FovCos;
    protected double _aspectRatio;
    protected double _frustumNear;
    protected double _frustumFar;
    protected double _frustumTop;
    protected double _frustumBottom;
    protected double _frustumLeft;
    protected double _frustumRight;
    protected double _field88;
    protected double _skyFrustumFar;
    protected Vector3d _lookAtTarget;
    protected Vector3d _lookAtPosition;
    protected Vector3d _fieldA8;
    protected Vector3d _fieldB4;
    protected Vector3d _upConst;
    protected double _fieldCC;
    protected bool _fieldD0;
    protected double _targetElevation;
    protected uint _fieldD8;
    protected double _fieldDC;
    protected double _fieldE0;
    protected Vector3d _fieldE4;
    protected double _playerOffsetDirection;
    protected Vector3d _fieldF4;
    protected Vector3d _field100;
    protected Vector3d _field10C;
    protected Vector3d _field118;
    protected Vector3d _field124;
    protected byte _field130;

    protected Vector3d _prevPosition;
    protected bool _isShaking;
    protected double _field144;
    protected double _shakeAmount;
    protected uint _field14C;
    protected short _field150;

    protected double _shakeDecay;
    protected double _field158;
    protected Vector3d _targetDirection;
    protected double _field168;
    protected uint _field16C;
    protected uint _field170;
    protected uint _field174;
    protected double _elevation;
    protected Vector3d _field17C;
    protected Vector3d _field188;
    protected CameraRoute _routeStat;
    protected CameraRoute _routeStat2;
    protected ushort _field20C;
    protected ushort _field20E;
    //private int _targetDriverId;
    protected Driver _targetDriver;

    protected MkdsCamera _currentCam;
    public MkdsCamera CameEntry { get; protected set; }
    //came_unknown_t* unknownMgCams;
    //came_unknown_t* unknownMgCamsCopy;
    protected ushort _field224;

    protected bool _field228;
    protected ushort _field22C;

    protected uint _field230;
    protected uint _field234;
    protected bool _field238;
    protected ushort _frameCounter;

    protected double _fovProgress;
    protected double _targetProgress;
    protected uint _field248;
    public CameraMode Mode { get; protected set; }
    protected uint _field250;
    protected uint _field254;
    protected bool _field258;
    protected uint _field25C;
    protected Vector3d _field260;
    protected short _field26C;
    protected ushort _field26E; // idk if this is a used field or just padding

    public RaceCamera(IMkdsCourse course, Driver driver)
    {
        _course = course;
        _targetDriver = driver;

        SetupRaceParams();
        Init();
    }

    private void Init()
    {
        Position.X = 483;
        Position.Y = 1213;
        Position.Z = 3179;
        Target = cam_sVecNull;
        _up = cam_sVecUp;
        _right = cam_sVecRight;
        View = Matrix4.Identity;
        _targetDirection = cam_sVecForward;
        _field168 = 0;
        _targetFov = _defaultFov;
        ushort fov = (ushort)(_defaultFov + 5);
        FovSin = MObjUtil.SinIdx(fov);
        FovCos = MObjUtil.CosIdx(fov);
        _aspectRatio = 1.3333;
        _frustumNear = 0.25 * 16f;
        _skyFrustumFar = 1600 * 16f;
        _frustumFar = _course.MapData.StageInfo?.FrustumFar ?? 0f * 16f;
        if (_frustumFar < 1.0 * 16f)
            _frustumFar = 1600 * 16f;
        _frustumTop = 0;
        _frustumBottom = 0;
        _frustumLeft = 0;
        _frustumRight = 0;
        _lookAtPosition = cam_sVecNull;
        _lookAtTarget = cam_sVecNull;
        _upConst = cam_sVecUp;
        _fieldCC = 0;
        _targetElevation = _maxTargetElevation;
        _fieldD8 = 1;
        _fieldE0 = _fieldDC = _cameraDistance;
        _fieldE4.X = 0;
        _fieldE4.Y = 1.0;
        _fieldE4.Z = 0;
        _playerOffsetDirection = 1.0;
        _fieldF4 = cam_sVecNull;
        _field100 = cam_sVecNull;
        _field10C = cam_sVecNull;
        _field118 = cam_sVecNull;
        _field130 = 0;
        _field124 = cam_sVecNull;
        _fovProgress = 0;
        _targetProgress = 0;
        _field248 = 0;
        _field16C = 0;
        _field170 = 0;
        _field174 = 0;
        _elevation = 0;
        _isShaking = false;
        _field144 = 0;
        _field14C = 0;
        _field150 = 1;
        _shakeDecay = 0;
        _field158 = 0;
        _shakeAmount = 0;
        //_targetDriverId = 0;
        //_currentCamId = -1;
        _frameCounter = 0;
        _field228 = false;
        _field22C = 0;
        _field230 = 0;
        _field234 = 0;
        _field238 = false;
        Mode = CameraMode.Normal;
        _field250 = 0;
        _field188 = cam_sVecNull;
        _field17C = cam_sVecNull;
        _field254 = 3;
        _field258 = false;
        _field25C = 0;
        _field260 = cam_sVecNull;
        _fieldA8 = cam_sVecNull;
        _fieldB4 = cam_sVecNull;
        _field26C = (short)(_isMirror ? -1 : 1);
        //_unknownMgCams = 0;
        //_unknownMgCamsCopy = 0;
        _field20C = 0;
        _field20E = 0;
        _field224 = 0;
        _field88 = 1.0;
    }

    private void SetupRaceParams()
    {
        //dword_217ACA8 = TRUE;
        //dword_217ACAC = 0;
        int paramsIdx = 0;
        //if (rconf_getRaceMode() == RACE_MODE_MR)
        //    paramsIdx = rconf_getMrCamParamsIdx();
        _cameraDistance = _camParams[paramsIdx].Distance;
        _cameraElevation = _camParams[paramsIdx].Elevation;
        _maxTargetElevation = _camParams[paramsIdx].MaxTargetElevation;

        if (_course.MapData.StageInfo?.CourseId == 36 /*COURSE_AIRSHIP_COURSE*/)
            _cannonShakeAmount = 10;
        else
            _cannonShakeAmount = 5;
    }

    protected void InitFromCame(MkdsCamera came)
    {
        CameEntry = came;
        _fovProgress = 0;
        FovSin = MathF.Sin(MathHelper.DegreesToRadians(came.FovBegin));
        FovCos = MathF.Cos(MathHelper.DegreesToRadians(came.FovBegin));
        _field130 = came.ShowAwardTrail;
        if (came.Path?.Target is not null)
        {
            _routeStat = new CameraRoute(came, false);
            //camr_initRouteStat(&_routeStat, came);
            //if (rconf_getRaceMode() == RACE_MODE_MG)
            //    camr_initRouteStatReversed(&_routeStat2, came);
        }
        //if (rstat_hasDriverFinished(driver_getById(gRaceMultiConfig->current.playerDriverId)->id))
        //    MATH_Rand32(&gRaceStatus->randomRng, 0);
        //dword_217ACA8 = 1;
    }

    private void UpdateFov()
    {
        RaceCameraUtils.CalculateFovSinCos(CameEntry.FovBegin, CameEntry.FovEnd,
            _fovProgress, CameEntry.FovSpeed,
            out FovSin, out FovCos);

        if (CameEntry.FovSpeed > 0)
        {
            _fovProgress += CameEntry.FovSpeed;
            if (_fovProgress > 1)
                _fovProgress = 1;
        }
    }

    private void UpdateLookAt()
    {
        Vector3d right, direction, up, camPos;

        up = cam_sVecUp;
        direction = (Target - Position).Normalized();
        _targetDirection = direction;

        right = Vector3d.Cross(up, direction);
        _up = Vector3d.Cross(direction, right);

        if (_up.X == 0 && _up.Y == 0 && _up.Z == 0)
        {
            _up = Vector3d.One;
            //_up.Normalize();
        }

        //G3_MtxMode(GX_MTXMODE_POSITION_VECTOR);

        camPos.X = Position.X;
        camPos.Y = Position.Y + _elevation;
        camPos.Z = Position.Z;

        //todo
        //vec_toRenderSpace(&_target, &target);
        //G3_LookAt(&camPos, &_up, &target, &_mtx);
        View = Matrix4.LookAt((Vector3)camPos, (Vector3)Target, (Vector3)_up);
    }

    public void UpdateLookAtEx()
    {
        Vector3d direction, up, newPos, newTarget;

        up = _upConst;
        newPos = Position;
        newTarget = Target;
        newTarget.Y += _field158;

        direction = (newTarget - newPos).Normalized();
        _targetDirection = direction;

        _right = Vector3d.Cross(up, direction);
        _up = Vector3d.Cross(direction, _right);

        if (_up.X == 0 && _up.Y == 0 && _up.Z == 0)
        {
            _up = Vector3d.One;
            //_up.Normalize();
        }

        //show award trail stuff
        //if (_field130 > 0)
        //{
        //    cam_2073904(camera);
        //    VEC_Add(&newPos, &_field118, &newPos);
        //    VEC_Add(&newTarget, &_field10C, &newTarget);
        //    VEC_Add(&_field124, &newTarget, &newTarget);
        //}

        //vec_toRenderSpace(&newPos, &_lookAtPosition);
        _lookAtPosition = newPos;
        _lookAtPosition.Y += _elevation;
        _lookAtTarget = newTarget;

        View = Matrix4.LookAt((Vector3)_lookAtPosition, (Vector3)_lookAtTarget, (Vector3)_up);
    }

    private void UpdateDriverTarget()
    {
        //if (rconf_getDisplayMode() == RACE_DISPLAY_MODE_AWARD)
        //{
        //    _targetDriverId = FX_Whole(_cameEntry.target2.z) - 1;
        //    driver_getPosition(&_target, _targetDriverId);
        //    _target.y += _cameEntry.target1.y;
        //}
        //else
        //{
        if (_targetDriver != null)
        {
            Target = _targetDriver.Position;
            Target.Y += 6;
        }
        //}
    }

    private void UpdateCameTarget()
    {
        Target = RaceCameraUtils.CalculateTarget(CameEntry.Target1, CameEntry.Target2, _targetProgress, CameEntry.TargetSpeed);

        if (CameEntry.TargetSpeed > 0)
        {
            _targetProgress += CameEntry.TargetSpeed;
            if (_targetProgress > 1)
                _targetProgress = 1;
        }
    }

    private void UpdateCameRoutePos(double speed)
    {
        var path = CameEntry.Path?.Target;

        if (path is null)
            return;

        if (path.Points.Count >= 4)
            Position = _routeStat.Update(CameEntry, speed);
        else if (path.Points.Count >= 2)
        {
            var point0 = path.Points[0];
            var point1 = path.Points[1];
            var p0 = point0.Position;
            var direction = point1.Position - point0.Position;

            double distance = direction.Length;
            direction.Normalize();
            _routeStat.Progress += speed;
            if (_routeStat.Progress >= 1)
                _routeStat.Progress = 1;
            var offset = direction * (_routeStat.Progress * distance);
            Position = p0 + offset;
        }
    }

    private void UpdateCameByRoute()
    {
        UpdateCameRoutePos(CameEntry.PathSpeed);
        UpdateCameTarget();
        UpdateLookAt();
    }

    private void UpdateCameByRouteAndDriver()
    {
        UpdateCameRoutePos(CameEntry.PathSpeed);
        UpdateDriverTarget();
        UpdateLookAtEx();
    }

    //cam_2074E6C
    protected void UpdateFollowDriver()
    {
        MkdsCamera cameEntry;
        Driver driver;
        Vector3d v14;
        Vector3d a2;
        Vector3d v17;
        Vector3d v18;
        Matrix3d v19;
        Matrix3d v20;
        Matrix3d v21;
        Matrix3d result;
        Matrix3d v23;
        Matrix3d v24;
        Matrix3d v25;
        Vector3d a3;
        Vector3d axis;
        Vector3d a1;
        Vector3d v29;
        Vector3d pushback;
        Vector3d v31;

        cameEntry = CameEntry;
        UpdateDriverTarget();
        driver = _targetDriver;
        a2 = driver.MainMtx.Row2; //*(VecFx32*)&driver->mainMtx.m[2];

        double target1angleX = MathHelper.DegreesToRadians(cameEntry.Target1.X);
        double target1angleY = MathHelper.DegreesToRadians(cameEntry.Target1.Y);
        double target2angleX = MathHelper.DegreesToRadians(cameEntry.Target2.X);
        double target2angleY = MathHelper.DegreesToRadians(cameEntry.Target2.Y);

        result = Matrix3d.CreateFromAxisAngle(driver.MainMtx.Row0 , MathHelper.DegreesToRadians(driver.Field2C0)); //MTX_RotAxis33(&result, (Vector3d*)driver->mainMtx.m[0], FX_SinIdx(driver->field2C0), FX_CosIdx(driver->field2C0));
        a2 = a2 * result; //MTX_MultVec33(&a2, &result, &a2);
        v31 = a2;
        a1 = a2;
        v17 = a2;

        v29 = v17 * -cameEntry.Target1.Z;
        a3 = Vector3d.Cross(cam_sVecUp, a2);
        axis = Vector3d.Cross(a1, a3);

        v20 = Matrix3d.CreateFromAxisAngle(axis, target1angleX);
        v19 = Matrix3d.CreateFromAxisAngle(axis, target1angleY);
        v21 = v19 * v20; //MTX_Concat33(&v19, &v20, &v21);
        v17 = v17 * v21; // MTX_MultVec33(&v17, &v21, &v17);
        v18 = v17 * cameEntry.Target1.Z;  //vec_mulScalar(&v17, cameEntry.target1.z, &v18);
        Position = Target + v18; //VEC_Add(&_target, &v18, &_position);
        a3.X = -a3.X;
        a3.Y = -a3.Y;
        a3.Z = -a3.Z;
        axis.X = -axis.X;
        axis.Y = -axis.Y;
        axis.Z = -axis.Z;
        v24 = Matrix3d.CreateFromAxisAngle(axis, target2angleX); //MTX_RotAxis33(&v24, &axis, FX_SinIdx(v9), FX_CosIdx(v9));
        v23 = Matrix3d.CreateFromAxisAngle(a3, target2angleY); //MTX_RotAxis33(&v23, &a3, FX_SinIdx(v10), FX_CosIdx(v10));
        v24 = v24 * v20; // MTX_Concat33(&v24, &v20, &v24);
        v23 = v23 * v19; // MTX_Concat33(&v23, &v19, &v23);
        v25 = v23 * v24; // MTX_Concat33(&v23, &v24, &v25);
        v29 = v29 * v25; // MTX_MultVec33(&v29, &v25, &v29);
        Target = Position + v29; //VEC_Add(&_position, &v29, &_target);
        Target.Y -= driver.Field2BC;
        Position.Y -= driver.Field2BC;
        //if (rconf_getRaceMode() != RACE_MODE_MR
        //    && rconf_getRaceMode() != RACE_MODE_MG
        //    && col_collide(&_position, &_prevPosition, NULL, sColSphereSize >> 1, COL_COLLIDE_FLAGS_IS_CAMERA,
        //                   -2, &pushback, NULL, NULL, NULL, NULL, NULL, NULL, NULL))
        //{
        //    v14 = (Vector3d*)driver_getById((ushort)_targetDriverId)->mainMtx.m[0];
        //    vec_mulScalar(v14, VEC_DotProduct(v14, &pushback), &pushback);
        //    VEC_Add(&pushback, &_position, &_position);
        //}
        UpdateLookAtEx();
    }

    protected void UpdateCame()
    {
        switch (CameEntry.Type)
        {
            case MkdsCameraType.FixedLookAtDriver:
                Position = CameEntry.Position;
                UpdateDriverTarget();
                UpdateLookAtEx();
                break;
            case MkdsCameraType.RouteLookAtDriver:
                UpdateCameByRouteAndDriver();
                break;
            case MkdsCameraType.FollowDriverA:
            case MkdsCameraType.FollowDriverB:
                UpdateFollowDriver();
                break;
            case MkdsCameraType.FixedLookAtTargets:
                Position = CameEntry.Position;
                UpdateCameTarget();
                UpdateLookAt();
                break;
            case MkdsCameraType.RouteLookAtTargets:
                UpdateCameByRoute();
                break;
        }
        UpdateFov();
    }

    public void UpdateFrustum(bool isMirror = false)
    {
        Projection = RaceCameraUtils.CalculateProjection(
            Mode, FovSin, FovCos, 
            _frustumNear, _skyFrustumFar, _aspectRatio, isMirror);
    }
}
