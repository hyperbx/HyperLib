namespace HyperLib.Helpers
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string in_message, ELogLevel in_logLevel, string in_caller)
        {
            var oldColour = Console.ForegroundColor;

            switch (in_logLevel)
            {
                case ELogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;

                case ELogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;

                case ELogLevel.Utility:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.WriteLine(string.IsNullOrEmpty(in_caller) ? in_message : $"[{in_caller}] {in_message}");

            Console.ForegroundColor = oldColour;
        }
    }
}
