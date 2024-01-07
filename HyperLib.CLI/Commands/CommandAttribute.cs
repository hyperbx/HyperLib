namespace HyperLib.CLI.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute(string in_name, string in_alias = null, Type[] in_inputs = null, string in_description = null, string in_usage = null, string in_example = null) : Attribute
    {
        public string Name { get; set; } = in_name;
        public string Alias { get; set; } = in_alias;
        public Type[] Inputs { get; set; } = in_inputs;
        public string Description { get; set; } = in_description;
        public string Usage { get; set; } = in_usage;
        public string Example { get; set; } = in_example;
    }
}
