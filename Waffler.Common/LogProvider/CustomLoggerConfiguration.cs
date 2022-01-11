using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Waffler.Common.LogProvider
{
    public class CustomLoggerConfiguration
    {
        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new Dictionary<LogLevel, ConsoleColor>()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }
}
