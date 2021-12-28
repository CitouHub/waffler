using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Waffler.Common;
using Waffler.Data;
using Waffler.Data.Extensions;
using Waffler.Domain.Statistics;

namespace Waffler.Service
{
    public interface IStatisticsService
    {
        Task<List<TradeRuleBuyStatisticsDTO>> GetTradeRuleBuyStatistics(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.StatisticsMode statisticsMode);
        Task<TrendDTO> GetTrend(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeTypeId, int samplePeriodMinues);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly ILogger<StatisticsService> _logger;
        private readonly WafflerDbContext _context;
        private readonly IMapper _mapper;

        public StatisticsService(ILogger<StatisticsService> logger, WafflerDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _logger.LogDebug("StatisticsService instantiated");
        }

        public async Task<List<TradeRuleBuyStatisticsDTO>> GetTradeRuleBuyStatistics(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.StatisticsMode statisticsMode)
        {
            var statistics = await _context.sp_getTradeRuleBuyStatistics(fromPeriodDateTime, toPeriodDateTime, (short)statisticsMode);
            return _mapper.Map<List<TradeRuleBuyStatisticsDTO>>(statistics);
        }

        public async Task<TrendDTO> GetTrend(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeTypeId, int samplePeriodMinues)
        {
            var fromPeriod = await _context.CandleSticks.Where(_ =>
                _.TradeTypeId == (short)tradeTypeId &&
                _.PeriodDateTime >= fromPeriodDateTime &&
                _.PeriodDateTime <= fromPeriodDateTime.AddMinutes(samplePeriodMinues))
                .ToListAsync();

            var toPeriod = await _context.CandleSticks.Where(_ =>
                _.TradeTypeId == (short)tradeTypeId &&
                _.PeriodDateTime >= toPeriodDateTime.AddMinutes(-1 * samplePeriodMinues) &&
                _.PeriodDateTime <= toPeriodDateTime)
                .ToListAsync();

            if(fromPeriod != null && fromPeriod.Any() &&
                toPeriod != null && toPeriod.Any())
            {
                var fromPrice = fromPeriod.Average(_ => (_.OpenPrice + _.ClosePrice) / 2);
                var toPrice = toPeriod.Average(_ => (_.OpenPrice + _.ClosePrice) / 2);
                return new TrendDTO
                {
                    FromPrice = fromPrice,
                    ToPrice = toPrice
                };
            }

            return default;
        }
    }
}
