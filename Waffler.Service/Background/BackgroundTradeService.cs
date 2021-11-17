using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Waffler.Domain;

namespace Waffler.Service.Background
{

    public class BackgroundTradeService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTradeService> _logger;

        private Timer _timer;
        private TimeSpan RequestPeriod = TimeSpan.FromMinutes(5);
        private TimeSpan ValidSyncOffser = TimeSpan.FromMinutes(15);
        private bool InProgress = false;

        public BackgroundTradeService(
            ILogger<BackgroundTradeService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await HandleTradeRules(cancellationToken), null, TimeSpan.Zero, RequestPeriod);
            return Task.CompletedTask;
        }

        private async Task HandleTradeRules(CancellationToken cancellationToken)
        {
            if(InProgress)
            {
                return;
            }

            InProgress = true;
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();

                _logger.LogInformation($"Running trade analysis");
                var lastCandleStick = await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow);
                
                if (IsDataSynced(lastCandleStick))
                {
                    _logger.LogWarning($"- Data synced, last period {lastCandleStick.PeriodDateTime}, analyse trade rules...");
                    var tradeRules = await _tradeRuleService.GetTradeRulesAsync();

                    foreach(int tradeRuleId in tradeRules.Select(_ => _.Id))
                    {
                        if(cancellationToken.IsCancellationRequested == false)
                        {
                            var result = await _tradeService.HandleTradeRule(tradeRuleId, lastCandleStick.PeriodDateTime);

                            _logger.LogInformation($"- Trade rule analyse result: {result.Id}:{result.Name}");
                            foreach (var tradeRuleCondition in result.TradeRuleCondtionEvaluations)
                            {
                                _logger.LogInformation($"- - Condition: {tradeRuleCondition.Id}:{tradeRuleCondition.Description} = {tradeRuleCondition.IsFullfilled}");
                            }
                        }
                    }
                } 
                else
                {
                    if(lastCandleStick == null)
                    {
                        _logger.LogWarning($"- Data not synced, no data available");
                    }
                    else
                    {
                        _logger.LogWarning($"- Data not synced, last period {lastCandleStick.PeriodDateTime}");
                    }
                }
            }
            _logger.LogInformation($"Syncing waffle candle stick data finished");
            InProgress = false;
        }

        private bool IsDataSynced(CandleStickDTO lastCandleStick)
        {
            return lastCandleStick != null && Math.Abs((decimal)(DateTime.UtcNow - lastCandleStick.PeriodDateTime).TotalMinutes) < (int)ValidSyncOffser.TotalMinutes; 
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
