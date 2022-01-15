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
        private readonly ICandleStickService _candleStickService;

        public StatisticsController(IStatisticsService statisticsService, ICandleStickService candleStickService)
        {
            _statisticsService = statisticsService;
            _candleStickService = candleStickService;
        }

        [HttpPost]
        [Route("buy/{fromPeriodDateTime}/{toPeriodDateTime}/{tradeTypeId}")]
        public async Task<List<TradeRuleBuyStatisticsDTO>> GetTradeRuleBuyStatistics(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeTypeId, [FromBody] TradeSelectionDTO tradeSelection)
        {
            return await _statisticsService.GetTradeRuleBuyStatisticsAsync(fromPeriodDateTime, toPeriodDateTime, tradeTypeId, tradeSelection);
        }

        [HttpGet]
        [Route("trend/{fromPeriodDateTime}/{toPeriodDateTime}/{tradeTypeId}/{samplePeriodMinues}")]
        public async Task<TrendDTO> GetTrend(DateTime fromPeriodDateTime, DateTime toPeriodDateTime, Variable.TradeType tradeTypeId, int samplePeriodMinues)
        {
            var lastCandleStickAsync = await _candleStickService.GetLastCandleStickAsync(toPeriodDateTime);

            return await _statisticsService.GetPriceTrendAsync(tradeTypeId,
                Variable.CandleStickValueType.OpenPrice, fromPeriodDateTime, fromPeriodDateTime.AddMinutes(samplePeriodMinues),
                Variable.CandleStickValueType.ClosePrice, lastCandleStickAsync.PeriodDateTime.AddMinutes(-1*samplePeriodMinues), lastCandleStickAsync.PeriodDateTime);
        }
    }
}