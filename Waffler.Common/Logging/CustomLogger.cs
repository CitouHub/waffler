using System;

using Microsoft.Extensions.Logging;

namespace Waffler.Common.Logging
{
    public sealed class CustomLogger : ILogger
    {
        private readonly string _name;
        private readonly Func<CustomLoggerConfiguration> _getCurrentConfig;

        public CustomLogger(string name, Func<CustomLoggerConfiguration> getCurrentConfig) =>
            (_name, _getCurrentConfig) = (name, getCurrentConfig);

        public IDisposable BeginScope<TState>(TState state) => default!;

        public bool IsEnabled(LogLevel logLevel) =>
            _getCurrentConfig().LogLevels.ContainsKey(logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            CustomLoggerConfiguration config = _getCurrentConfig();
            if (_name.StartsWith("Waffler"))
            {
                var source = _name[(_name.LastIndexOf(".") + 1)..];
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = config.LogLevels[logLevel];
                Console.Write($"{logLevel,-12}|");

                Console.ForegroundColor = originalColor;
                Console.Write($" {source,-35}|");

                Console.ForegroundColor = config.LogLevels[logLevel];
                Console.Write($" {formatter(state, exception)}");

                if(exception != null)
                {
                    Console.ForegroundColor = config.LogLevels[logLevel];
                    var message = exception.Message;
                    var innerMessage = exception.InnerException?.Message;
                    Console.WriteLine($" - {message}{(string.IsNullOrEmpty(innerMessage) == false ? $" - {innerMessage}" : "")}");
                    Console.WriteLine(exception.StackTrace);
                } 
                else
                {
                    Console.ForegroundColor = originalColor;
                    Console.WriteLine();
                }
            }
        }
    }
}