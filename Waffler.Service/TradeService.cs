using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Waffler.Common;
using Waffler.Domain;
using static Waffler.Common.Variable;

namespace Waffler.Service
{
    public interface ITradeService
    {
        Task<TradeRuleEvaluationDTO> HandleTradeRuleAsync(int tradeRuleId, DateTime currentPeriodDateTime);

        Task<bool> SetupTestTradeAsync(int tradeRuleId);
    }

    public class TradeService : ITradeService
    {
        private readonly ILogger<TradeService> _logger;
        private readonly ICandleStickService _candleStickService;
        private readonly ITradeRuleService _tradeRuleService;
        private readonly ITradeOrderService _tradeOrderService;
        private readonly IBitpandaService _bitpandaService;

        public TradeService(
            ILogger<TradeService> logger,
            ICandleStickService candleStickService,
            ITradeRuleService tradeRuleService,
            ITradeOrderService tradeOrderService,
            IBitpandaService bitpandaService)
        {
            _logger = logger;
            _candleStickService = candleStickService;
            _tradeRuleService = tradeRuleService;
            _tradeOrderService = tradeOrderService;
            _bitpandaService = bitpandaService;
            _logger.LogDebug("TradeService instantiated");
        }

        public async Task<TradeRuleEvaluationDTO> HandleTradeRuleAsync(int tradeRuleId, DateTime currentPeriodDateTime)
        {
            _logger.LogInformation($"Handling trade rule");
            var tradeRule = await _tradeRuleService.GetTradeRuleAsync(tradeRuleId);
            
            if (CanHandleTradeRule(tradeRule, currentPeriodDateTime))
            {
                var candleStick = await _candleStickService.GetLastCandleStickAsync(currentPeriodDateTime);

                _logger.LogInformation($"Analysing \"{tradeRule.Name}\" at {currentPeriodDateTime:yyyy-MM-dd HH:mm:ss}");
                var tradeRuleResult = new TradeRuleEvaluationDTO()
                {
                    Id = tradeRule.Id,
                    Name = tradeRule.Name,
                    TradeRuleCondtionEvaluations = new List<TradeRuleConditionEvaluationDTO>()
                };

                foreach (var tradeRuleCondition in tradeRule.TradeRuleConditions.Where(_ => _.IsOn == true))
                {
                    _logger.LogInformation($" - Checking condition \"{tradeRuleCondition.Description}\"");
                    var trend = await _candleStickService.GetPriceTrendAsync(
                        currentPeriodDateTime,
                        (TradeType)tradeRule.TradeTypeId,
                        tradeRuleCondition);

                    var conditionResult = new TradeRuleConditionEvaluationDTO()
                    {
                        Id = tradeRuleCondition.Id,
                        Description = tradeRuleCondition.Description,
                        IsFullfilled = false
                    };
                    if (trend != null)
                    {
                        _logger.LogInformation($" - Trend {trend}");
                        conditionResult.IsFullfilled = EvaluateCondition(tradeRuleCondition, trend.Value);
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
                            Guid? orderId = Guid.NewGuid();
                            var price = GetPrice(tradeRule.CandleStickValueTypeId, candleStick, tradeRule.PriceDeltaPercent);
                            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.DecimalPrecision);
                            if (tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Active)
                            {
                                var order = await _bitpandaService.PlaceOrderAsync(tradeRule, amount, price);
                                orderId = order != null ? new Guid(order.Order_id) : (Guid?)null;
                            }

                            if(orderId != null)
                            {
                                await _tradeOrderService.AddTradeOrderAsync(new TradeOrderDTO()
                                {
                                    TradeActionId = (short)TradeAction.Buy,
                                    TradeOrderStatusId = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Test ? (short)TradeOrderStatus.Test : (short)TradeOrderStatus.Open,
                                    TradeRuleId = tradeRule.Id,
                                    Amount = amount,
                                    FilledAmount = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Test ? amount : 0,
                                    OrderDateTime = currentPeriodDateTime,
                                    Price = price,
                                    OrderId = orderId.Value,
                                    IsActive = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Active
                                });

                                tradeRule.LastTrigger = candleStick.PeriodDateTime;
                                await _tradeRuleService.UpdateTradeRuleAsync(tradeRule);
                            }
                            
                            break;
                        case TradeAction.Sell:
                            throw new NotImplementedException();
                    }
                    _logger.LogInformation($" - Trade complete!");
                }

                return tradeRuleResult;
            } 
            else
            {
                _logger.LogInformation($"Trade rule skipped");
                return null;
            }
        }

        public async Task<bool> SetupTestTradeAsync(int tradeRuleId)
        {
            var success = await _tradeRuleService.SetupTradeRuleTestAsync(tradeRuleId);
            if(success)
            {
                await _tradeOrderService.RemoveTestTradeOrdersAsync(tradeRuleId);
            }

            return success;
        }

        private bool CanHandleTradeRule(TradeRuleDTO tradeRule, DateTime currentPeriodDateTime)
        {
            return tradeRule.LastTrigger < currentPeriodDateTime.AddMinutes(-1 * tradeRule.TradeMinIntervalMinutes) &&
                tradeRule.TradeRuleStatusId != (short)TradeRuleStatus.Inactive &&
                tradeRule.TradeRuleConditions != null &&
                tradeRule.TradeRuleConditions.Any();
        }

        private static bool EvaluateRule(List<TradeRuleConditionEvaluationDTO> conditionResult, TradeConditionOperator tradeConditionOperator)
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

        private static bool EvaluateCondition(TradeRuleConditionDTO condition, decimal value)
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

        private static decimal GetPrice(short candleStickValueTypeId, CandleStickDTO candleStick, decimal priceDeltaPercent)
        {
            decimal basePrice = 0;
            switch ((CandleStickValueType)candleStickValueTypeId)
            {
                case CandleStickValueType.HighPrice:
                    basePrice = candleStick.HighPrice;
                    break;
                case CandleStickValueType.LowPrice:
                    basePrice = candleStick.LowPrice;
                    break;
                case CandleStickValueType.OpenPrice:
                    basePrice = candleStick.OpenPrice;
                    break;
                case CandleStickValueType.ClosePrice:
                    basePrice = candleStick.ClosePrice;
                    break;
                default:
                    break;
            }

            return basePrice + basePrice * (priceDeltaPercent / (decimal)100.0);
        }
    }
}
