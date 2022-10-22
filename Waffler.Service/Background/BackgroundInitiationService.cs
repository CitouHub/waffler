using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Waffler.Service.Infrastructure;

#pragma warning disable IDE0063 // Use simple 'using' statement
namespace Waffler.Service.Background
{
    public class BackgroundInitiationService : BackgroundService
    {
        private readonly ILogger<BackgroundInitiationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;
        private readonly IConfigCache _configCache;

        public BackgroundInitiationService(
            ILogger<BackgroundInitiationService> logger,
            IServiceProvider serviceProvider,
            IDatabaseSetupSignal databaseSetupSignal,
            IConfigCache configCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;
            _configCache = configCache;
            _logger.LogDebug("Instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            if(cancellationToken.IsCancellationRequested == false)
            {
                await SetupConfigCache();
            }
        }

        public async Task SetupConfigCache()
        {
            _logger.LogInformation($"Setting up config cache");
            
            try
            {
                _logger.LogDebug($"Setting up scoped services");

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var apiKey = await _profileService.GetBitpandaApiKeyAsync();
                    _configCache.SetApiKey(apiKey);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected exception");
            }

            _logger.LogInformation($"Setting up config cache finished");
        }
    }
}
