namespace HyperLib.CLI.Commands
{
    public class Command(CommandAttribute in_attribute, Type in_type, List<object> in_inputs)
    {
        public CommandAttribute Attribute { get; set; } = in_attribute;
        public Type Type { get; set; } = in_type;
        public List<object> Inputs { get; set; } = in_inputs;
    }
}
