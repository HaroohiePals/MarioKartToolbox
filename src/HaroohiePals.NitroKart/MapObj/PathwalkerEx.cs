using HaroohiePals.NitroKart.MapData.Intermediate.Sections;

namespace HaroohiePals.NitroKart.MapObj;

sealed class PathwalkerEx : Pathwalker
{
    private double _speedDiv100;
    private double _defaultSpeedDiv100;
    private double _prevSpeedDiv100;
    private double _field30;
    private double _curSpeedDiv100;

    public override void Init(int initialPoint, bool forwards)
    {
        double speedDiv100;
        double curSpeedDiv100;

        base.Init(initialPoint, forwards);

        if (PrevPoit.Duration != 0)
        {
            speedDiv100 = _speedDiv100 * PrevPoit.Duration;
        }
        else
        {
            speedDiv100 = _defaultSpeedDiv100;
        }

        _prevSpeedDiv100 = speedDiv100;
        _field30 = _prevSpeedDiv100;

        if (CurPoit.Duration != 0)
        {
            curSpeedDiv100 = _speedDiv100 * CurPoit.Duration;
        }
        else
        {
            curSpeedDiv100 = _defaultSpeedDiv100;
        }
        _curSpeedDiv100 = curSpeedDiv100;

        double speed = _field30;
        Speed = speed;
        PartSpeed = speed * Path.Parts[PartIdx].OneDivLength;
    }

    public override bool Update()
    {
        bool updateResult;
        double speedDiv100;

        updateResult = base.Update();
        if (updateResult)
        {
            _prevSpeedDiv100 = _curSpeedDiv100;
            if (CurPoit.Duration != 0)
            {
                speedDiv100 = _speedDiv100 * CurPoit.Duration;
            }
            else
            {
                speedDiv100 = _defaultSpeedDiv100;
            }
            _curSpeedDiv100 = speedDiv100;
        }
        if (_prevSpeedDiv100 != _curSpeedDiv100)
        {
            double progress = Progress;
            double speed = (_prevSpeedDiv100 * (1.0 - progress)) + (_curSpeedDiv100 * progress);
            Speed = speed;
            PartSpeed = (speed * Path.Parts[PartIdx].OneDivLength);
        }
        return updateResult;
    }

    private void InitFromPath(MkdsPath path, double speed, short baseSpeed)
    {
        InitFromPath(path, speed);
        _speedDiv100 = speed / 100;
        _defaultSpeedDiv100 = (speed * baseSpeed) / 100;
        Init(0, true);
    }

    public static PathwalkerEx FromPath(MkdsPath path, double speed, short baseSpeed)
    {
        var pw = new PathwalkerEx();
        pw.InitFromPath(path, speed, baseSpeed);
        return pw;
    }
}
