using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Waffler.Domain;
using Waffler.Domain.Message;
using Waffler.Service.Infrastructure;

namespace Waffler.Service.Background
{
    public class BackgroundTestTradeService : BackgroundService
    {
        private readonly ILogger<BackgroundTestTradeService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TradeRuleTestQueue _testTradeRuleQueue;
        private readonly DatabaseSetupSignal _databaseSetupSignal;

        public BackgroundTestTradeService(
            ILogger<BackgroundTestTradeService> logger,
            IServiceProvider serviceProvider,
            TradeRuleTestQueue testTradeRuleQueue,
            DatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _testTradeRuleQueue = testTradeRuleQueue;
            _databaseSetupSignal = databaseSetupSignal;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Waiting for trade rule test request...");
                var tradeRuleTestRequest = await _testTradeRuleQueue.DequeueTestAsync(cancellationToken);
                var currentStatus = _testTradeRuleQueue.GetTradeRuleTestStatus(tradeRuleTestRequest.TradeRuleId);
                if(currentStatus != null && currentStatus.Progress < 100)
                {
                    _testTradeRuleQueue.AbortTest(tradeRuleTestRequest.TradeRuleId);
                    await _testTradeRuleQueue.AwaitClose(cancellationToken, tradeRuleTestRequest.TradeRuleId);
                }

                if(cancellationToken.IsCancellationRequested == false)
                {
                    _ = RunTradeTest(cancellationToken, tradeRuleTestRequest);
                }
            }
        }

        public async Task RunTradeTest(CancellationToken cancellationToken, TradeRuleTestRequestDTO tradeRuleTestRequest)
        {
            _logger.LogInformation($"New trade rule test request found {tradeRuleTestRequest}");
            var currentStatus = _testTradeRuleQueue.InitTradeRuleTestRun(tradeRuleTestRequest);

            try
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                    var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    var tradeRule = await _tradeRuleService.GetTradeRuleAsync(tradeRuleTestRequest.TradeRuleId);
                    if(tradeRule != null)
                    {
                        var tradeRuleDTO = _mapper.Map<TradeRuleDTO>(tradeRule);

                        var testReady = await _tradeService.SetupTestTrade(tradeRuleDTO.Id);
                        if (testReady)
                        {
                            var results = new List<TradeRuleEvaluationDTO>();

                            currentStatus.CurrentPositionDate = tradeRuleTestRequest.FromDate;
                            while (!cancellationToken.IsCancellationRequested &&
                                !_testTradeRuleQueue.IsTestAborted(tradeRule.Id) &&
                                currentStatus.CurrentPositionDate < tradeRuleTestRequest.ToDate.AddMinutes(tradeRuleTestRequest.MinuteStep))
                            {
                                var result = await _tradeService.HandleTradeRule(tradeRuleTestRequest.TradeRuleId, currentStatus.CurrentPositionDate);
                                if (result != null)
                                {
                                    results.Add(result);

                                }
                                currentStatus.CurrentPositionDate = currentStatus.CurrentPositionDate.AddMinutes(tradeRuleTestRequest.MinuteStep);
                            }

                            _logger.LogInformation($"- Trade rule test result: {tradeRuleDTO.Id}:{tradeRuleDTO.Name}");
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

                            var updated = await _tradeRuleService.UpdateTradeRuleAsync(tradeRuleDTO);
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
                            _logger.LogWarning($"Unable to setup trade rule test");
                        }
                    } 
                    else
                    {
                        _logger.LogWarning($"Trade rule not found");
                    }
                }

                var testStatus = _testTradeRuleQueue.GetTradeRuleTestStatus(tradeRuleTestRequest.TradeRuleId);

                if (cancellationToken.IsCancellationRequested || testStatus.Aborted)
                {
                    _logger.LogInformation($"Trade rule test ended prematurely");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }

            _testTradeRuleQueue.CloseTest(tradeRuleTestRequest.TradeRuleId);
        }
    }
}
