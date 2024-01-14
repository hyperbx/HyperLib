using HyperLib.Formats.Sonic_Crytek;

namespace HyperLib.CLI.Commands.Impl.Formats
{
    [Command("Sonic_Crytek", "rol", [typeof(string), typeof(string)], "Sonic Boom: Rise of Lyric", "\n\t--Sonic_Crytek Archive [file|directory] [opt: destination]")]
    public class Sonic_Crytek : ICommand
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
                case "stream":
                    CommandHelper.HandleArchiveType<Archive>(inputPath, outputPath);
                    break;
            }
        }
    }
}
