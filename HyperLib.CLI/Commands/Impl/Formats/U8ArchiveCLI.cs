using HyperLib.Formats;
using System.IO.Compression;

namespace HyperLib.CLI.Commands.Impl.Formats
{
    [Command("U8Archive", "arc", [typeof(string), typeof(string)], "Nintendo U8 (*.arc)",
    [
        "[\"file\"|\"directory\"] [opt: \"destination\"]",
        "[opt: /Uncompressed] /SonicNext [\"directory\"] [opt: \"destination\"]",
        "/List [\"file\"]"
    ])]
    public class U8ArchiveCLI : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command)
        {
            var args = CommandHelper.GetCommandArguments(in_command.Inputs);
            var isUncompressed = false;

            foreach (var arg in args)
            {
                var ioPaths = CommandHelper.GetInputOutputPaths(arg.Value);

                switch (arg.Key)
                {
                    case "/":
                        CommandHelper.HandleArchiveType<U8Archive>(ioPaths);
                        break;

                    case "/List":
                    {
                        // TODO
                        break;
                    }

                    case "/Uncompressed":
                        isUncompressed = true;
                        break;

                    case "/SonicNext":
                    {
                        if (isUncompressed)
                            U8Archive.CompressionLevel = CompressionLevel.NoCompression;

                        CommandHelper.HandleArchiveType<U8Archive>(ioPaths);

                        break;
                    }
                }
            }
        }
    }
}
