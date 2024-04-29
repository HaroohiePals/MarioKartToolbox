using System.Xml;

namespace HaroohiePals.Nitro.NitroSystem.G3d.Intermediate
{
    public class Head
    {
        public Head(XmlElement element)
        {
            var create = element["create"];
            if (create != null)
            {
                CreateUser   = IntermediateUtil.GetStringAttribute(create, "user");
                CreateHost   = IntermediateUtil.GetStringAttribute(create, "host");
                CreateDate   = IntermediateUtil.GetStringAttribute(create, "date");
                CreateSource = IntermediateUtil.GetStringAttribute(create, "source");
            }

            var title = element["title"];
            if (title != null)
                Title = title.InnerText;

            var comment = element["comment"];
            if (comment != null)
                Comment = comment.InnerText;

            var generator = element["generator"];
            if (generator != null)
            {
                GeneratorName    = IntermediateUtil.GetStringAttribute(generator, "name");
                GeneratorVersion = IntermediateUtil.GetStringAttribute(generator, "version");
            }
        }

        public string CreateUser;
        public string CreateHost;
        public string CreateDate;
        public string CreateSource;

        public string Title;

        public string Comment;

        public string GeneratorName;
        public string GeneratorVersion;
    }
}