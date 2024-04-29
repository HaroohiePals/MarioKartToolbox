using HaroohiePals.NitroKart.MapData.Intermediate.Sections;
using OpenTK.Mathematics;

namespace HaroohiePals.NitroKart.Race;

//https://github.com/Gericom/MarioKartDS/blob/master/Matching/src/race/cameraRoute.c
public class CameraRoute
{
    private Vector3d[] _pointCache = new Vector3d[4];
    public double Progress = 0;
    private int _index = 0;
    private int _field38 = 0;

    public CameraRoute(MkdsCamera camera, bool reversed = false)
    {
        if (reversed)
            InitReversed(camera);
        else
            Init(camera);
    }

    private void Init(MkdsCamera camera)
    {
        var path = camera.Path?.Target;

        if (path is null || path.Points.Count < 4)
            return;

        for (int i = 0; i < 4; i++)
        {
            _pointCache[i] = path.Points[i].Position;
            _index = i;
        }
    }

    private void InitReversed(MkdsCamera camera)
    {
        var path = camera.Path?.Target;

        if (path is null || path.Points.Count < 4)
            return;

        _field38 = path.Points.Count - 1;
        _index = 2;

        int poit = 2;

        for (int i = 0; i < 3; i++)
        {
            _pointCache[i] = path.Points[poit].Position;
            poit--;
        }

        _pointCache[3] = path.Points[path.Points.Count - 1].Position;
    }

    public Vector3d Update(MkdsCamera camera, double pointSpeed)
    {
        var path = camera.Path?.Target;

        if (path is null)
            return Vector3d.Zero;

        Progress += pointSpeed;
        if (Progress > 1)
        {
            if (++_index >= path.Points.Count)
            {
                _index = path.Points.Count;
                Progress = 1;
            }
            else
            {
                var entry = path.Points[_index];

                for (int i = 0; i < 3; i++)
                    _pointCache[i] = _pointCache[i + 1];

                _pointCache[3] = entry.Position;
                Progress -= 1;
            }
        }

       return InterpolateCubicSpline();
    }

    private Vector3d InterpolateCubicSpline()
    {
        double prog = Progress;
        double progSqInv = (1 - prog) * (1 - prog);
        double progSq = prog * prog;
        double progCu = progSq * prog;

        double[] coefs = new double[4];

        coefs[0] = (1f - prog) / 6f * progSqInv;
        coefs[1] = (3f * progCu - 6f * progSq + 4) / 6f;
        coefs[2] = (3f * (prog + progSq - progCu) + 1f) / 6f;
        coefs[3] = progCu / 6f;

        var dst =
            (_pointCache[0] * coefs[0]) +
            (_pointCache[1] * coefs[1]) +
            (_pointCache[2] * coefs[2]) +
            (_pointCache[3] * coefs[3]);
        return dst;
    }
}
