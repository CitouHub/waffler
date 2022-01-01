using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Waffler.Domain;
using Waffler.Domain.Message;
using Waffler.Service.Infrastructure;

namespace Waffler.Service.Background
{
    public class BackgroundTestTradeService : BackgroundService
    {
        private readonly ILogger<BackgroundTestTradeService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITradeRuleTestQueue _tradeRuleTestQueue;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;

        public BackgroundTestTradeService(
            ILogger<BackgroundTestTradeService> logger,
            IServiceProvider serviceProvider,
            ITradeRuleTestQueue tradeRuleTestQueue,
            IDatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _tradeRuleTestQueue = tradeRuleTestQueue;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("BackgroundTestTradeService instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Waiting for trade rule test request...");
                var tradeRuleTestRequest = await _tradeRuleTestQueue.DequeueTestAsync(cancellationToken);
                await AbortOngoingTest(cancellationToken, tradeRuleTestRequest.TradeRuleId);

                if (cancellationToken.IsCancellationRequested == false)
                {
                    _ = RunTradeTest(cancellationToken, tradeRuleTestRequest);
                }
            }
        }

        public async Task AbortOngoingTest(CancellationToken cancellationToken, int tradeRuleId)
        {
            var currentStatus = _tradeRuleTestQueue.GetTradeRuleTestStatus(tradeRuleId);
            if (currentStatus != null && currentStatus.Progress < 100)
            {
                _tradeRuleTestQueue.AbortTest(tradeRuleId);
                await _tradeRuleTestQueue.AwaitClose(cancellationToken, tradeRuleId);
            }
        }

        public async Task RunTradeTest(CancellationToken cancellationToken, TradeRuleTestRequestDTO tradeRuleTestRequest)
        {
            _logger.LogInformation($"New trade rule test request found {tradeRuleTestRequest}");
            var currentStatus = _tradeRuleTestQueue.InitTradeRuleTestRun(tradeRuleTestRequest);

            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                    var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();

                    var testReady = await _tradeService.SetupTestTradeAsync(tradeRuleTestRequest.TradeRuleId);
                    if (testReady)
                    {
                        var tradeRule = await _tradeRuleService.GetTradeRuleAsync(tradeRuleTestRequest.TradeRuleId);

                        var results = new List<TradeRuleEvaluationDTO>();

                        currentStatus.CurrentPositionDate = tradeRuleTestRequest.FromDate;
                        while (!cancellationToken.IsCancellationRequested &&
                            !_tradeRuleTestQueue.IsTestAborted(tradeRule.Id) &&
                            currentStatus.CurrentPositionDate < tradeRuleTestRequest.ToDate.AddMinutes(tradeRuleTestRequest.MinuteStep))
                        {
                            var result = await _tradeService.HandleTradeRuleAsync(tradeRule, currentStatus.CurrentPositionDate);
                            if (result != null)
                            {
                                results.Add(result);

                            }
                            currentStatus.CurrentPositionDate = currentStatus.CurrentPositionDate.AddMinutes(tradeRuleTestRequest.MinuteStep);
                        }

                        _logger.LogInformation($"- Trade rule test result: {tradeRule.Id}:{tradeRule.Name}");
                        foreach (var tradeRuleCondition in results.SelectMany(_ => _.TradeRuleCondtionEvaluations).GroupBy(_ => new
                        {
                            _.Id,
                            _.Description
                        }))
                        {
                            var conditionName = $"{tradeRuleCondition.Key.Id}:{tradeRuleCondition.Key.Description}";
                            var conditions = tradeRuleCondition.Count();
                            var fullfilled = tradeRuleCondition.Count(_ => _.IsFullfilled == true);
                            _logger.LogInformation($"- - Condition: {conditionName} = {fullfilled}/{conditions}");
                        }

                        var updated = await _tradeRuleService.UpdateTradeRuleAsync(tradeRule);
                        if (updated)
                        {
                            _logger.LogInformation($"Trade rule test finished and trade rule reset");
                        }
                        else
                        {
                            _logger.LogWarning($"Unable to reset trade rule after test");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to setup trade rule test, trade rule not found");
                    }
                }

                var testStatus = _tradeRuleTestQueue.GetTradeRuleTestStatus(tradeRuleTestRequest.TradeRuleId);

                if (cancellationToken.IsCancellationRequested || (testStatus?.Aborted ?? false))
                {
                    _logger.LogInformation($"Trade rule test ended prematurely");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }

            _tradeRuleTestQueue.CloseTest(tradeRuleTestRequest.TradeRuleId);
        }
    }
}
