using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData;
using System.Linq;

namespace HaroohiePals.NitroKart.Race;

internal class IntroCamera : RaceCamera
{
    private readonly bool _isBottom = false; 
    private readonly bool _isSingle = false;

    public IntroCamera(IMkdsCourse course, Driver driver, bool isBottom = false, bool isSingle = false) : base(course, driver)
    {
        _isBottom = isBottom;
        _isSingle = isSingle;

        if (!_isSingle)
            SetupIntroCamDouble(course);
        else
            SetupIntroCamSingle(course);
    }


    //cam_setupIntroCamDouble
    private void SetupIntroCamDouble(IMkdsCourse course)
    {
        CameEntry = _isBottom ?
            course.MapData.Cameras.FirstOrDefault(x => x.FirstIntroCamera == MkdsCameIntroCamera.Bottom) :
            course.MapData.Cameras.FirstOrDefault(x => x.FirstIntroCamera == MkdsCameIntroCamera.Top);

        Mode = _isBottom ? CameraMode.DoubleBottom : CameraMode.DoubleTop;

        _field250 = 2;

        if (CameEntry != null && CameEntry.Path?.Target != null)
            _routeStat = new CameraRoute(CameEntry);
    }

    //cam_setupIntroCamSingle
    private void SetupIntroCamSingle(IMkdsCourse course)
    {
        CameEntry = course.MapData.Cameras.FirstOrDefault(x => x.FirstIntroCamera == MkdsCameIntroCamera.Top);

        Mode = CameraMode.Normal;

        _field250 = 2;

        if (CameEntry != null && CameEntry.Path?.Target != null)
            _routeStat = new CameraRoute(CameEntry);
    }

    //updateIntroCam
    public void Update(IntroCamera bottomCamera = null)
    {
        if (CameEntry == null)
            return;
        UpdateCame();

        if (++_frameCounter != CameEntry.Duration)
            return;

        SetupNextCamera();

        if (CameEntry != null && !_isBottom && Mode == CameraMode.DoubleTop && bottomCamera != null)
        {
            bottomCamera.SetupNextCamera();
        }
    }

    private void SetupNextCamera()
    {
        var entry = CameEntry?.NextCamera?.Target;

        //if (entry == null)
        //    gRaceStatus.camAnimComplete = TRUE;
        //else
        if (entry != null)
        {
            _frameCounter = 0;
            CameEntry = entry;
            _targetProgress = 0;
            _fovProgress = 0;
            if (entry.Path?.Target != null)
                _routeStat = new CameraRoute(entry);
            //if (rconf_getDisplayMode() == RACE_DISPLAY_MODE_AWARD)
            //    trl_setVisibleForAward(entry.unknown != 0);

            //dword_217ACA8 = TRUE;
        }
    }
}
