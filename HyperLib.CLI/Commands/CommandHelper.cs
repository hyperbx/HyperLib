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

        public static void HandleArchiveType<T>((string, string) in_inputs) where T : FileBase, new()
        {
            HandleArchiveType<T>(in_inputs.Item1, in_inputs.Item2);
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

        public static Dictionary<string, List<object>> GetCommandArguments(List<object> in_inputs)
        {
            var result = new Dictionary<string, List<object>>();
            var argName = "/";
            var argValues = new List<object>();

            foreach (var input in in_inputs)
            {
                if (input is string out_input && out_input.StartsWith('/'))
                {
                    result.Add(argName, argValues);
                    argValues.Clear();

                    argName = out_input;
                    continue;
                }

                argValues.Add(input);
            }

            return result;
        }

        public static (string InputPath, string OutputPath) GetInputOutputPaths(List<object> in_args)
        {
            var inputPath = string.Empty;
            var outputPath = string.Empty;

            if (in_args.Count != 0 && in_args[0] is string out_inputPath)
            {
                inputPath = out_inputPath;

                if (in_args.Count <= 2 && in_args[1] is string out_outputPath)
                    outputPath = out_outputPath;
            }

            return (inputPath, outputPath);
        }
    }
}
