namespace HyperLib.Helpers
{
    public class FileSystemHelper
    {
        public static string GetDirectoryWithFileName(string in_filePath)
        {
            return Path.Combine(Path.GetDirectoryName(in_filePath), Path.GetFileNameWithoutExtension(in_filePath));
        }
    }
}
