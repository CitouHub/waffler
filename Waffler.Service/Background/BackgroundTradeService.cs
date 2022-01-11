using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Waffler.Service.Infrastructure;
using Waffler.Service.Util;

namespace Waffler.Service.Background
{
    public class BackgroundTradeService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTradeService> _logger;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;
        private readonly TimeSpan RequestPeriod = TimeSpan.FromMinutes(5);
        private readonly TimeSpan RetryRequestPeriod = TimeSpan.FromMinutes(1);
        private readonly object StartUpLock = new object();

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundTradeService(
            ILogger<BackgroundTradeService> logger,
            IServiceProvider serviceProvider,
            IDatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("Instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            if(!cancellationToken.IsCancellationRequested)
            {
                _timer = new Timer(async _ => await HandleTradeRulesAsync(cancellationToken), null, TimeSpan.FromSeconds(0), RequestPeriod);
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
            lock (StartUpLock)
            {
                if (InProgress)
                {
                    _logger.LogInformation($"Analysing price trends for trade rules already in progress");
                    return;
                }

                InProgress = true;
            }

            _logger.LogInformation($"Waiting for database to be ready");
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);
            try
            {
                _logger.LogInformation($"Setting up scoped services");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                    var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();
                    var _tradeRuleTestQueue = scope.ServiceProvider.GetRequiredService<ITradeRuleTestQueue>();

                    _logger.LogInformation($"Preparing analyse");
                    var lastCandleStick = await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow);

                    if (DataSyncHandler.IsDataSynced(lastCandleStick))
                    {
                        _logger.LogInformation($"Getting trade rules");
                        var tradeRules = await _tradeRuleService.GetTradeRulesAsync();

                        foreach (var tradeRule in tradeRules.Where(_ => TestInProgress(_tradeRuleTestQueue, _.Id) == false))
                        {
                            _logger.LogInformation($"Analysing trade rule {tradeRule.Name}");
                            if (cancellationToken.IsCancellationRequested == false)
                            {
                                var result = await _tradeService.HandleTradeRuleAsync(tradeRule, lastCandleStick.PeriodDateTime);
                                
                                if(result != null)
                                {
                                    _logger.LogInformation($"Trade rule analyse result: {result.Id}:{result.Name}");
                                    foreach (var tradeRuleCondition in result.TradeRuleCondtionEvaluations)
                                    {
                                        _logger.LogInformation($"Condition: {tradeRuleCondition.Id}: {tradeRuleCondition.Description} = {tradeRuleCondition.IsFullfilled}");
                                    }
                                } 
                                else
                                {
                                    _logger.LogInformation($"Trade rule not applicable");
                                }
                            }

                            _timer.Change(RequestPeriod, RequestPeriod);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to do analasys, insufficient data");
                        _timer.Change(RetryRequestPeriod, RequestPeriod);
                    }
                }
            } 
            catch(Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }

            InProgress = false;

            _logger.LogInformation($"Analysing price trends for trade rules finished");
        }
    }
}