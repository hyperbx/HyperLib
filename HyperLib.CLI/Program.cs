using HyperLib.CLI.Commands;

namespace HyperLib.CLI
{
    internal class Program
    {
        static void Main(string[] in_args)
        {
            if (in_args.Length <= 0)
            {
                CommandProcessor.ShowHelp();
                return;
            }

            CommandProcessor.ExecuteArguments(CommandProcessor.ParseArguments(in_args)); // ?
        }
    }
}
