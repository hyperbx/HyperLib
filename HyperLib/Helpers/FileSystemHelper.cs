namespace HyperLib.Helpers
{
    public class FileSystemHelper
    {
        public static string GetDirectoryWithFileName(string in_filePath)
        {
            return Path.Combine(Path.GetDirectoryName(in_filePath), Path.GetFileNameWithoutExtension(in_filePath));
        }

        public static string GetDirectoryNameFromRoot(string in_rootDir, string in_path, bool in_isConvertToUnixSeparators = false)
        {
            var relativePath = in_path[in_rootDir.Length..].TrimStart(Path.DirectorySeparatorChar);

            if (in_isConvertToUnixSeparators)
                relativePath = relativePath.Replace('\\', '/');

            return relativePath;
        }

        public static string TruncateLastExtension(string in_filePath)
        {
            return Path.Combine(Path.GetDirectoryName(in_filePath), Path.GetFileNameWithoutExtension(in_filePath));
        }

        public static EFileSystemItemType GetFileSystemItemType(string in_path)
        {
            if (Directory.Exists(in_path))
                return EFileSystemItemType.Directory;

            return EFileSystemItemType.File;
        }

        public enum EFileSystemItemType
        {
            File,
            Directory
        }
    }
}
