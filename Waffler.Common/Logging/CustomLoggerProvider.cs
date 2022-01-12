using System;
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Waffler.Common.Logging
{
    [ProviderAlias("CustomLogger")]
    public sealed class CustomLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private CustomLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new ConcurrentDictionary<string, CustomLogger>(StringComparer.OrdinalIgnoreCase);

        public CustomLoggerProvider(IOptionsMonitor<CustomLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new CustomLogger(name, GetCurrentConfig));

        private CustomLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
