using HaroohiePals.NitroKart.MapData.Intermediate;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace HaroohiePals.MarioKartToolbox.Gui.View.Main
{
    public class NitroKartCourseProject
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(NitroKartCourseProject));

        public string RomPath { get; set; } = "";

        public static bool Create(string destinationPath, string romPath, bool isMgCourse)
        {
            try
            {
                var project = new NitroKartCourseProject();
                project.RomPath = romPath;

                File.WriteAllBytes(destinationPath, project.WriteXml());

                string destinationRoot = Path.GetDirectoryName(destinationPath);
                string mapDataPath = Path.Combine(destinationRoot, "course_map.inkm");

                if (!File.Exists(mapDataPath))
                {
                    var mapData = new MkdsMapData(isMgCourse);
                    File.WriteAllBytes(mapDataPath, mapData.WriteXml());
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public byte[] WriteXml()
        {
            var m = new MemoryStream();
            var xns = new XmlSerializerNamespaces();
            xns.Add(string.Empty, string.Empty);
            using (var writer = XmlWriter.Create(m, new XmlWriterSettings { Indent = true }))
            {
                Serializer.Serialize(writer, this, xns);
                writer.Close();
            }

            return m.ToArray();
        }

        public static NitroKartCourseProject ReadXml(byte[] data)
        {
            return ReadXml(new MemoryStream(data));
        }

        public static NitroKartCourseProject ReadXml(MemoryStream m)
        {
            var deserializer = Serializer;
            NitroKartCourseProject data;
            using (var reader = new StreamReader(m))
                data = (NitroKartCourseProject)deserializer.Deserialize(reader);

            return data;
        }
    }

    //public class ProjectCourse : Course
    //{
    //    private NitroKartCourseProject _project;

    //    public ProjectCourse(string path) : base(new DiskArchive(Path.GetDirectoryName(path)), new DiskArchive(Path.GetDirectoryName(path)))
    //    {
    //        string basePath = Path.GetDirectoryName(path);

    //        _project = NitroKartCourseProject.ReadXml(File.ReadAllBytes(path));

    //    }
    //}
}
