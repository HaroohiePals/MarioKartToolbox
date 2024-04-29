using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HaroohiePals.Graphics.Svg
{
    public class SvgPathBuilder
    {
        private readonly StringBuilder _builder = new();

        private static string Inv(double x) => x.ToString(CultureInfo.InvariantCulture);

        public void MoveTo(Vector2d point) => MoveTo(point.X, point.Y);
        public void MoveTo(double x, double y) => _builder.Append($" M {Inv(x)},{Inv(y)}");

        public void MoveToRelative(Vector2d delta) => MoveToRelative(delta.X, delta.Y);
        public void MoveToRelative(double dx, double dy) => _builder.Append($" m {Inv(dx)},{Inv(dy)}");

        public void ClosePath() => _builder.Append(" z");

        public void LineTo(Vector2d point) => LineTo(point.X, point.Y);
        public void LineTo(double x, double y) => _builder.Append($" L {Inv(x)},{Inv(y)}");

        public void LineToRelative(Vector2d delta) => LineToRelative(delta.X, delta.Y);
        public void LineToRelative(double dx, double dy) => _builder.Append($" l {Inv(dx)},{Inv(dy)}");

        public void CubicBezierTo(Vector2d control1, Vector2d control2, Vector2d point)
            => CubicBezierTo(control1.X, control1.Y, control2.X, control2.Y, point.X, point.Y);

        public void CubicBezierTo(double control1X, double control1Y, double control2X, double control2Y, double x,
            double y)
            => _builder.Append(
                $" C {Inv(control1X)},{Inv(control1Y)} {Inv(control2X)},{Inv(control2Y)} {Inv(x)},{Inv(y)}");

        public void QuadraticBezierTo(Vector2d control, Vector2d point)
            => QuadraticBezierTo(control.X, control.Y, point.X, point.Y);

        public void QuadraticBezierTo(double controlX, double controlY, double x, double y)
            => _builder.Append($" Q {Inv(controlX)},{Inv(controlY)} {Inv(x)},{Inv(y)}");

        public override string ToString() => _builder.ToString();

        public static string FromPoints(IEnumerable<Vector2d> points, bool closed)
        {
            var builder = new SvgPathBuilder();

            int count = 0;
            foreach (var point in points)
            {
                if (count == 0)
                    builder.MoveTo(point);
                else
                    builder.LineTo(point);

                count++;
            }

            if (count > 2 && closed)
                builder.ClosePath();

            return builder.ToString();
        }

        public static string FromLine(Vector2d start, Vector2d end)
        {
            var builder = new SvgPathBuilder();
            builder.MoveTo(start);
            builder.LineTo(end);
            return builder.ToString();
        }

        public static string FromQuadraticBezier(Vector2d start, Vector2d control, Vector2d end)
        {
            var builder = new SvgPathBuilder();
            builder.MoveTo(start);
            builder.QuadraticBezierTo(control, end);
            return builder.ToString();
        }

        public static string FromCubicBezier(Vector2d start, Vector2d control1, Vector2d control2, Vector2d end)
        {
            var builder = new SvgPathBuilder();
            builder.MoveTo(start);
            builder.CubicBezierTo(control1, control2, end);
            return builder.ToString();
        }
    }
}