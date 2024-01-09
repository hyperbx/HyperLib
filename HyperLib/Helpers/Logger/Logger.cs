using System.Runtime.CompilerServices;

namespace HyperLib.Helpers
{
    public static class Logger
    {
        private static List<ILogger> _handlers = [ new ConsoleLogger() ];

        public static void Add(ILogger in_logger)
        {
            _handlers.Add(in_logger);
        }

        public static bool Remove(ILogger in_logger)
        {
            return _handlers.Remove(in_logger);
        }

        public static void Log(string in_message, ELogLevel in_logLevel, [CallerMemberName] string in_caller = null)
        {
            foreach (var logger in _handlers)
                logger.Log(in_message, in_logLevel, in_caller);
        }

        public static void Log(string in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, ELogLevel.None, in_caller);
        }

        public static void Log(string in_message)
        {
            Log(in_message, string.Empty);
        }

        public static void Utility(string in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, ELogLevel.Utility, in_caller);
        }

        public static void Utility(string in_message)
        {
            Utility(in_message, string.Empty);
        }

        public static void Warning(string in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, ELogLevel.Warning, in_caller);
        }

        public static void Warning(string in_message)
        {
            Warning(in_message, string.Empty);
        }

        public static void Error(string in_message, [CallerMemberName] string in_caller = null)
        {
            Log(in_message, ELogLevel.Error, in_caller);
        }

        public static void Error(string in_message)
        {
            Error(in_message, string.Empty);
        }
    }
}
