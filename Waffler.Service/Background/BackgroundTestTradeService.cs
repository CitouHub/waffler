using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Waffler.Domain;
using Waffler.Service.Infrastructure;

namespace Waffler.Service.Background
{
    public class BackgroundTestTradeService : BackgroundService
    {
        private readonly ILogger<BackgroundTestTradeService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TestTradeRuleQueue _testTradeRuleQueue;

        public BackgroundTestTradeService(
            ILogger<BackgroundTestTradeService> logger,
            IServiceProvider serviceProvider,
            TestTradeRuleQueue testTradeRuleQueue)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _testTradeRuleQueue = testTradeRuleQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Waiting for trade request...");
                var tradeRequest = await _testTradeRuleQueue.DequeueAsync(cancellationToken);
                var currentStatus = _testTradeRuleQueue.SetStatus(tradeRequest);

                try
                {
                    _logger.LogInformation($"New trade request found {tradeRequest}");
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        var _tradeRuleService = scope.ServiceProvider.GetRequiredService<ITradeRuleService>();
                        var _tradeService = scope.ServiceProvider.GetRequiredService<ITradeService>();
                        var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                        var tradeRule = await _tradeRuleService.GetTradeRuleAsync(tradeRequest.TradeRuleId);
                        var tradeRuleDTO = _mapper.Map<TradeRuleDTO>(tradeRule);
                        
                        var testReady = await _tradeService.SetupTestTrade(tradeRuleDTO.Id);
                        if(testReady)
                        {
                            var results = new List<TradeRuleEvaluationDTO>();

                            currentStatus.CurrentPositionDate = tradeRequest.FromDate;
                            while (!cancellationToken.IsCancellationRequested &&
                                currentStatus.CurrentPositionDate < tradeRequest.ToDate.AddMinutes(tradeRequest.MinuteStep))
                            {
                                var result = await _tradeService.HandleTradeRule(tradeRequest.TradeRuleId, currentStatus.CurrentPositionDate);
                                if (result != null)
                                {
                                    results.Add(result);
                                    
                                }
                                currentStatus.CurrentPositionDate = currentStatus.CurrentPositionDate.AddMinutes(tradeRequest.MinuteStep);
                            }

                            _logger.LogInformation($"- Trade rule result: {tradeRuleDTO.Id}:{tradeRuleDTO.Name}");
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

                            var updated = await _tradeRuleService.UpdateTradeRule(tradeRuleDTO);
                            if(updated)
                            {
                                _logger.LogWarning($"Trade rule test finished and trade rule reset");
                            } 
                            else
                            {
                                _logger.LogWarning($"Unable to reset trade rule");
                            }
                        } 
                        else
                        {
                            _logger.LogWarning($"Unable to setup trade test");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unexpected exception", e);
                }
            }
        }
    }
}
