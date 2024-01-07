using HyperLib.Helpers;
using System.Reflection;

// https://github.com/thesupersonic16/HedgeModManager/blob/rewrite/HedgeModManager/CLI/CommandLine.cs

namespace HyperLib.CLI.Commands
{
    public class CommandProcessor
    {
        private static Dictionary<CommandAttribute, Type> _registeredCommands = [];

        static CommandProcessor()
        {
            RegisterCommands(Assembly.GetExecutingAssembly());
        }

        public static void RegisterCommands(Assembly in_assembly)
        {
            var types = in_assembly.GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t));

            if (types == null)
                return;

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CommandAttribute>();

                if (attr == null)
                    continue;

                _registeredCommands[attr] = type;
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine
            (
                $"HyperLib.CLI - Version {AssemblyExtensions.GetInformationalVersion()}\n\n" +
                $"" +
                $"Commands:"
            );

            foreach (var command in _registeredCommands)
            {
                Console.Write($"--{command.Key.Name}");

                if (!string.IsNullOrEmpty(command.Key.Alias))
                    Console.Write($"|-{command.Key.Alias}");

                if (!string.IsNullOrEmpty(command.Key.Description))
                    Console.Write($": {command.Key.Description.Replace("\n", "\n          ")}");

                Console.WriteLine();

                if (!string.IsNullOrEmpty(command.Key.Usage))
                    Console.WriteLine($"    Usage: {command.Key.Usage}");

                if (!string.IsNullOrEmpty(command.Key.Example))
                    Console.WriteLine($"    Example: {command.Key.Example}");
            }

            Console.ReadKey();
        }

        public static List<Command> ParseArguments(string[] in_args)
        {
            var commands = new List<Command>();

            for (int i = 0; i < in_args.Length; ++i)
            {
                if (in_args[i].StartsWith('-'))
                {
                    var command = _registeredCommands.FirstOrDefault(x => x.Key.Alias == in_args[i][1..]);

                    if (in_args[i].StartsWith("--"))
                        command = _registeredCommands.FirstOrDefault(x => x.Key.Name == in_args[i][2..]);

                    if (command.Key == null)
                        continue;

                    if (command.Key.Inputs?.Length + i > in_args.Length)
                    {
                        Logger.Error($"Error: too few inputs for --{command.Key.Name}", "");
                        continue;
                    }

                    var inputs = new List<object>();

                    foreach (var input in command.Key.Inputs ?? [])
                    {
                        i++;

                        if (in_args[i].StartsWith('-'))
                        {
                            Logger.Error($"Error: too few inputs for --{command.Key.Name}", "");
                            break;
                        }

                        var data = ParseDataFromString(Type.GetTypeCode(input), in_args[i]);

                        if (data != null)
                        {
                            inputs.Add(data);
                        }
                        else
                        {
                            Logger.Error($"Error: unknown type \"{input.Name}\" for --{command.Key.Name}.");
                        }
                    }

                    commands.Add(new Command(command.Key, command.Value, inputs));
                }
                else
                {
                    commands.LastOrDefault()?.Inputs.Add(in_args[i]);
                }
            }

            return commands;
        }

        public static object ParseDataFromString(TypeCode typeCode, string data)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return typeCode switch
            {
                TypeCode.String => data,
                TypeCode.Int32 => int.Parse(data),
                TypeCode.Boolean => bool.Parse(data),
                _ => null,
            };
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static void ExecuteArguments(List<Command> commands)
        {
            foreach (var command in commands)
                (Activator.CreateInstance(command.Type) as ICommand)?.Execute(commands, command);
        }
    }
}
