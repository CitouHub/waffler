using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.API.Security;
using Waffler.Common;
using Waffler.Domain.Statistics;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    [ApiKey]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [Route("buy/{fromPeriodDateTime}/{toPeriodDateTime}/{statisticsMode}")]
        public async Task<List<TradeRuleBuyStatisticsDTO>> GetTradeRuleBuyStatistics(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.StatisticsMode statisticsMode)
        {
            return await _statisticsService.GetTradeRuleBuyStatisticsAsync(fromPeriodDateTime, toPeriodDateTime, statisticsMode);
        }

        [HttpGet]
        [Route("trend/{fromPeriodDateTime}/{toPeriodDateTime}/{tradeTypeId}/{samplePeriodMinues}")]
        public async Task<TrendDTO> GetTrend(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeTypeId, int samplePeriodMinues)
        {
            return await _statisticsService.GetPriceTrendAsync(tradeTypeId,
                Variable.CandleStickValueType.OpenPrice, fromPeriodDateTime, fromPeriodDateTime.AddMinutes(samplePeriodMinues),
                Variable.CandleStickValueType.ClosePrice, toPeriodDateTime.AddMinutes(-1*samplePeriodMinues), toPeriodDateTime);
        }
    }
}