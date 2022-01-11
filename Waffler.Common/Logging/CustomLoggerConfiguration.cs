using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace Waffler.Common.Logging
{
    public class CustomLoggerConfiguration
    {
        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new Dictionary<LogLevel, ConsoleColor>()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }
}
