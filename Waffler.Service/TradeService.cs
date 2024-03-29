﻿using System;
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
        Task<TradeRuleEvaluationDTO> HandleTradeRuleAsync(TradeRuleDTO tradeRule, DateTime currentPeriodDateTime);

        Task<bool> SetupTestTradeAsync(int tradeRuleId);
    }

    public class TradeService : ITradeService
    {
        private readonly ILogger<TradeService> _logger;
        private readonly ICandleStickService _candleStickService;
        private readonly ITradeRuleService _tradeRuleService;
        private readonly ITradeOrderService _tradeOrderService;
        private readonly IBitpandaService _bitpandaService;
        private readonly IStatisticsService _statisticsService;

        public TradeService(
            ILogger<TradeService> logger,
            ICandleStickService candleStickService,
            ITradeRuleService tradeRuleService,
            ITradeOrderService tradeOrderService,
            IBitpandaService bitpandaService,
            IStatisticsService statisticsService)
        {
            _logger = logger;
            _candleStickService = candleStickService;
            _tradeRuleService = tradeRuleService;
            _tradeOrderService = tradeOrderService;
            _bitpandaService = bitpandaService;
            _statisticsService = statisticsService;
            _logger.LogDebug("Instantiated");
        }

        public async Task<TradeRuleEvaluationDTO> HandleTradeRuleAsync(TradeRuleDTO tradeRule, DateTime currentPeriodDateTime)
        {
            _logger.LogInformation($"Handling trade rule \"{tradeRule.Name}\"");

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
                    _logger.LogInformation($"Checking condition \"{tradeRuleCondition.Description}\"");
                    var trend = await _statisticsService.GetPriceTrendAsync(
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
                        _logger.LogInformation($"Trend {trend}");
                        conditionResult.IsFullfilled = EvaluateCondition(tradeRuleCondition, trend.Change);
                    }

                    tradeRuleResult.TradeRuleCondtionEvaluations.Add(conditionResult);
                }

                var tradeConditionOperator = (TradeConditionOperator)tradeRule.TradeConditionOperatorId;
                _logger.LogInformation($"Evaluating result ({tradeConditionOperator}) {tradeRuleResult}");
                var ruleFullfilled = EvaluateRule(tradeRuleResult.TradeRuleCondtionEvaluations, tradeConditionOperator);
                _logger.LogInformation($"Rule fullfilled: {ruleFullfilled}");

                if (ruleFullfilled)
                {
                    var tradeAction = (TradeAction)tradeRule.TradeActionId;
                    _logger.LogInformation($"Trade action: {tradeAction}");
                    switch (tradeAction)
                    {
                        case TradeAction.Buy:
                            Guid? orderId = Guid.NewGuid();
                            var orderDate = currentPeriodDateTime;
                            var price = Math.Round(GetPrice(tradeRule.CandleStickValueTypeId, candleStick, tradeRule.PriceDeltaPercent), Bitpanda.PriceDecimalPrecision);
                            var amount = Math.Round(tradeRule.Amount / price, Bitpanda.AmountDecimalPrecision);
                            if (tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Active)
                            {
                                var balance = await _bitpandaService.GetAccountAsync();
                                var available = balance.Balances.FirstOrDefault(_ => _.Currency_code == Bitpanda.CurrencyCode.EUR).Available;
                                if (tradeRule.Amount >= available)
                                {
                                    amount = Math.Round(available * (decimal)0.9 / price, Bitpanda.AmountDecimalPrecision);
                                    _logger.LogWarning($"Not enough balance for full order, amount recalculated to {amount}");
                                }
                                var order = await _bitpandaService.TryPlaceOrderAsync(tradeRule, amount, price);
                                orderId = order != null ? new Guid(order.Order_id) : (Guid?)null;
                                orderDate = order != null ? order.Time : currentPeriodDateTime;
                            }

                            if (orderId != null)
                            {
                                var tradeOrderStatusId = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Test ? (short)TradeOrderStatus.Test : (short)TradeOrderStatus.Open;
                                var filledAmout = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Test && await IsTestOrderFilled(price, currentPeriodDateTime, tradeRule.TradeOrderExpirationMinutes) ? amount : 0;
                                var isActive = tradeRule.TradeRuleStatusId == (short)TradeRuleStatus.Active;

                                await _tradeOrderService.AddTradeOrderAsync(new TradeOrderDTO()
                                {
                                    TradeActionId = (short)TradeAction.Buy,
                                    TradeOrderStatusId = tradeOrderStatusId,
                                    TradeRuleId = tradeRule.Id,
                                    Amount = amount,
                                    FilledAmount = filledAmout,
                                    OrderDateTime = orderDate,
                                    Price = price,
                                    OrderId = orderId.Value,
                                    IsActive = isActive
                                });

                                tradeRule.LastTrigger = candleStick.PeriodDateTime;
                                await _tradeRuleService.UpdateTradeRuleAsync(tradeRule);
                            }

                            break;
                        case TradeAction.Sell:
                            throw new NotImplementedException();
                    }
                    _logger.LogInformation($"Trade complet!");
                }

                return tradeRuleResult;
            }
            else
            {
                _logger.LogInformation($"Trade rule skipped");
                return null;
            }
        }

        public async Task<bool> IsTestOrderFilled(decimal price, DateTime currentPeriodDateTime, int? tradeOrderExpirationMinutes)
        {
            var toDate = tradeOrderExpirationMinutes != null ? currentPeriodDateTime.AddMinutes(tradeOrderExpirationMinutes.Value) : DateTime.UtcNow;
            var candleSticks = await _candleStickService.GetCandleSticksAsync(currentPeriodDateTime, toDate, TradeType.BTC_EUR, 1);
            var priceValid = candleSticks == null || candleSticks.Any(_ => _.LowPrice <= price);
            return priceValid;
        }

        public async Task<bool> SetupTestTradeAsync(int tradeRuleId)
        {
            var success = await _tradeRuleService.SetupTradeRuleTestAsync(tradeRuleId);
            if (success)
            {
                await _tradeOrderService.RemoveTestTradeOrdersAsync(tradeRuleId);
            }

            return success;
        }
        private bool CanHandleTradeRule(TradeRuleDTO tradeRule, DateTime currentPeriodDateTime)
        {
            return tradeRule.LastTrigger < currentPeriodDateTime.AddMinutes(-1 * tradeRule.TradeMinIntervalMinutes) &&
                tradeRule.TradeRuleStatusId != (short)TradeRuleStatus.Inactive &&
                tradeRule.CandleStickValueTypeId != (short)CandleStickValueType.Volume &&
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
