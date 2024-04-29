using HaroohiePals.Graphics;
using OpenTK.Mathematics;
using System;
using System.Globalization;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate
{
    public static class IntermediateUtil
    {
        public static int GetIntAttribute(XmlElement element, string attribute, int defaultValue = 0)
        {
            if (!element.HasAttribute(attribute))
                return defaultValue;

            if (!int.TryParse(element.GetAttribute(attribute), out int val))
                throw new Exception();

            return val;
        }

        public static double GetDoubleAttribute(XmlElement element, string attribute, double defaultValue = 0)
        {
            if (!element.HasAttribute(attribute))
                return defaultValue;

            if (!double.TryParse(element.GetAttribute(attribute), NumberStyles.Float | NumberStyles.AllowThousands,
                    CultureInfo.InvariantCulture, out double val))
                throw new Exception();

            return val;
        }

        public static string GetStringAttribute(XmlElement element, string attribute, string defaultValue = null)
        {
            if (!element.HasAttribute(attribute))
                return defaultValue;

            return element.GetAttribute(attribute);
        }

        public static bool GetOnOffAttribute(XmlElement element, string attribute, bool defaultValue = false)
        {
            if (!element.HasAttribute(attribute))
                return defaultValue;

            return element.GetAttribute(attribute) == "on";
        }

        public static string[] GetArrayAttribute(XmlElement element, string attribute)
        {
            if (!element.HasAttribute(attribute))
                return null;

            return element.GetAttribute(attribute)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        public static Rgb555 GetRgb555Attribute(XmlElement element, string attribute, Rgb555 defaultValue = default)
        {
            var components = GetArrayAttribute(element, attribute);
            if (components == null)
                return defaultValue;

            if (components.Length != 3)
                throw new Exception();

            if (!int.TryParse(components[0], out int r) || r < 0 || r > 31)
                throw new Exception();

            if (!int.TryParse(components[1], out int g) || g < 0 || g > 31)
                throw new Exception();

            if (!int.TryParse(components[2], out int b) || b < 0 || b > 31)
                throw new Exception();

            return new(r, g, b);
        }

        public static Vector2d GetVec2Attribute(XmlElement element, string attribute, Vector2d defaultValue = default)
        {
            var components = GetArrayAttribute(element, attribute);
            if (components == null)
                return defaultValue;

            if (components.Length != 2)
                throw new Exception();

            double x = double.Parse(components[0], CultureInfo.InvariantCulture);
            double y = double.Parse(components[1], CultureInfo.InvariantCulture);

            return new(x, y);
        }

        public static Vector3d GetVec3Attribute(XmlElement element, string attribute, Vector3d defaultValue = default)
        {
            var components = GetArrayAttribute(element, attribute);
            if (components == null)
                return defaultValue;

            if (components.Length != 3)
                throw new Exception();

            double x = double.Parse(components[0], CultureInfo.InvariantCulture);
            double y = double.Parse(components[1], CultureInfo.InvariantCulture);
            double z = double.Parse(components[2], CultureInfo.InvariantCulture);

            return new(x, y, z);
        }

        public static Matrix4d GetMtx4Attribute(XmlElement element, string attribute, Matrix4d defaultValue = default)
        {
            var components = GetArrayAttribute(element, attribute);
            if (components == null)
                return defaultValue;

            if (components.Length != 16)
                throw new Exception();

            var matrix = new Matrix4d();
            for (int i = 0; i < 16; i++)
                matrix[i / 4, i % 4] = double.Parse(components[i], CultureInfo.InvariantCulture);

            return matrix;
        }

        public static string[] GetInnerTextParts(XmlElement element)
        {
            return element.InnerText.Split(new[] { ' ', '\r', '\n', '\t' },
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }
}