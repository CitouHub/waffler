using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Waffler.Domain;
using Waffler.Service.Infrastructure;

namespace Waffler.Service.Background
{
    public class BackgroundTradeService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTradeService> _logger;
        private readonly DatabaseSetupSignal _databaseSetupSignal;
        private readonly TimeSpan RequestPeriod = TimeSpan.FromMinutes(5);
        private readonly TimeSpan RetryRequestPeriod = TimeSpan.FromMinutes(1);
        private readonly TimeSpan ValidSyncOffser = TimeSpan.FromMinutes(15);

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundTradeService(
            ILogger<BackgroundTradeService> logger,
            IServiceProvider serviceProvider,
            DatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("BackgroundTradeService instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            if(!cancellationToken.IsCancellationRequested)
            {
                _timer = new Timer(async _ => await HandleTradeRules(cancellationToken), null, TimeSpan.FromSeconds(0), RequestPeriod);
            }
        }

        private async Task HandleTradeRules(CancellationToken cancellationToken)
        {
            if (InProgress)
            {
                return;
            }
            InProgress = true;

            try
            {
                _logger.LogInformation($"Running trade analysis");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _logger.LogInformation($"- Setting up scoped services");
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                    var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();

                    _logger.LogInformation($"- Getting last candlestick");
                    var lastCandleStick = await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow);

                    if (IsDataSynced(lastCandleStick))
                    {
                        _logger.LogInformation($"- Data synced, last period {lastCandleStick.PeriodDateTime}, analyse trade rules...");
                        var tradeRules = await _tradeRuleService.GetTradeRulesAsync();

                        foreach (int tradeRuleId in tradeRules.Where(_ => _.TestTradeInProgress == false).Select(_ => _.Id))
                        {
                            if (cancellationToken.IsCancellationRequested == false)
                            {
                                var result = await _tradeService.HandleTradeRuleAsync(tradeRuleId, lastCandleStick.PeriodDateTime);
                                
                                if(result != null)
                                {
                                    _logger.LogInformation($"- Trade rule analyse result: {result.Id}:{result.Name}");
                                    foreach (var tradeRuleCondition in result.TradeRuleCondtionEvaluations)
                                    {
                                        _logger.LogInformation($"- - Condition: {tradeRuleCondition.Id}:{tradeRuleCondition.Description} = {tradeRuleCondition.IsFullfilled}");
                                    }
                                }
                            }

                            _timer.Change(RequestPeriod, RequestPeriod);
                        }
                    }
                    else
                    {
                        if (lastCandleStick == null)
                        {
                            _logger.LogWarning($"- Data not synced, no data available");
                        }
                        else
                        {
                            _logger.LogWarning($"- Data not synced, last period {lastCandleStick.PeriodDateTime}");
                            _timer.Change(RetryRequestPeriod, RequestPeriod);
                        }
                    }
                }
                _logger.LogInformation($"Running trade analysis finished");
            } 
            catch(Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
            InProgress = false;
        }

        private bool IsDataSynced(CandleStickDTO lastCandleStick)
        {
            return lastCandleStick != null && Math.Abs((decimal)(DateTime.UtcNow - lastCandleStick.PeriodDateTime).TotalMinutes) < (int)ValidSyncOffser.TotalMinutes; 
        }
    }
}