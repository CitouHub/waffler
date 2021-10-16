using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using WaffleBot.Domain;
using WaffleBot.Data;
using WaffleBot.Data.Extensions;
using WaffleBot.Common;

namespace WaffleBot.Service
{
    public interface ICandleStickService
    {
        Task AddCandleSticksAsync(List<CandleStickDTO> candleSticks);
        Task<CandleStickDTO> GetLastCandleStickAsync(DateTime toPeriodDateTime);
        Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime startDateTime, Variable.TradeRuleConditionSampleDirection sampleDirection, int fromMinutesOffset, int toMinutesOffset, int fromMinutesSample, int toMinutesSample);
    }

    public class CandleStickService : ICandleStickService
    {
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public CandleStickService(WafflerDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime startDateTime, 
            Variable.TradeRuleConditionSampleDirection sampleDirection, 
            int fromMinutesOffset, 
            int toMinutesOffset, 
            int fromMinutesSample, 
            int toMinutesSample)
        {
            var fromFromDateTime = startDateTime.AddMinutes(fromMinutesOffset);
            var fromToDateTime = startDateTime.AddMinutes(fromMinutesOffset);
            var toFromDateTime = startDateTime.AddMinutes(toMinutesOffset);
            var toToDateTime = startDateTime.AddMinutes(toMinutesOffset);

            switch(sampleDirection)
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

            var priceTrends = (await _context.sp_getPriceTrends(fromFromDateTime, fromToDateTime, toFromDateTime, toToDateTime)).FirstOrDefault();
            return _mapper.Map<PriceTrendsDTO>(priceTrends);
        }
    }
}
