namespace HyperLib.CLI.Commands
{
    public interface ICommand
    {
        void Execute(List<Command> in_commands, Command in_command);
    }
}
