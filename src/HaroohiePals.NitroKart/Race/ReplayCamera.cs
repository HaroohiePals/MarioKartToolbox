using HaroohiePals.NitroKart.Course;
using HaroohiePals.NitroKart.MapData;

namespace HaroohiePals.NitroKart.Race;

internal class ReplayCamera : RaceCamera
{
    //cam_20777AC
    public ReplayCamera(IMkdsCourse course, Driver driver) : base(course, driver)
    {
        //_targetDriverId = driverId;
        CameEntry = null;
        Mode = CameraMode.Normal;
        _field250 = 1;
    }

    //cam_updateReplayCamera
    public bool Update()
    {
        bool result = true;

        if (_field230 > 0 || _field234 > 0)
            return false;

        _prevPosition = Position;

        if (_field238)
        {
            UpdateFollowDriver();
            return true;
        }

        var driverPosition = _targetDriver.Position;
        var cameEntry = MkdsMapDataUtil.FindCamera(_course.MapData, driverPosition);
        if (_field228)
        {
            //if (rconf_getRaceMode() == RACE_MODE_MG)
            //{
            //    updateCame(cam);
            //    return TRUE;
            //}
            if (++_field22C >= 300)
                _field228 = false;
        }

        if (cameEntry is null)
            return false;
        
        if (cameEntry != _currentCam)
        {
            _currentCam = cameEntry;
            if (!_field228)
                InitFromCame(cameEntry);
        }
        UpdateCame();

        return result;
    }

}
