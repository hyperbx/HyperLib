using HyperLib.Frameworks.TommunismEngine;
using HyperLib.Helpers;

namespace HyperLib.CLI.Commands.Impl.Frameworks
{
    [Command("TommunismEngine", "tom", [typeof(string), typeof(string)], "The game engine developed for Super Meat Boy.",
        "\n\t--TommunismEngine Archive [file|directory] [opt: destination]\n" +
        "\t--TommunismEngine Registry [file] [opt: destination]\n" +
        "\t--TommunismEngine TexturePackage [file|directory] [opt: destination]")]
    public class TommunismEngine : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command)
        {
            string inputPath = in_command.Inputs[1] as string;
            string outputPath = null;

            if (in_command.Inputs.Count > 2)
                outputPath = in_command.Inputs[2] as string;

            switch (in_command.Inputs[0] as string)
            {
                case "Archive":
                case "dat":
                {
                    if (FileSystemHelper.GetFileSystemItemType(inputPath) == FileSystemHelper.EFileSystemItemType.File)
                    {
                        new Archive(inputPath, false).Export(outputPath);
                    }
                    else
                    {
                        var dat = new Archive();
                        dat.Import(inputPath);
                        dat.Write(outputPath ?? inputPath + dat.Extension);
                    }

                    break;
                }

                case "Registry":
                case "reg":
                {
                    var dat = new Registry();

                    if (Path.GetExtension(inputPath) == ".json")
                    {
                        dat.Import(inputPath);
                        dat.Write(outputPath ?? FileSystemHelper.TruncateLastExtension(inputPath));
                    }
                    else
                    {
                        dat.Read(inputPath);
                        dat.Export(outputPath);
                    }

                    break;
                }

                case "TexturePackage":
                case "tp":
                {
                    if (FileSystemHelper.GetFileSystemItemType(inputPath) == FileSystemHelper.EFileSystemItemType.File)
                    {
                        new TexturePackage(inputPath).Export(outputPath);
                    }
                    else
                    {
                        var tp = new TexturePackage();
                        tp.Import(inputPath);
                        tp.Write(outputPath ?? inputPath + tp.Extension);
                    }

                    break;
                }
            }
        }
    }
}
