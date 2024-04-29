using System.Xml;
using System.Xml.Serialization;

namespace HaroohiePals.Nitro.JNLib.Spl
{
    [XmlRoot("ParticleArchive")]
    public class IntermediateParticleArchive
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(IntermediateParticleArchive));

        [XmlArray]
        [XmlArrayItem("Emitter")]
        public List<string> Emitters { get; set; } = new List<string>();

        [XmlArray]
        [XmlArrayItem("Texture")]
        public List<ParticleTexture> Textures { get; set; } = new List<ParticleTexture>();

        public class ParticleTexture
        {
            public enum TexMode
            {
                Clamp,
                Repeat,
                Mirror
            }

            [XmlText]
            public string Path { get; set; }

            [XmlAttribute]
            public string Name { get; set; }

            [XmlAttribute]
            public TexMode S { get; set; }

            [XmlAttribute]
            public TexMode T { get; set; }
        }

        public byte[] ToXml()
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

        public static IntermediateParticleArchive FromXml(byte[] data)
        {
            using (var reader = new StreamReader(new MemoryStream(data)))
                return (IntermediateParticleArchive)Serializer.Deserialize(reader);
        }
    }
}