using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoMapper;
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
    }
}
