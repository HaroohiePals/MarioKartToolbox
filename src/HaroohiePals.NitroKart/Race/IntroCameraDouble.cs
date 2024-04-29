using HaroohiePals.NitroKart.Course;

namespace HaroohiePals.NitroKart.Race;

internal class IntroCameraDouble
{
    public readonly IntroCamera _introCameraTop;
    public readonly IntroCamera _introCameraBottom;

    public IntroCameraDouble(IMkdsCourse course, Driver driver)
    {
        _introCameraTop = new IntroCamera(course, driver);
        _introCameraBottom = new IntroCamera(course, driver, true);
    }

    //cam_updateIntroCamDouble
    public IntroCamera Update(bool isOddFrame)
    {
        //if (_introCameraBottom.CameEntry != null)
        //{
        //    if (_introCameraBottom.CameEntry.Type == MapData.Binary.Came.CameraType.IntroBottom)
        //    {
        //        _introCameraTop.Mode = CameraMode.DoubleTop;
        //        _introCameraBottom.Mode = CameraMode.DoubleBottom;
        //    }
        //    else
        //    {
        //        _introCameraBottom.Mode = CameraMode.Normal;
        //        _introCameraTop.Mode = _introCameraBottom.Mode;
        //    }
        //}
        _introCameraTop.Update(_introCameraBottom);
        if (_introCameraBottom.Mode == CameraMode.DoubleBottom)
        {
            _introCameraBottom.FovSin = _introCameraTop.FovSin;
            _introCameraBottom.FovCos = _introCameraTop.FovCos;
            _introCameraBottom.Position = _introCameraTop.Position;
            _introCameraBottom.Target = _introCameraTop.Target;
        }
        else
            _introCameraBottom.Update(); //Why is this done?

        if (!isOddFrame)
        {
            _introCameraTop.UpdateLookAtEx();
            return _introCameraTop;
        }
        else
        {
            _introCameraBottom.UpdateLookAtEx();
            return _introCameraBottom;
        }
    }
}
