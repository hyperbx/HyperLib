namespace HyperLib.Helpers
{
    public class FileSystemHelper
    {
        public static string GetDirectoryNameOfFileName(string in_filePath)
        {
            return Path.Combine(Path.GetDirectoryName(in_filePath), TruncateAllExtensions(in_filePath, true));
        }

        public static string GetRelativeDirectoryName(string in_rootDir, string in_path, bool in_isConvertToUnixSeparators = false)
        {
            var relativePath = in_path[in_rootDir.Length..].TrimStart(Path.DirectorySeparatorChar);

            if (in_isConvertToUnixSeparators)
                relativePath = relativePath.Replace('\\', '/');

            return relativePath;
        }

        public static string TruncateAllExtensions(string in_filePath, bool in_isFileNameOnly = false)
        {
            var name = Path.GetFileName(in_filePath).Split('.', StringSplitOptions.RemoveEmptyEntries)[0];

            if (in_isFileNameOnly)
                return name;

            return Path.Combine(Path.GetDirectoryName(in_filePath), name);
        }

        public static string TruncateLastExtension(string in_filePath, bool in_isFileNameOnly = false)
        {
            var name = Path.GetFileNameWithoutExtension(in_filePath);

            if (in_isFileNameOnly)
                return name;

            return Path.Combine(Path.GetDirectoryName(in_filePath), name);
        }

        public static EFileSystemBasicType GetBasicType(string in_path)
        {
            if (Directory.Exists(in_path))
                return EFileSystemBasicType.Directory;

            return EFileSystemBasicType.File;
        }

        public enum EFileSystemBasicType
        {
            File,
            Directory
        }
    }
}
