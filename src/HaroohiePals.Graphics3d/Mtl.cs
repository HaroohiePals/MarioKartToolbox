using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace HaroohiePals.Graphics3d
{
    public class Mtl
    {
        public Mtl() { }

        public Mtl(byte[] data)
            : this(new MemoryStream(data, false)) { }

        public Mtl(Stream stream)
        {
            MtlMaterial curMat = null;
            using (var tr = new StreamReader(stream))
            {
                string line;
                while ((line = tr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length < 1 || line.StartsWith("#"))
                        continue;

                    string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 1)
                        continue;
                    switch (parts[0])
                    {
                        case "newmtl":
                            if (parts.Length < 2)
                                continue;
                            if (curMat != null) Materials.Add(curMat);
                            curMat = new MtlMaterial(parts[1]);
                            break;
                        case "Ka":
                        {
                            if (parts.Length < 4)
                                continue;
                            double r = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double g = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double b = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            curMat.AmbientColor =
                                Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
                            break;
                        }
                        case "Kd":
                        {
                            if (parts.Length < 4)
                                continue;
                            double r = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double g = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double b = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            curMat.DiffuseColor =
                                Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
                            break;
                        }
                        case "Ks":
                        {
                            if (parts.Length < 4)
                                continue;
                            double r = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            double g = double.Parse(parts[2], CultureInfo.InvariantCulture);
                            double b = double.Parse(parts[3], CultureInfo.InvariantCulture);
                            curMat.SpecularColor =
                                Color.FromArgb((int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
                            break;
                        }
                        case "d":
                            if (parts.Length < 2)
                                continue;
                            curMat.Alpha = double.Parse(parts[1], CultureInfo.InvariantCulture);
                            break;
                        case "map_Kd":
                            curMat.DiffuseMapPath = line.Substring(parts[0].Length + 1).Trim();
                            break;
                    }
                }

                if (curMat != null && !Materials.Contains(curMat))
                    Materials.Add(curMat);
            }
        }

        public byte[] Write()
        {
            var b = new StringBuilder();
            foreach (var m in Materials)
            {
                b.AppendFormat("newmtl {0}\n", m.Name);
                b.AppendFormat("Ka {0} {1} {2}\n",
                    (m.AmbientColor.R / 255.0).ToString(CultureInfo.InvariantCulture),
                    (m.AmbientColor.G / 255.0).ToString(CultureInfo.InvariantCulture),
                    (m.AmbientColor.B / 255.0).ToString(CultureInfo.InvariantCulture));
                b.AppendFormat("Kd {0} {1} {2}\n",
                    (m.DiffuseColor.R / 255.0).ToString(CultureInfo.InvariantCulture),
                    (m.DiffuseColor.G / 255.0).ToString(CultureInfo.InvariantCulture),
                    (m.DiffuseColor.B / 255.0).ToString(CultureInfo.InvariantCulture));
                b.AppendFormat("Ks {0} {1} {2}\n",
                    (m.SpecularColor.R / 255.0).ToString(CultureInfo.InvariantCulture),
                    (m.SpecularColor.G / 255.0).ToString(CultureInfo.InvariantCulture),
                    (m.SpecularColor.B / 255.0).ToString(CultureInfo.InvariantCulture));
                b.AppendFormat("d {0}\n", m.Alpha.ToString(CultureInfo.InvariantCulture));
                if (m.DiffuseMapPath != null)
                    b.AppendFormat("map_Kd {0}\n", m.DiffuseMapPath);
                b.AppendLine();
            }

            return Encoding.ASCII.GetBytes(b.ToString());
        }

        public readonly List<MtlMaterial> Materials = new();

        public MtlMaterial GetMaterialByName(string name)
            => Materials.FirstOrDefault(m => m.Name == name);

        public class MtlMaterial
        {
            public MtlMaterial(string name)
            {
                Name = name;
            }

            public string Name;
            public Color  DiffuseColor;
            public Color  AmbientColor;
            public Color  SpecularColor;
            public double Alpha = 1;

            public string DiffuseMapPath;

            public override string ToString() => Name;
        }
    }
}