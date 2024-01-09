using HyperLib.CLI.Commands;

if (args.Length <= 0)
{
    CommandProcessor.ShowHelp();
    return;
}

CommandProcessor.ExecuteArguments(CommandProcessor.ParseArguments(args));