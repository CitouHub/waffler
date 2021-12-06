﻿using Microsoft.Extensions.Logging;
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

        Task<bool> SetupTestTrade(int tradeRuleId);
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

                foreach (var condition in tradeRule.TradeRuleConditions.Where(_ => _.IsOn == true))
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
                        var value = GetTargetValue(condition.CandleStickValueTypeId, trends);
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
                            var orderId = Guid.NewGuid();
                            var price = GetPrice(tradeRule.CandleStickValueTypeId, candleStick, tradeRule.PriceDeltaPercent);
                            var amount = Math.Round(tradeRule.Amount / price, 8);
                            if (tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Active)
                            {
                                //orderId = await _bitpandaService.CreateOrderAsync(new CreateOrderDTO(){});
                            }

                            await _tradeOrderService.AddTradeOrderAsync(new TradeOrderDTO()
                            {
                                TradeActionId = (short)TradeAction.Buy,
                                TradeOrderStatusId = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Test ? (short)TradeOrderStatus.Test : (short)TradeOrderStatus.Open,
                                TradeRuleId = tradeRule.Id,
                                Amount = amount,
                                OrderDateTime = currentPeriodDateTime,
                                Price = price,
                                OrderId = orderId
                            });
                            tradeRule.LastTrigger = candleStick.PeriodDateTime;
                            await _tradeRuleService.UpdateTradeRuleAsync(tradeRule);
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

        private bool CanHandleTradeRule(TradeRuleDTO tradeRule, DateTime currentPeriodDateTime)
        {
            return tradeRule.LastTrigger < currentPeriodDateTime.AddMinutes(-1 * tradeRule.TradeMinIntervalMinutes) &&
                tradeRule.TradeRuleStatusId != (short)TradeRuleStatus.Disabled;
        }

        public async Task<bool> SetupTestTrade(int tradeRuleId)
        {
            var success = await _tradeRuleService.SetupTradeRuleTestAsync(tradeRuleId);
            if(success)
            {
                await _tradeOrderService.RemoveTestTradeOrdersAsync(tradeRuleId);
            }

            return success;
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

        public static decimal GetTargetValue(short candleStickValueTypeId, PriceTrendsDTO trends)
        {
            switch ((CandleStickValueType)candleStickValueTypeId)
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

        public static decimal GetPrice(short candleStickValueTypeId, CandleStickDTO candleStick, decimal priceDeltaPercent)
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
                case CandleStickValueType.HighLowPrice:
                    basePrice = ((candleStick.HighPrice + candleStick.LowPrice) / (decimal)2.0);
                    break;
                case CandleStickValueType.OpenClosePrice:
                    basePrice = ((candleStick.OpenPrice + candleStick.ClosePrice) / (decimal)2.0);
                    break;
                case CandleStickValueType.AvgHighLowPrice:
                    basePrice = ((candleStick.HighPrice + candleStick.LowPrice) / (decimal)2.0);
                    break;
                case CandleStickValueType.AvgOpenClosePrice:
                    basePrice = ((candleStick.OpenPrice + candleStick.ClosePrice) / (decimal)2.0);
                    break;
                default:
                    break;
            }

            return basePrice + basePrice * (priceDeltaPercent / (decimal)100.0);
        }
    }
}
