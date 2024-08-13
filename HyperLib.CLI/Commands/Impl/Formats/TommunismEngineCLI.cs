using HyperLib.Formats.TommunismEngine;

namespace HyperLib.CLI.Commands.Impl.Formats
{
    [Command("TommunismEngine", "tom", [typeof(string), typeof(string)], "Super Meat Boy",
    [
        "/Archive [\"file\"|\"directory\"] [opt: \"destination\"]",
        "/Registry [\"file\"] [opt: \"destination\"]",
        "/TexturePackage [\"file\"|\"directory\"] [opt: \"destination\"]"
    ])]
    public class TommunismEngineCLI : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command)
        {
            string inputPath = in_command.Inputs[1] as string;
            string outputPath = null;

            if (in_command.Inputs.Count > 2)
                outputPath = in_command.Inputs[2] as string;

            switch (in_command.Inputs[0] as string)
            {
                case "/Archive":
                case "/dat":
                    CommandHelper.HandleArchiveType<Archive>(inputPath, outputPath);
                    break;

                case "/Registry":
                case "/reg":
                    CommandHelper.HandleJsonType<Registry>(inputPath, outputPath);
                    break;

                case "/TexturePackage":
                case "/tp":
                    CommandHelper.HandleArchiveType<TexturePackage>(inputPath, outputPath);
                    break;
            }
        }
    }
}
