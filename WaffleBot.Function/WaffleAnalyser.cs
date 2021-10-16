using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WaffleBot.Common;
using WaffleBot.Domain;
using WaffleBot.Service;
using static WaffleBot.Common.Variable;

namespace WaffleBot.Function
{
    public class WaffleAnalyser
    {
        private readonly IMapper _mapper;
        private readonly IBitpandaService _bitpandaService;
        private readonly ICandleStickService _candleStickService;
        private readonly ITradeRuleService _tradeRuleService;
        private readonly IProfileService _profileService;
        private readonly ITradeOrderService _tradeOrderService;

        private readonly Dictionary<string, bool> FunctionActive = new Dictionary<string, bool>()
        {
            {"WafflerDataFetch", true },
            {"WafflerTradeAnalyser", false },
        };

        public WaffleAnalyser(
            IMapper mapper,
            IBitpandaService bitpandaService,
            ICandleStickService candleStickService,
            ITradeRuleService tradeRuleService,
            IProfileService profileService,
            ITradeOrderService tradeOrderService)
        {
            _mapper = mapper;
            _bitpandaService = bitpandaService;
            _candleStickService = candleStickService;
            _tradeRuleService = tradeRuleService;
            _profileService = profileService;
            _tradeOrderService = tradeOrderService;
        }

        private bool IsFunctionActive(string functionName)
        {
            return FunctionActive[functionName];
        }

        [FunctionName("WafflerDataFetch")]
        public async Task RunWafflerDataFetchAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            if(IsFunctionActive("WafflerDataFetch"))
            {
                log.LogInformation($"Syncing waffle candle stick data");
                var syncingData = true;
                var requestMinutes = 120;
                var defaultStart = -60 * 24 * 30; //If no data exists then start 30 days back

                while (syncingData)
                {
                    var period = (await _candleStickService.GetLastCandleStickAsync())?.PeriodDateTime ?? DateTime.UtcNow.AddMinutes(defaultStart);
                    period = period.AddMinutes(1);
                    log.LogInformation($"- Fetch data from {period} onward");

                    var bp_cancleSticksDTO = await _bitpandaService.GetCandleSticks(
                        Bitpanda.GetInstrumentCode(TradeType.BTC_EUR),
                        Bitpanda.Period.MINUTES, 1, period, period.AddMinutes(requestMinutes));

                    if (bp_cancleSticksDTO.Any())
                    {
                        log.LogInformation($"- Fetch successfull, {bp_cancleSticksDTO.Count()} new candlesticks found");
                        var cancleSticksDTO = _mapper.Map<List<CandleStickDTO>>(bp_cancleSticksDTO);
                        await _candleStickService.AddCandleSticksAsync(cancleSticksDTO);
                        log.LogInformation($"- Data save successfull");
                    }
                    else
                    {
                        log.LogInformation($"- Fetch successfull, no new data found, stop sync");
                        syncingData = false;
                    }
                }

                log.LogInformation($"Syncing waffle candle stick data finished");
            }
        }

        [FunctionName("WafflerTradeAnalyser")]
        public async Task RunWafflerTradeAnalyserAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            if (IsFunctionActive("WafflerTradeAnalyser"))
            {
                log.LogInformation($"Syncing waffle candle stick data");
            var lastCandleStick = await _candleStickService.GetLastCandleStickAsync();
            var tradeRules = await _tradeRuleService.GetTradeRulesAsync();
                foreach (var tradeRule in tradeRules)
                {
                    var conditionResult = new Dictionary<int, bool>();

                    foreach (var condition in tradeRule.TradeRuleConditions)
                    {
                        var trends = await _candleStickService.GetPriceTrendsAsync(
                            lastCandleStick.PeriodDateTime,
                            condition.FromMinutesOffset,
                            condition.ToMinutesOffset,
                            condition.FromMinutesSample,
                            condition.ToMinutesSample);
                        var value = GetTargetValue(condition, trends);
                        conditionResult.Add(condition.Id, EvaluateCondition(condition, value));
                    }

                    var ruleFullfilled = EvaluateRule(conditionResult, (TradeConditionOperator)tradeRule.TradeConditionOperatorId);

                    if (ruleFullfilled)
                    {
                        switch ((TradeAction)tradeRule.TradeActionId)
                        {
                            case TradeAction.Buy:
                                //await _bitpandaService.CreateOrderAsync(new CreateOrderDTO(){});
                                await _tradeOrderService.CreateTradeOrder(new TradeOrderDTO()
                                {
                                    Amount = tradeRule.Amount,
                                    InstrumentCode = Bitpanda.GetInstrumentCode((TradeType)tradeRule.TradeTypeId),
                                    OrderDateTime = DateTime.UtcNow,
                                    Price = lastCandleStick.HighPrice,
                                    TradeRuleId = tradeRule.Id,
                                    OrderId = Guid.NewGuid(),
                                    TradeOrderStatusId = (short)TradeOrderStatus.Open
                                });
                                break;
                            case TradeAction.Sell:
                                throw new NotImplementedException();
                        }
                    }
                }
            }
        }

        private bool EvaluateRule(Dictionary<int, bool> conditionResult, TradeConditionOperator tradeConditionOperator)
        {
            switch (tradeConditionOperator)
            {
                case TradeConditionOperator.AND:
                    return conditionResult.All(_ => _.Value == true);
                case TradeConditionOperator.OR:
                    return conditionResult.Any(_ => _.Value == true);
                default:
                    break;
            }

            return false;
        }

        private bool EvaluateCondition(TradeRuleConditionDTO condition, decimal value)
        {
            switch ((ConditionComparator)condition.ConditionComparatorId)
            {
                case ConditionComparator.MoreThen:
                    return value > condition.DeltaPercent;
                case ConditionComparator.LessThen:
                    return value < condition.DeltaPercent;
                case ConditionComparator.AbsMoreThen:
                    return Math.Abs(value) > condition.DeltaPercent;
                case ConditionComparator.AbsLessThen:
                    return Math.Abs(value) < condition.DeltaPercent;
                default:
                    break;
            }

            return false;
        }

        private decimal GetTargetValue(TradeRuleConditionDTO condition, PriceTrendsDTO trends)
        {
            switch ((CandleStickValueType)condition.CandleStickValueTypeId)
            {
                case CandleStickValueType.HighPrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.LowPrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.OpenPrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.ClosePrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.AvgHighLowPrice:
                    return trends.HighPriceTrend;
                case CandleStickValueType.AvgOpenClosePrice:
                    return trends.HighPriceTrend;
                default:
                    break;
            }

            return default;
        }
    }
}