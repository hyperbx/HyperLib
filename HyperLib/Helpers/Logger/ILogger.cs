using System.Runtime.CompilerServices;

namespace HyperLib.Helpers
{
    public interface ILogger
    {
        void Log(string in_message, ELogLevel in_logLevel, [CallerMemberName] string in_caller = null);
    }
}
