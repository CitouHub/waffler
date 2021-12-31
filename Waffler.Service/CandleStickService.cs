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
        Task<List<CandleStickDTO>> GetCandleSticksAsync(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeType, int periodMinutes);
        Task<CandleStickDTO> GetLastCandleStickAsync(DateTime toPeriodDateTime);
        Task<CandleStickDTO> GetFirstCandleStickAsync(DateTime toPeriodDateTime);
        Task ResetCandleStickDataAsync();
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
            _logger.LogDebug("CandleStickService instantiated");
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

        public async Task<List<CandleStickDTO>> GetCandleSticksAsync(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeType, int periodMinutes)
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

        public async Task ResetCandleStickDataAsync()
        {
            await _context.TruncateTable(nameof(CandleStick));
        }
    }
}
