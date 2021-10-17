using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using Waffler.Domain;
using Waffler.Data;
using Waffler.Common;
using Waffler.Common.Util;

namespace Waffler.Service
{
    public interface ICandleStickService
    {
        Task AddCandleSticksAsync(List<CandleStickDTO> candleSticks);
        Task<CandleStickDTO> GetLastCandleStickAsync(DateTime toPeriodDateTime);
        Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime currentPeriodDateTime, Variable.TradeType tradeType, Variable.TradeRuleConditionSampleDirection sampleDirection, int fromMinutesOffset, int toMinutesOffset, int fromMinutesSample, int toMinutesSample);
    }

    public class CandleStickService : ICandleStickService
    {
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;
        private readonly Cache _cache;

        public CandleStickService(WafflerDbContext context, IMapper mapper, Cache cache)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task AddCandleSticksAsync(List<CandleStickDTO> candleSticks)
        {
            var newCandleSticks = _mapper.Map<List<CandleStick>>(candleSticks);
            newCandleSticks.ForEach(_ =>
            {
                _.InsertByUser = 1;
                _.InsertDate = DateTime.UtcNow;
            });

            await _context.CandleStick.AddRangeAsync(newCandleSticks);
            await _context.SaveChangesAsync();
        }

        public async Task<CandleStickDTO> GetLastCandleStickAsync(DateTime toPeriodDateTime)
        {
            var candleStick = await _context.CandleStick.Where(_ => _.PeriodDateTime <= toPeriodDateTime)
                .OrderByDescending(_ => _.PeriodDateTime).FirstOrDefaultAsync();
            return _mapper.Map<CandleStickDTO>(candleStick);
        }

        public async Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime currentPeriodDateTime, 
            Variable.TradeType tradeType,
            Variable.TradeRuleConditionSampleDirection sampleDirection, 
            int fromMinutesOffset, 
            int toMinutesOffset, 
            int fromMinutesSample, 
            int toMinutesSample)
        {
            var fromFromDateTime = currentPeriodDateTime.AddMinutes(fromMinutesOffset);
            var fromToDateTime = currentPeriodDateTime.AddMinutes(fromMinutesOffset);
            var toFromDateTime = currentPeriodDateTime.AddMinutes(toMinutesOffset);
            var toToDateTime = currentPeriodDateTime.AddMinutes(toMinutesOffset);

            switch (sampleDirection)
            {
                case Variable.TradeRuleConditionSampleDirection.Inward:
                    fromToDateTime = fromToDateTime.AddMinutes(fromMinutesSample);
                    toFromDateTime = toFromDateTime.AddMinutes(-1 * toMinutesSample);
                    break;
                case Variable.TradeRuleConditionSampleDirection.Outward:
                    fromFromDateTime = fromFromDateTime.AddMinutes(-1 * fromMinutesSample);
                    toToDateTime = toToDateTime.AddMinutes(toMinutesSample);
                    break;
                case Variable.TradeRuleConditionSampleDirection.LeftShift:
                    fromFromDateTime = fromFromDateTime.AddMinutes(-1 * fromMinutesSample);
                    fromToDateTime = fromToDateTime.AddMinutes(-1 * toMinutesSample);
                    break;
                case Variable.TradeRuleConditionSampleDirection.RightShift:
                    toFromDateTime = toFromDateTime.AddMinutes(fromMinutesSample);
                    toToDateTime = toToDateTime.AddMinutes(toMinutesSample);
                    break;
                case Variable.TradeRuleConditionSampleDirection.Centered:
                    fromFromDateTime = fromFromDateTime.AddMinutes(-1 * fromMinutesSample / 2);
                    fromToDateTime = fromToDateTime.AddMinutes(fromMinutesSample / 2);
                    toFromDateTime = toFromDateTime.AddMinutes(-1 * toMinutesSample / 2);
                    toToDateTime = toToDateTime.AddMinutes(toMinutesSample / 2);
                    break;
            }

            var candleSticksDTO = _cache.Get<List<CandleStickDTO>, DateTime?>(out DateTime? cacheFromPeriodDateTime);
            if (cacheFromPeriodDateTime == null || fromFromDateTime < cacheFromPeriodDateTime)
            {
                var candleSticks = await _context.CandleStick.Where(_ => _.PeriodDateTime >= fromFromDateTime).ToArrayAsync();
                candleSticksDTO = _mapper.Map<List<CandleStickDTO>>(candleSticks);
                _cache.Set(candleSticksDTO, fromFromDateTime);
            }

            var fromCandleSticks = candleSticksDTO.Where(_ => _.PeriodDateTime >= fromFromDateTime && _.PeriodDateTime <= fromToDateTime && _.TradeTypeId == (short)tradeType)
                .GroupBy(_ => _.TradeTypeId)
                .Select(_ => new
                {
                    AvgHighPrice = _.Average(p => p.HighPrice),
                    AvgLowPrice = _.Average(p => p.LowPrice),
                    AvgOpenPrice = _.Average(p => p.OpenPrice),
                    AvgClosePrice = _.Average(p => p.ClosePrice),
                    AvgAvgHighLowPrice = _.Average(p => p.AvgHighLowPrice),
                    AvgAvgOpenClosePrice = _.Average(p => p.AvgOpenClosePrice)
                }).FirstOrDefault();

            var toCandleSticks = candleSticksDTO.Where(_ => _.PeriodDateTime >= toFromDateTime && _.PeriodDateTime <= toToDateTime && _.TradeTypeId == (short)tradeType)
                .GroupBy(_ => _.TradeTypeId)
                .Select(_ => new
                {
                    AvgHighPrice = _.Average(p => p.HighPrice),
                    AvgLowPrice = _.Average(p => p.LowPrice),
                    AvgOpenPrice = _.Average(p => p.OpenPrice),
                    AvgClosePrice = _.Average(p => p.ClosePrice),
                    AvgAvgHighLowPrice = _.Average(p => p.AvgHighLowPrice),
                    AvgAvgOpenClosePrice = _.Average(p => p.AvgOpenClosePrice)
                }).FirstOrDefault();

            if(fromCandleSticks != null && toCandleSticks != null)
            {
                return new PriceTrendsDTO()
                {
                    HighPriceTrend = (1 - fromCandleSticks.AvgHighPrice / toCandleSticks.AvgHighPrice) * 100,
                    LowPriceTrend = (1 - fromCandleSticks.AvgLowPrice / toCandleSticks.AvgLowPrice) * 100,
                    OpenPriceTrend = (1 - fromCandleSticks.AvgOpenPrice / toCandleSticks.AvgOpenPrice) * 100,
                    ClosePriceTrend = (1 - fromCandleSticks.AvgClosePrice / toCandleSticks.AvgClosePrice) * 100,
                    HighLowPriceTrend = (1 - fromCandleSticks.AvgHighPrice / toCandleSticks.AvgLowPrice) * 100,
                    OpenClosePriceTrend = (1 - fromCandleSticks.AvgOpenPrice / toCandleSticks.AvgClosePrice) * 100,
                    AvgHighLowPriceTrend = (1 - fromCandleSticks.AvgAvgHighLowPrice / toCandleSticks.AvgAvgHighLowPrice) * 100,
                    AvgOpenClosePriceTrend = (1 - fromCandleSticks.AvgAvgOpenClosePrice / toCandleSticks.AvgAvgOpenClosePrice) * 100,
                };
            }

            return null;
        }
    }
}
