using HaroohiePals.NitroKart.Course;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Race;

public class RaceContext
{
    private readonly IMkdsCourse _course;
    private readonly ReplayCamera _replayCamera;
    private readonly IntroCamera _introCameraSingle;
    private readonly IntroCameraDouble _introCameraDouble;
    private readonly Driver _driver;

    //Used for intro camera
    private bool _isCamAnimMode = false;
    //Used for replay camera
    private bool _isUncontrollable = true;
    private RaceCamera _curCamera;

    public Matrix4 CurrentCameraView => _curCamera?.View ?? Matrix4.Identity;
    public float CurrentCameraFov => _curCamera?.Fov ?? 45;
    public Matrix4 CurrentCameraProjection => _curCamera?.Projection ?? Matrix4.Identity;

    public RaceContext(IMkdsCourse course, Driver driver, bool isCamAnimMode = false)
    {
        _isCamAnimMode = isCamAnimMode;
        _course = course;
        _driver = driver;

        //Camera init
        _replayCamera = new ReplayCamera(_course, _driver);
        _introCameraDouble = new IntroCameraDouble(_course, driver);
        _introCameraSingle = new IntroCamera(_course, _driver, false, true);
    }

    public void Update(bool isOddFrame, bool isCamAnimSingleScreen)
    {
        UpdateCamera(isOddFrame, isCamAnimSingleScreen);
    }

    private void UpdateCamera(bool isOddFrame, bool isCamAnimSingleScreen)
    {
        if (_isCamAnimMode)
        {
            if (isCamAnimSingleScreen)
            {
                _introCameraSingle.Update();
                _curCamera = _introCameraSingle;
            }
            else
            {
                _curCamera = _introCameraDouble.Update(isOddFrame);
            }
        }
        else if (_isUncontrollable)
        {
            //cam_updatePlayerCamera(sPlayerCamera, driver_getById(rconf_getCpuDriverId()));
            //cam_updateLookAtEx(sPlayerCamera);
            if (_replayCamera.Update())
            {
                _curCamera = _replayCamera;
            }
            //else
            //{
            //    sCurCamera = sPlayerCamera;
            //    sCurCameraMtx = &sPlayerCamera->mtx;
            //}
        }
        _curCamera?.UpdateFrustum();
    }
}
