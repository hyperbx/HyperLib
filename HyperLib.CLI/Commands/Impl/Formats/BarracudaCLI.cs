using HyperLib.Formats.Barracuda;
using HyperLib.Helpers;

namespace HyperLib.CLI.Commands.Impl.Formats
{
    [Command("Barracuda", "hth", [typeof(string), typeof(string)], "Hydro Thunder Hurricane",
    [
        "/Archive [opt: /PC] [\"file\"|\"directory\"] [opt: \"destination\"]",
        "/JsonBinary [opt: /PC] [\"file\"|\"directory\"] [opt: \"destination\"]",
    ])]
    public class BarracudaCLI : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command)
        {
            bool isPCVersion = (in_command.Inputs[1] as string) == "/PC";

            string inputPath = (in_command.Inputs[isPCVersion ? 2 : 1] as string)!;
            string outputPath = null;

            if (in_command.Inputs.Count > (isPCVersion ? 3 : 2))
                outputPath = (in_command.Inputs[isPCVersion ? 3 : 2] as string)!;

            switch (in_command.Inputs[0] as string)
            {
                case "/Archive":
                case "/apf":
                {
                    var apf = new Archive
                    {
                        IsPCVersion = isPCVersion
                    };

                    if (FileSystemHelper.GetBasicType(inputPath) == FileSystemHelper.EFileSystemBasicType.File)
                    {
                        apf.Read(inputPath);
                        apf.Export(outputPath);
                    }
                    else
                    {
                        apf.Import(inputPath);
                        apf.Write(outputPath ?? inputPath + apf.Extension);
                    }

                    break;
                }

                case "/JsonBinary":
                case "/ajb":
                {
                    var ajb = new JsonBinary()
                    {
                        IsPCVersion = isPCVersion
                    };

                    if (Path.GetExtension(inputPath) == ".json")
                    {
                        ajb.Import(inputPath);
                        ajb.Write(outputPath ?? FileSystemHelper.TruncateLastExtension(inputPath));
                    }
                    else
                    {
                        ajb.Read(inputPath);
                        ajb.Export(outputPath);
                    }

                    break;
                }
            }
        }
    }
}
