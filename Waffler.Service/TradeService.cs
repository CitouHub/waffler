using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Waffler.Domain;
using static Waffler.Common.Variable;

namespace Waffler.Service
{
    public interface ITradeService
    {
        Task<TradeRuleEvaluationDTO> HandleTradeRule(int tradeRuleId, DateTime currentPeriodDateTime);
    }

    public class TradeService : ITradeService
    {
        private readonly ILogger<TradeService> _logger;
        private readonly ICandleStickService _candleStickService;
        private readonly ITradeRuleService _tradeRuleService;
        private readonly ITradeOrderService _tradeOrderService;

        public TradeService(
            ILogger<TradeService> logger,
            ICandleStickService candleStickService,
            ITradeRuleService tradeRuleService,
            ITradeOrderService tradeOrderService)
        {
            _logger = logger;
            _candleStickService = candleStickService;
            _tradeRuleService = tradeRuleService;
            _tradeOrderService = tradeOrderService;
        }

        public async Task<TradeRuleEvaluationDTO> HandleTradeRule(int tradeRuleId, DateTime currentPeriodDateTime)
        {
            var candleStick = await _candleStickService.GetLastCandleStickAsync(currentPeriodDateTime);
            var tradeRule = await _tradeRuleService.GetTradeRuleAsync(tradeRuleId);

            _logger.LogInformation($"Analysing \"{tradeRule.Name}\" at {currentPeriodDateTime:yyyy-MM-dd HH:mm:ss}");
            var tradeRuleResult = new TradeRuleEvaluationDTO()
            {
                Id = tradeRule.Id,
                Name = tradeRule.Name,
                TradeRuleCondtionEvaluations = new List<TradeRuleConditionEvaluationDTO>()
            };

            foreach (var condition in tradeRule.TradeRuleConditions)
            {
                _logger.LogInformation($" - Checking condition \"{condition.Description}\"");
                var trends = await _candleStickService.GetPriceTrendsAsync(
                    currentPeriodDateTime,
                    (TradeType)tradeRule.TradeTypeId,
                    (TradeRuleConditionSampleDirection)condition.TradeRuleConditionSampleDirectionId,
                    condition.FromMinutesOffset,
                    condition.ToMinutesOffset,
                    condition.FromMinutesSample,
                    condition.ToMinutesSample);
                _logger.LogInformation($" - Trends {trends}");
                var conditionResult = new TradeRuleConditionEvaluationDTO()
                {
                    Id = condition.Id,
                    Description = condition.Description,
                    IsFullfilled = false
                };

                if (trends != null)
                {
                    var value = GetTargetValue(condition, trends);
                    conditionResult.IsFullfilled = EvaluateCondition(condition, value);
                }

                tradeRuleResult.TradeRuleCondtionEvaluations.Add(conditionResult);
            }

            var tradeConditionOperator = (TradeConditionOperator)tradeRule.TradeConditionOperatorId;
            _logger.LogInformation($" - Evaluating result ({tradeConditionOperator}) {tradeRuleResult}");
            var ruleFullfilled = EvaluateRule(tradeRuleResult.TradeRuleCondtionEvaluations, tradeConditionOperator);
            _logger.LogInformation($" - Rule fullfilled: {ruleFullfilled}");

            if (ruleFullfilled)
            {
                var tradeAction = (TradeAction)tradeRule.TradeActionId;
                _logger.LogInformation($" - Trade action: {tradeAction}");
                switch (tradeAction)
                {

                    case TradeAction.Buy:
                        //await _bitpandaService.CreateOrderAsync(new CreateOrderDTO(){});

                        await _tradeOrderService.CreateTradeOrder(new TradeOrderDTO()
                        {
                            Amount = tradeRule.Amount,
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
                _logger.LogInformation($" - Trade complete!");
            }

            return tradeRuleResult;
        }

        public static bool EvaluateRule(List<TradeRuleConditionEvaluationDTO> conditionResult, TradeConditionOperator tradeConditionOperator)
        {
            if (conditionResult.Any() == false)
            {
                return false;
            }

            switch (tradeConditionOperator)
            {
                case TradeConditionOperator.AND:
                    return conditionResult.All(_ => _.IsFullfilled == true);
                case TradeConditionOperator.OR:
                    return conditionResult.Any(_ => _.IsFullfilled == true);
                default:
                    break;
            }

            return false;
        }

        public static bool EvaluateCondition(TradeRuleConditionDTO condition, decimal value)
        {
            switch ((TradeRuleConditionComparator)condition.TradeRuleConditionComparatorId)
            {
                case TradeRuleConditionComparator.MoreThen:
                    return value > condition.DeltaPercent;
                case TradeRuleConditionComparator.LessThen:
                    return value < condition.DeltaPercent;
                case TradeRuleConditionComparator.AbsMoreThen:
                    return Math.Abs(value) > condition.DeltaPercent;
                case TradeRuleConditionComparator.AbsLessThen:
                    return Math.Abs(value) < condition.DeltaPercent;
                default:
                    break;
            }

            return false;
        }

        public static decimal GetTargetValue(TradeRuleConditionDTO condition, PriceTrendsDTO trends)
        {
            switch ((CandleStickValueType)condition.CandleStickValueTypeId)
            {
                case CandleStickValueType.HighPrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.LowPrice:
                    return trends.LowPriceTrend;
                case CandleStickValueType.OpenPrice:
                    return trends.OpenPriceTrend;
                case CandleStickValueType.ClosePrice:
                    return trends.ClosePriceTrend;
                case CandleStickValueType.HighLowPrice:
                    return trends.HighLowPriceTrend;
                case CandleStickValueType.OpenClosePrice:
                    return trends.OpenClosePriceTrend;
                case CandleStickValueType.AvgHighLowPrice:
                    return trends.AvgHighLowPriceTrend;
                case CandleStickValueType.AvgOpenClosePrice:
                    return trends.AvgHighLowPriceTrend;
                default:
                    break;
            }

            return default;
        }
    }
}
