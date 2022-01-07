using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.Extensions.Logging;

using Waffler.Common;
using Waffler.Data;
using Waffler.Data.Extensions;
using Waffler.Domain;
using Waffler.Domain.Statistics;

namespace Waffler.Service
{
    public interface IStatisticsService
    {
        Task<List<TradeRuleBuyStatisticsDTO>> GetTradeRuleBuyStatisticsAsync(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.StatisticsMode statisticsMode);
        Task<TrendDTO> GetPriceTrendAsync(DateTime currentPeriodDateTime, Variable.TradeType tradeType, TradeRuleConditionDTO tradeRuleCondition);
        Task<TrendDTO> GetPriceTrendAsync(Variable.TradeType tradeType,
            Variable.CandleStickValueType fromCandleStickValueTypeId, DateTime fromFromDate, DateTime toFromDate,
            Variable.CandleStickValueType toCandleStickValueTypeId, DateTime fromToDate, DateTime toToDate);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly ILogger<StatisticsService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICandleStickService _candleStickService;

        public StatisticsService(ILogger<StatisticsService> logger, WafflerDbContext context, IMapper mapper, ICandleStickService candleStickService)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _candleStickService = candleStickService;
            _logger.LogDebug("StatisticsService instantiated");
        }

        public async Task<List<TradeRuleBuyStatisticsDTO>> GetTradeRuleBuyStatisticsAsync(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.StatisticsMode statisticsMode)
        {
            var statistics = await _context.sp_getTradeRuleBuyStatistics(fromPeriodDateTime, toPeriodDateTime, (short)statisticsMode);
            return _mapper.Map<List<TradeRuleBuyStatisticsDTO>>(statistics);
        }

        public async Task<TrendDTO> GetPriceTrendAsync(Variable.TradeType tradeType,
            Variable.CandleStickValueType fromCandleStickValueTypeId, DateTime fromFromDate, DateTime toFromDate,
            Variable.CandleStickValueType toCandleStickValueTypeId, DateTime fromToDate, DateTime toToDate)
        {
            var from = await _candleStickService.GetCandleSticksAsync(fromFromDate, toFromDate, Variable.TradeType.BTC_EUR, (int)(toFromDate - fromFromDate).TotalMinutes);
            var to = from;
            if(fromFromDate != fromToDate || toFromDate != toToDate)
            {
                to = await _candleStickService.GetCandleSticksAsync(fromToDate, toToDate, Variable.TradeType.BTC_EUR, (int)(toFromDate - fromFromDate).TotalMinutes);
            }

            var fromPrice = GetPrice(fromCandleStickValueTypeId, from.FirstOrDefault());
            var toPrice = GetPrice(toCandleStickValueTypeId, to.FirstOrDefault());

            if (fromPrice != null && toPrice != null)
            {
                return new TrendDTO
                {
                    FromPrice = fromPrice.Value,
                    ToPrice = toPrice.Value
                };
            }

            return null;
        }

        public Task<TrendDTO> GetPriceTrendAsync(DateTime currentPeriodDateTime, Variable.TradeType tradeType, TradeRuleConditionDTO tradeRuleCondition)
        {
            var fromPeriod = GetPeriod((Variable.TradeRuleConditionPeriodDirection)tradeRuleCondition.FromTradeRuleConditionPeriodDirectionId,
                currentPeriodDateTime.AddMinutes(tradeRuleCondition.FromMinutes), tradeRuleCondition.FromPeriodMinutes);

            var toPeriod = GetPeriod((Variable.TradeRuleConditionPeriodDirection)tradeRuleCondition.ToTradeRuleConditionPeriodDirectionId,
                currentPeriodDateTime.AddMinutes(tradeRuleCondition.ToMinutes), tradeRuleCondition.ToPeriodMinutes);

            return GetPriceTrendAsync(tradeType,
                (Variable.CandleStickValueType)tradeRuleCondition.FromCandleStickValueTypeId, fromPeriod.From, fromPeriod.To,
                (Variable.CandleStickValueType)tradeRuleCondition.ToCandleStickValueTypeId, toPeriod.From, toPeriod.To);
        }

        public PeriodDTO GetPeriod(Variable.TradeRuleConditionPeriodDirection direction, DateTime dateTime, int periodMinutes)
        {
            var fromDateTime = dateTime;
            var toDateTime = dateTime;

            switch (direction)
            {
                case Variable.TradeRuleConditionPeriodDirection.Centered:
                    fromDateTime = fromDateTime.AddMinutes(-1 * periodMinutes / 2);
                    toDateTime = toDateTime.AddMinutes(periodMinutes / 2);
                    break;
                case Variable.TradeRuleConditionPeriodDirection.RightShift:
                    toDateTime = toDateTime.AddMinutes(periodMinutes);
                    break;
                case Variable.TradeRuleConditionPeriodDirection.LeftShift:
                    fromDateTime = fromDateTime.AddMinutes(-1 * periodMinutes);
                    break;
            }

            return new PeriodDTO()
            {
                From = fromDateTime,
                To = toDateTime
            };
        }

        public decimal? GetPrice(Variable.CandleStickValueType candleStickValueType, CandleStickDTO candleStick)
        {
            return candleStickValueType switch
            {
                Variable.CandleStickValueType.HighPrice => candleStick?.HighPrice,
                Variable.CandleStickValueType.LowPrice => candleStick?.LowPrice,
                Variable.CandleStickValueType.OpenPrice => candleStick?.OpenPrice,
                Variable.CandleStickValueType.ClosePrice => candleStick?.ClosePrice,
                _ => default,
            };
        }
    }
}