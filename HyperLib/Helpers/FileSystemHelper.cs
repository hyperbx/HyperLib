namespace HyperLib.Helpers
{
    public class FileSystemHelper
    {
        public static string GetDirectoryWithFileName(string in_filePath)
        {
            return Path.Combine(Path.GetDirectoryName(in_filePath), Path.GetFileNameWithoutExtension(in_filePath));
        }

        public static string GetDirectoryNameFromRoot(string in_rootDir, string in_path)
        {
            return in_path[in_rootDir.Length..];
        }
    }
}
