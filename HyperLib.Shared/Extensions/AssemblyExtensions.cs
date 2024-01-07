using System.Reflection;

namespace HyperLib
{
    public class AssemblyExtensions
    {
        /// <summary>
        /// Returns the assembly informational version from the entry assembly. 
        /// </summary>
        public static string GetInformationalVersion()
        {
            return Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion
                .Split('+')[0]; // weird behaviour introduced in .NET 8?
        }

        /// <summary>
        /// Returns the current assembly name.
        /// </summary>
        public static string GetAssemblyName()
        {
            return Assembly.GetEntryAssembly().GetName().Name;
        }

        /// <summary>
        /// Returns the current assembly version.
        /// </summary>
        public static string GetAssemblyVersion()
        {
            return Assembly.GetEntryAssembly().GetName().Version.ToString();
        }
    }
}