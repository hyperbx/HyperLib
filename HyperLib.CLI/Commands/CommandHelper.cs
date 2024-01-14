using HyperLib.Helpers;
using HyperLib.IO;

namespace HyperLib.CLI.Commands
{
    public static class CommandHelper
    {
        public static void HandleArchiveType<T>(string in_inputPath, string in_outputPath) where T : FileBase, new()
        {
            var t = new T();

            if (FileSystemHelper.GetBasicType(in_inputPath) == FileSystemHelper.EFileSystemBasicType.File)
            {
                t.Read(in_inputPath);
                t.Export(in_outputPath);
            }
            else
            {
                t.Import(in_inputPath);
                t.Write(in_outputPath ?? in_inputPath + t.Extension);
            }
        }

        public static void HandleJsonType<T>(string in_inputPath, string in_outputPath) where T : FileBase, new()
        {
            var t = new T();

            if (Path.GetExtension(in_inputPath) == ".json")
            {
                t.Import(in_inputPath);
                t.Write(in_outputPath ?? FileSystemHelper.TruncateLastExtension(in_inputPath));
            }
            else
            {
                t.Read(in_inputPath);
                t.Export(in_outputPath);
            }
        }
    }
}
