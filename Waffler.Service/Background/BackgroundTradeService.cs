using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Waffler.Service.Infrastructure;

#pragma warning disable IDE0063 // Use simple 'using' statement
namespace Waffler.Service.Background
{
    public class BackgroundTradeService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTradeService> _logger;
        private readonly ICandleStickSyncSignal _candleStickSyncSignal;

        public BackgroundTradeService(
            ILogger<BackgroundTradeService> logger,
            IServiceProvider serviceProvider,
            ICandleStickSyncSignal candleStickSyncSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _candleStickSyncSignal = candleStickSyncSignal;
            _logger.LogDebug("Instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Waiting for candle stick data to be fully synced...");
                await _candleStickSyncSignal.AwaitSyncCompleteAsync(cancellationToken);
                await HandleTradeRulesAsync(cancellationToken);
            }
        }

        public bool TestInProgress(ITradeRuleTestQueue tradeRuleTestQueue, int tradeRuleId)
        {
            var tradeRuleTestStatus = tradeRuleTestQueue.GetTradeRuleTestStatus(tradeRuleId);
            return tradeRuleTestStatus != null && tradeRuleTestStatus.Progress < 100;
        }

        public async Task HandleTradeRulesAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Analysing price trends for trade rules");
            try
            {
                _logger.LogDebug($"Setting up scoped services");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                    var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();
                    var _tradeRuleTestQueue = scope.ServiceProvider.GetRequiredService<ITradeRuleTestQueue>();

                    _logger.LogInformation($"Preparing analyse");
                    var lastCandleStick = await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow);

                    _logger.LogInformation($"Getting trade rules");
                    var tradeRules = await _tradeRuleService.GetTradeRulesAsync();

                    foreach (var tradeRule in tradeRules.Where(_ => TestInProgress(_tradeRuleTestQueue, _.Id) == false))
                    {
                        _logger.LogInformation($"Analysing trade rule \"{tradeRule.Name}\"");
                        if (cancellationToken.IsCancellationRequested == false)
                        {
                            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, lastCandleStick.PeriodDateTime);

                            if (result != null)
                            {
                                _logger.LogInformation($"Trade rule analyse result: \"{result.Name}\"");
                                foreach (var tradeRuleCondition in result.TradeRuleCondtionEvaluations)
                                {
                                    _logger.LogInformation($"Condition: \"{tradeRuleCondition.Description}\" = {tradeRuleCondition.IsFullfilled}");
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"Trade rule not applicable");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected exception");
            }

            _logger.LogInformation($"Analysing price trends for trade rules finished");
        }
    }
}