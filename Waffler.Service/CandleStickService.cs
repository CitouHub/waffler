using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Waffler.Domain;
using Waffler.Data;
using Waffler.Common;
using Waffler.Common.Util;
using Waffler.Data.Extensions;

namespace Waffler.Service
{
    public interface ICandleStickService
    {
        Task AddCandleSticksAsync(List<CandleStickDTO> candleSticks);
        Task<List<CandleStickDTO>> GetCandleSticksAsync(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeType, short periodMinutes);
        Task<CandleStickDTO> GetLastCandleStickAsync(DateTime toPeriodDateTime);
        Task<CandleStickDTO> GetFirstCandleStickAsync(DateTime toPeriodDateTime);
        Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime currentPeriodDateTime, Variable.TradeType tradeType, Variable.TradeRuleConditionSampleDirection sampleDirection, int fromMinutesOffset, int toMinutesOffset, int fromMinutesSample, int toMinutesSample);
        Task ResetCandleStickSyncAsync();
    }

    public class CandleStickService : ICandleStickService
    {
        private readonly ILogger<CandleStickService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;
        private readonly Cache _cache;

        public CandleStickService(ILogger<CandleStickService> logger, WafflerDbContext context, IMapper mapper, Cache cache)
        {
            _logger = logger;
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

            await _context.CandleSticks.AddRangeAsync(newCandleSticks);
            await _context.SaveChangesAsync();
        }

        public async Task<List<CandleStickDTO>> GetCandleSticksAsync(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeType, short periodMinutes)
        {
            var candleSticks = await _context.sp_getCandleSticks(fromPeriodDateTime, toPeriodDateTime, (short)tradeType, periodMinutes);
            return _mapper.Map<List<CandleStickDTO>>(candleSticks);
        }

        public async Task<CandleStickDTO> GetLastCandleStickAsync(DateTime toPeriodDateTime)
        {
            var candleStick = await _context.CandleSticks.Where(_ => _.PeriodDateTime <= toPeriodDateTime)
                .OrderByDescending(_ => _.PeriodDateTime).FirstOrDefaultAsync();
            return _mapper.Map<CandleStickDTO>(candleStick);
        }

        public async Task<CandleStickDTO> GetFirstCandleStickAsync(DateTime fromPeriodDateTime)
        {
            var candleStick = await _context.CandleSticks.Where(_ => _.PeriodDateTime >= fromPeriodDateTime)
                .OrderBy(_ => _.PeriodDateTime).FirstOrDefaultAsync();
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
                case Variable.TradeRuleConditionSampleDirection.Centered:
                    fromFromDateTime = fromFromDateTime.AddMinutes(-1 * fromMinutesSample / 2);
                    fromToDateTime = fromToDateTime.AddMinutes(fromMinutesSample / 2);
                    toFromDateTime = toFromDateTime.AddMinutes(-1 * toMinutesSample);
                    break;
                case Variable.TradeRuleConditionSampleDirection.RightShift:
                    fromToDateTime = fromToDateTime.AddMinutes(fromMinutesSample);
                    toFromDateTime = toFromDateTime.AddMinutes(-1 * toMinutesSample);
                    break;
                case Variable.TradeRuleConditionSampleDirection.LeftShift:
                    fromFromDateTime = fromFromDateTime.AddMinutes(-1 * fromMinutesSample);
                    toFromDateTime = toFromDateTime.AddMinutes(-1 * toMinutesSample);
                    break;
            }

            var candleSticksDTO = _cache.Get<List<CandleStickDTO>, DateTime?>(MethodBase.GetCurrentMethod().Name, out DateTime? cacheFromPeriodDateTime);
            if (cacheFromPeriodDateTime == null || fromFromDateTime < cacheFromPeriodDateTime)
            {
                candleSticksDTO = await GetCandleSticksAsync(fromFromDateTime, DateTime.UtcNow, Variable.TradeType.BTC_EUR, 1);
                _cache.Set(MethodBase.GetCurrentMethod().Name, candleSticksDTO, fromFromDateTime);
            }

            var fromCandleSticks = candleSticksDTO.Where(_ => _.PeriodDateTime >= fromFromDateTime && _.PeriodDateTime <= fromToDateTime)
                .GroupBy(_ => "From")
                .Select(_ => new
                {
                    HighPrice = _.Max(p => p.HighPrice),
                    LowPrice = _.Min(p => p.LowPrice),
                    candleSticksDTO.Where(_ => _.PeriodDateTime >= fromFromDateTime && _.PeriodDateTime <= fromToDateTime).OrderBy(_ => _.PeriodDateTime).FirstOrDefault().OpenPrice,
                    candleSticksDTO.Where(_ => _.PeriodDateTime >= fromFromDateTime && _.PeriodDateTime <= fromToDateTime).OrderByDescending(_ => _.PeriodDateTime).FirstOrDefault().ClosePrice,
                    AvgHighLowPrice = _.Average(p => p.AvgHighLowPrice),
                    AvgOpenClosePrice = _.Average(p => p.AvgOpenClosePrice)
                }).FirstOrDefault();

            var toCandleSticks = candleSticksDTO.Where(_ => _.PeriodDateTime >= toFromDateTime && _.PeriodDateTime <= toToDateTime)
                .GroupBy(_ => "To")
                .Select(_ => new
                {
                    HighPrice = _.Max(p => p.HighPrice),
                    LowPrice = _.Min(p => p.LowPrice),
                    candleSticksDTO.Where(_ => _.PeriodDateTime >= toFromDateTime && _.PeriodDateTime <= toToDateTime).OrderBy(_ => _.PeriodDateTime).FirstOrDefault().OpenPrice,
                    candleSticksDTO.Where(_ => _.PeriodDateTime >= toFromDateTime && _.PeriodDateTime <= toToDateTime).OrderByDescending(_ => _.PeriodDateTime).FirstOrDefault().ClosePrice,
                    AvgHighLowPrice = _.Average(p => p.AvgHighLowPrice),
                    AvgOpenClosePrice = _.Average(p => p.AvgOpenClosePrice)
                }).FirstOrDefault();

            if(fromCandleSticks != null && toCandleSticks != null)
            {
                return new PriceTrendsDTO()
                {
                    HighPriceTrend = Math.Round((1 - fromCandleSticks.HighPrice / toCandleSticks.HighPrice) * 100, 4),
                    LowPriceTrend = Math.Round((1 - fromCandleSticks.LowPrice / toCandleSticks.LowPrice) * 100, 4),
                    OpenPriceTrend = Math.Round((1 - fromCandleSticks.OpenPrice / toCandleSticks.OpenPrice) * 100, 4),
                    ClosePriceTrend = Math.Round((1 - fromCandleSticks.ClosePrice / toCandleSticks.ClosePrice) * 100, 4),
                    HighLowPriceTrend = Math.Round((1 - fromCandleSticks.HighPrice / toCandleSticks.LowPrice) * 100, 4),
                    OpenClosePriceTrend = Math.Round((1 - fromCandleSticks.OpenPrice / toCandleSticks.ClosePrice) * 100, 4),
                    AvgHighLowPriceTrend = Math.Round((1 - fromCandleSticks.AvgHighLowPrice.Value / toCandleSticks.AvgHighLowPrice.Value) * 100, 4),
                    AvgOpenClosePriceTrend = Math.Round((1 - fromCandleSticks.AvgOpenClosePrice.Value / toCandleSticks.AvgOpenClosePrice.Value) * 100, 4),
                };
            }

            return null;
        }

        public async Task ResetCandleStickSyncAsync()
        {
            await _context.TruncateTable(nameof(CandleStick));
        }
    }
}
