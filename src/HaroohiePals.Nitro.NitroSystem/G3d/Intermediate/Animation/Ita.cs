using HaroohiePals.Nitro.NitroSystem.G3d.Binary.Animation.TextureSrtAnimation;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate.Animation
{
    public class Ita
    {
        public Ita(byte[] data)
               : this(new MemoryStream(data, false)) { }

        public Ita(Stream stream)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var doc = new XmlDocument();
            doc.Load(stream);

            var root = doc["ita"];
            Version = IntermediateUtil.GetStringAttribute(root, "version");

            Head = new Head(root["head"]);

            var body = root["body"];
            if (body == null)
                throw new Exception();
        }

        public string Version;

        public Head Head;

        public Nsbta ToNsbta(string modelName) => ToNsbta(new[] { this }, new[] { modelName });

        public static Nsbta ToNsbta(Ita[] itas, string[] modelNames)
        {
            //todo
            return null;
        }
    }
}
