namespace HyperLib.CLI.Commands.Impl
{
    [Command("Help", "h", in_description: "Displays help information.")]
    public class Help : ICommand
    {
        public void Execute(List<Command> in_commands, Command in_command)
        {
            CommandProcessor.ShowHelp();
        }
    }
}
