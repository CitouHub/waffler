using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Waffler.Common;
using Waffler.Domain;
using Waffler.Function.Util;
using Waffler.Service;
using static Waffler.Common.Variable;

namespace Waffler.Function
{
    public class WafflerTrader
    {
        private readonly ICandleStickService _candleStickService;
        private readonly ITradeRuleService _tradeRuleService;
        private readonly ITradeOrderService _tradeOrderService;

        public WafflerTrader(
            ICandleStickService candleStickService,
            ITradeRuleService tradeRuleService,
            ITradeOrderService tradeOrderService)
        {
            _candleStickService = candleStickService;
            _tradeRuleService = tradeRuleService;
            _tradeOrderService = tradeOrderService;
        }

        [FunctionName("WafflerTrader")]
        [DebugDisable]
        public async Task RunWafflerTraderAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Syncing waffle candle stick data");
            await HandleTradeRules(DateTime.UtcNow, log);
        }

        [FunctionName("WafflerTraderReplay")]
        [DebugDisable]
        public async Task RunWafflerTraderReplayAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            var currentPeriodDateTime = new DateTime(2021, 10, 6, 0, 0, 0, DateTimeKind.Utc);
            //var currentPeriodDateTime = new DateTime(2021, 10, 13, 11, 45, 0, DateTimeKind.Utc);
            var results = new List<TradeRuleEvaluationDTO>();
            while(currentPeriodDateTime < DateTime.UtcNow)
            {
                var result = await HandleTradeRules(currentPeriodDateTime, log);
                results.AddRange(result);
                currentPeriodDateTime = currentPeriodDateTime.AddMinutes(15);
            }

            foreach(var tradeRule in results.GroupBy(_ => new { _.Id, _.Name }))
            {
                log.LogInformation($"Trade rule result: {tradeRule.Key.Id}:{tradeRule.Key.Name}");
                var tradeRuleConditions = results.Where(_ => _.Id == tradeRule.Key.Id)
                    .SelectMany(_ => _.TradeRuleCondtionEvaluations)
                    .Where(_ => _.IsFullfilled == true)
                    .GroupBy(_ => new { _.Id, _.Description})
                    .Select(_ => new { _.Key.Id, _.Key.Description, Count = _.Count()});
                foreach(var tradeRuleCondition in tradeRuleConditions)
                {
                    log.LogInformation($"- Condition: {tradeRuleCondition.Id}:{tradeRuleCondition.Description} = {tradeRuleCondition.Count}");
                }
            }
        }

        private async Task<List<TradeRuleEvaluationDTO>> HandleTradeRules(DateTime currentPeriodDateTime, ILogger log)
        {
            var candleStick = await _candleStickService.GetLastCandleStickAsync(currentPeriodDateTime);
            var tradeRules = (await _tradeRuleService.GetTradeRulesAsync())
                .Where(_ => _.IsActive == true && _.LastTrigger < candleStick.PeriodDateTime.AddMinutes(-1 * _.TradeMinIntervalMinutes));
            var result = new List<TradeRuleEvaluationDTO>();
            
            foreach (var tradeRule in tradeRules)
            {
                log.LogInformation($"Analysing \"{tradeRule.Name}\" at {currentPeriodDateTime:yyyy-MM-dd HH:mm:ss}");
                var tradeRuleResult = new TradeRuleEvaluationDTO()
                {
                    Id = tradeRule.Id,
                    Name = tradeRule.Name,
                    TradeRuleCondtionEvaluations = new List<TradeRuleConditionEvaluationDTO>()
                };

                foreach (var condition in tradeRule.TradeRuleConditions)
                {
                    log.LogInformation($" - Checking condition \"{condition.Description}\"");
                    var trends = await _candleStickService.GetPriceTrendsAsync(
                        currentPeriodDateTime,
                        (TradeType)tradeRule.TradeTypeId,
                        (TradeRuleConditionSampleDirection)condition.TradeRuleConditionSampleDirectionId,
                        condition.FromMinutesOffset,
                        condition.ToMinutesOffset,
                        condition.FromMinutesSample,
                        condition.ToMinutesSample);
                    log.LogInformation($" - Trends {trends}");
                    var conditionResult = new TradeRuleConditionEvaluationDTO()
                    {
                        Id = condition.Id,
                        Description = condition.Description,
                        IsFullfilled = false
                    };

                    if (trends != null)
                    {
                        var value = TradeRuleHelper.GetTargetValue(condition, trends);
                        conditionResult.IsFullfilled = TradeRuleHelper.EvaluateCondition(condition, value);
                    }

                    tradeRuleResult.TradeRuleCondtionEvaluations.Add(conditionResult);
                }

                var tradeConditionOperator = (TradeConditionOperator)tradeRule.TradeConditionOperatorId;
                log.LogInformation($" - Evaluating result ({tradeConditionOperator}) {tradeRuleResult}");
                var ruleFullfilled = TradeRuleHelper.EvaluateRule(tradeRuleResult.TradeRuleCondtionEvaluations, tradeConditionOperator);
                log.LogInformation($" - Rule fullfilled: {ruleFullfilled}");

                if (ruleFullfilled)
                {
                    var tradeAction = (TradeAction)tradeRule.TradeActionId;
                    log.LogInformation($" - Trade action: {tradeAction}");
                    switch (tradeAction)
                    {
                        
                        case TradeAction.Buy:
                            //await _bitpandaService.CreateOrderAsync(new CreateOrderDTO(){});
                            
                            await _tradeOrderService.CreateTradeOrder(new TradeOrderDTO()
                            {
                                Amount = tradeRule.Amount,
                                InstrumentCode = Bitpanda.GetInstrumentCode((TradeType)tradeRule.TradeTypeId),
                                OrderDateTime = currentPeriodDateTime,
                                Price = candleStick.HighPrice,
                                TradeRuleId = tradeRule.Id,
                                OrderId = Guid.NewGuid(),
                                TradeOrderStatusId = (short)TradeOrderStatus.Open
                            });
                            await _tradeRuleService.UpdateTradeRuleLastTrigger(tradeRule.Id, candleStick.PeriodDateTime);
                            break;
                        case TradeAction.Sell:
                            throw new NotImplementedException();
                    }
                    log.LogInformation($" - Trade complete!");
                }

                result.Add(tradeRuleResult);
            }

            return result;
        }
    }
}