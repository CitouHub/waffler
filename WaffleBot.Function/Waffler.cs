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
    public class Waffler
    {
        private readonly IMapper _mapper;
        private readonly IBitpandaService _bitpandaService;
        private readonly ICandleStickService _candleStickService;
        private readonly ITradeRuleService _tradeRuleService;
        private readonly IProfileService _profileService;
        private readonly ITradeOrderService _tradeOrderService;

        private readonly Dictionary<string, bool> FunctionActive = new Dictionary<string, bool>()
        {
            {"WafflerDataFetch", false },
            {"WafflerTradeAnalyser", false },
            {"WafflerTradeAnalyserReplay", true }
        };

        public Waffler(
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
            if (IsFunctionActive("WafflerDataFetch"))
            {
                log.LogInformation($"Syncing waffle candle stick data");
                var syncingData = true;
                var requestMinutes = 120;
                var defaultStart = -60 * 24 * 30; //If no data exists then start 30 days back

                while (syncingData)
                {
                    var period = (await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow))?.PeriodDateTime ?? DateTime.UtcNow.AddMinutes(defaultStart);
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

        [FunctionName("WafflerTradeAnalyserReplay")]
        public async Task RunWafflerTradeAnalyserReplayAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            if (IsFunctionActive("WafflerTradeAnalyserReplay"))
            {
                var toPeriodDateTime = new DateTime(2021, 10, 6, 0, 0, 0, DateTimeKind.Utc);
                var results = new List<TradeRuleEvaluationDTO>();
                while(toPeriodDateTime < DateTime.UtcNow)
                {
                    var result = await HandleTradeRules(toPeriodDateTime, log);
                    results.AddRange(result);
                    toPeriodDateTime = toPeriodDateTime.AddMinutes(15);
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
        }

        [FunctionName("WafflerTradeAnalyser")]
        public async Task RunWafflerTradeAnalyserAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            if (IsFunctionActive("WafflerTradeAnalyser"))
            {
                log.LogInformation($"Syncing waffle candle stick data");
                await HandleTradeRules(DateTime.UtcNow, log);
            }
        }

        private async Task<List<TradeRuleEvaluationDTO>> HandleTradeRules(DateTime toPeriodDate, ILogger log)
        {
            var candleStick = await _candleStickService.GetLastCandleStickAsync(toPeriodDate);
            var tradeRules = (await _tradeRuleService.GetTradeRulesAsync())
                .Where(_ => _.IsActive == true && _.LastTrigger < candleStick.PeriodDateTime.AddMinutes(-1 * _.TradeMinIntervalMinutes));
            var result = new List<TradeRuleEvaluationDTO>();
            
            foreach (var tradeRule in tradeRules)
            {
                log.LogInformation($"Analysing \"{tradeRule.Name}\" at {toPeriodDate:yyyy-MM-dd HH:mm:ss}");
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
                        toPeriodDate,
                        (TradeRuleConditionSampleDirection) condition.TradeRuleConditionSampleDirectionId,
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
                        var value = GetTargetValue(condition, trends);
                        conditionResult.IsFullfilled = EvaluateCondition(condition, value);
                    }

                    tradeRuleResult.TradeRuleCondtionEvaluations.Add(conditionResult);
                }

                var tradeConditionOperator = (TradeConditionOperator)tradeRule.TradeConditionOperatorId;
                log.LogInformation($" - Evaluating result ({tradeConditionOperator}) {tradeRuleResult}");
                var ruleFullfilled = EvaluateRule(tradeRuleResult.TradeRuleCondtionEvaluations, tradeConditionOperator);
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
                                OrderDateTime = toPeriodDate,
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

        private bool EvaluateRule(List<TradeRuleConditionEvaluationDTO> conditionResult, TradeConditionOperator tradeConditionOperator)
        {
            if(conditionResult.Any() == false)
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

        private bool EvaluateCondition(TradeRuleConditionDTO condition, decimal value)
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

        private decimal GetTargetValue(TradeRuleConditionDTO condition, PriceTrendsDTO trends)
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