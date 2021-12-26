using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.Domain.Statistics;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [Route("buy/{fromPeriodDateTime}/{toPeriodDateTime}")]
        public async Task<List<BuyTradeRuleStatisticsDTO>> GetBuyTradeRuleStatistics(DateTime fromPeriodDateTime, DateTime toPeriodDateTime)
        {
            return await _statisticsService.GetBuyTradeRuleStatistics(fromPeriodDateTime, toPeriodDateTime);
        }
    }
}
