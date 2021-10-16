using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using AutoMapper;

using WaffleBot.Domain;
using WaffleBot.Data;
using WaffleBot.Data.Extensions;

namespace WaffleBot.Service
{
    public interface ICandleStickService
    {
        Task AddCandleSticksAsync(List<CandleStickDTO> candleSticks);
        Task<CandleStickDTO> GetLastCandleStickAsync();
        Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime startDateTime, int fromMinutesOffset, int toMinutesOffset, int fromMinutesSample, int toMinutesSample);
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

        public async Task<CandleStickDTO> GetLastCandleStickAsync()
        {
            var candleStick = await _context.CandleStick.OrderByDescending(_ => _.PeriodDateTime).FirstOrDefaultAsync();
            return _mapper.Map<CandleStickDTO>(candleStick);
        }

        public async Task<PriceTrendsDTO> GetPriceTrendsAsync(DateTime startDateTime, int fromMinutesOffset, int toMinutesOffset, int fromMinutesSample, int toMinutesSample)
        {
            var priceTrends = (await _context.sp_getPriceTrends(startDateTime, fromMinutesOffset, toMinutesOffset, fromMinutesSample, toMinutesSample)).FirstOrDefault();
            return _mapper.Map<PriceTrendsDTO>(priceTrends);
        }
    }
}
