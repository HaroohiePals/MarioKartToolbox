using OpenTK.Mathematics;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace HaroohiePals.Graphics.Svg
{
    public class SvgWriter : IDisposable
    {
        protected bool _disposed;

        private XmlWriter _writer;

        private int _groupDepth = 0;

        private static string Inv(double x) => x.ToString(CultureInfo.InvariantCulture);

        public SvgWriter(Stream stream, Vector2d canvasSize)
            : this(stream, canvasSize.X, canvasSize.Y) { }

        public SvgWriter(Stream stream, double canvasWidth, double canvasHeight)
        {
            _writer = XmlWriter.Create(stream, new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent   = true,
                CloseOutput = true
            });
            _writer.WriteStartElement("svg");
            _writer.WriteAttributeString("width", Inv(canvasWidth));
            _writer.WriteAttributeString("height", Inv(canvasHeight));
        }

        public void BeginGroup() => BeginGroup(0, 0);
        public void BeginGroup(Vector2d translation) => BeginGroup(translation.X, translation.Y);

        public void BeginGroup(double tx, double ty)
        {
            _groupDepth++;
            _writer.WriteStartElement("g");
            if (tx != 0 || ty != 0)
                _writer.WriteAttributeString("transform", $"translate({Inv(tx)}, {Inv(ty)})");
        }

        public void EndGroup()
        {
            _writer.WriteEndElement();
            _groupDepth--;
        }

        public void WriteRect(Vector2d position, Vector2d size, string stroke, double strokeWidth, string fill = "none")
            => WriteRect(position.X, position.Y, size.X, size.Y, stroke, strokeWidth, fill);

        public void WriteRect(double x, double y, double width, double height, string stroke, double strokeWidth,
            string fill = "none")
        {
            _writer.WriteStartElement("rect");
            _writer.WriteAttributeString("x", Inv(x));
            _writer.WriteAttributeString("y", Inv(y));
            _writer.WriteAttributeString("width", Inv(width));
            _writer.WriteAttributeString("height", Inv(height));
            _writer.WriteAttributeString("stroke", stroke);
            if (stroke != "none")
                _writer.WriteAttributeString("stroke-width", Inv(strokeWidth));
            _writer.WriteAttributeString("fill", fill);
            _writer.WriteEndElement();
        }

        public void WritePath(SvgPathBuilder path, string stroke, double strokeWidth, string fill = "none")
            => WritePath(path.ToString(), stroke, strokeWidth, fill);

        public void WritePath(string path, string stroke, double strokeWidth, string fill = "none")
        {
            _writer.WriteStartElement("path");
            _writer.WriteAttributeString("fill", fill);
            _writer.WriteAttributeString("stroke", stroke);
            if (stroke != "none")
                _writer.WriteAttributeString("stroke-width", Inv(strokeWidth));
            _writer.WriteAttributeString("d", path);
            _writer.WriteEndElement();
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                while (_groupDepth > 0)
                    EndGroup();
                _writer.WriteEndElement();
                _writer.Close();
                _writer.Dispose();
            }

            _disposed = true;
        }
    }
}