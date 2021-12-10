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
        Task<decimal?> GetPriceTrendAsync(DateTime currentPeriodDateTime, Variable.TradeType tradeType, TradeRuleConditionDTO tradeRuleCondition);
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

        public async Task<decimal?> GetPriceTrendAsync(DateTime currentPeriodDateTime, 
            Variable.TradeType tradeType,
            TradeRuleConditionDTO tradeRuleCondition)
        {
            var fromPeriod = GetPeriod((Variable.TradeRuleConditionPeriodDirection)tradeRuleCondition.FromTradeRuleConditionPeriodDirectionId,
                currentPeriodDateTime.AddMinutes(tradeRuleCondition.FromMinutes), tradeRuleCondition.FromPeriodMinutes);

            var toPeriod = GetPeriod((Variable.TradeRuleConditionPeriodDirection)tradeRuleCondition.ToTradeRuleConditionPeriodDirectionId,
                currentPeriodDateTime.AddMinutes(tradeRuleCondition.ToMinutes), tradeRuleCondition.ToPeriodMinutes);


            var candleSticksDTO = _cache.Get<List<CandleStickDTO>, DateTime?>(MethodBase.GetCurrentMethod().Name, out DateTime? cacheFromPeriodDateTime);
            if (cacheFromPeriodDateTime == null || fromPeriod.From < cacheFromPeriodDateTime)
            {
                candleSticksDTO = await GetCandleSticksAsync(fromPeriod.From, DateTime.UtcNow, tradeType, 1);
                _cache.Set(MethodBase.GetCurrentMethod().Name, candleSticksDTO, fromPeriod.From);
            }

            var fromCandleStick = candleSticksDTO.Where(_ => _.PeriodDateTime >= fromPeriod.From && _.PeriodDateTime <= fromPeriod.To)
                .GroupBy(_ => "From")
                .Select(_ => new CandleStickDTO()
                {
                    HighPrice = _.Max(p => p.HighPrice),
                    LowPrice = _.Min(p => p.LowPrice),
                    OpenPrice = candleSticksDTO.Where(_ => _.PeriodDateTime >= fromPeriod.From && _.PeriodDateTime <= fromPeriod.To).OrderBy(_ => _.PeriodDateTime).FirstOrDefault().OpenPrice,
                    ClosePrice = candleSticksDTO.Where(_ => _.PeriodDateTime >= fromPeriod.From && _.PeriodDateTime <= fromPeriod.To).OrderByDescending(_ => _.PeriodDateTime).FirstOrDefault().ClosePrice,
                }).FirstOrDefault();

            var toCandleStick = candleSticksDTO.Where(_ => _.PeriodDateTime >= toPeriod.From && _.PeriodDateTime <= toPeriod.To)
                .GroupBy(_ => "To")
                .Select(_ => new CandleStickDTO()
                {
                    HighPrice = _.Max(p => p.HighPrice),
                    LowPrice = _.Min(p => p.LowPrice),
                    OpenPrice = candleSticksDTO.Where(_ => _.PeriodDateTime >= toPeriod.From && _.PeriodDateTime <= toPeriod.To).OrderBy(_ => _.PeriodDateTime).FirstOrDefault().OpenPrice,
                    ClosePrice = candleSticksDTO.Where(_ => _.PeriodDateTime >= toPeriod.From && _.PeriodDateTime <= toPeriod.To).OrderByDescending(_ => _.PeriodDateTime).FirstOrDefault().ClosePrice,
                }).FirstOrDefault();

            var fromValue = GetValue((Variable.CandleStickValueType)tradeRuleCondition.FromCandleStickValueTypeId, fromCandleStick);
            var toValue = GetValue((Variable.CandleStickValueType)tradeRuleCondition.ToCandleStickValueTypeId, toCandleStick);

            if(fromValue != null && toValue != null)
            {
                return Math.Round((1 - fromValue.Value / toValue.Value) * 100, 4);
            }

            return null;
        }

        private PeriodDTO GetPeriod(Variable.TradeRuleConditionPeriodDirection direction, DateTime dateTime, int minutes)
        {
            var fromDateTime = dateTime;
            var toDateTime = dateTime;

            switch (direction)
            {
                case Variable.TradeRuleConditionPeriodDirection.Centered:
                    fromDateTime = fromDateTime.AddMinutes(-1 * minutes / 2);
                    toDateTime = toDateTime.AddMinutes(minutes / 2);
                    break;
                case Variable.TradeRuleConditionPeriodDirection.RightShift:
                    toDateTime = toDateTime.AddMinutes(minutes);
                    break;
                case Variable.TradeRuleConditionPeriodDirection.LeftShift:
                    fromDateTime = fromDateTime.AddMinutes(-1 * minutes);
                    break;
            }

            return new PeriodDTO()
            {
                From = fromDateTime,
                To = toDateTime
            };
        }

        private decimal? GetValue(Variable.CandleStickValueType candleStickValueType, CandleStickDTO candleStick)
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

        public async Task ResetCandleStickSyncAsync()
        {
            await _context.TruncateTable(nameof(CandleStick));
        }
    }
}
