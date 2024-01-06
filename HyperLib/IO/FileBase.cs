namespace HyperLib.IO
{
    public class FileBase
    {
        public string Location { get; set; }

        public FileBase() { }

        public FileBase(string in_path) : this()
        {
            Location = in_path;
        }
    }
}
