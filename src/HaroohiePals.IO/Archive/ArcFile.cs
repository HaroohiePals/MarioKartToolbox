namespace HaroohiePals.IO.Archive
{
    public class ArcFile : ArcEntry
    {
        public ArcFile(string name, ArcDirectory parent, byte[] data)
            : base(name, parent)
        {
            Data = data;
        }

        public byte[] Data { get; set; }
    }
}