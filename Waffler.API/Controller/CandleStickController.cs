using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.API.Security;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Service.Infrastructure;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    [ApiKey]
    public class CandleStickController : ControllerBase
    {
        private readonly ICandleStickService _candleStickService;
        private readonly ICandleStickSyncSignal _candleStickSyncSignal;

        public CandleStickController(ICandleStickService candleStickService, ICandleStickSyncSignal candleStickSyncSignal)
        {
            _candleStickService = candleStickService;
            _candleStickSyncSignal = candleStickSyncSignal;
        }

        [HttpGet]
        [Route("{from}/{to}/{tradeType}/{periodMinutes}")]
        public async Task<IEnumerable<CandleStickDTO>> GetCandleSticksAsync(DateTime from, DateTime to, Common.Variable.TradeType tradeType, short periodMinutes)
        {
            return await _candleStickService.GetCandleSticksAsync(from, to, tradeType, periodMinutes);
        }

        [HttpGet]
        [Route("period/first")]
        public async Task<DateTime?> GetCandleSticksAsync()
        {
            var firstCandleStick = await _candleStickService.GetFirstCandleStickAsync(DateTime.MinValue);
            return firstCandleStick?.PeriodDateTime;
        }

        [HttpPost]
        [Route("sync/reset")]
        public async Task ResetCandleStickDataAsync()
        {
            if(_candleStickSyncSignal.IsActive())
            {
                _candleStickSyncSignal.Abort();
                await _candleStickSyncSignal.AwaitAbortAsync();
            }
            await _candleStickService.ResetCandleStickDataAsync();
        }

        [HttpGet]
        [Route("sync/status")]
        public async Task<CandleStickSyncStatusDTO> GetCandleStickSyncStatucAsync()
        {
            return new CandleStickSyncStatusDTO()
            {
                ThrottlePaused = _candleStickSyncSignal.IsThrottlePaused(),
                FirstPeriodDateTime = (await _candleStickService.GetFirstCandleStickAsync(DateTime.MinValue))?.PeriodDateTime,
                LastPeriodDateTime = (await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow))?.PeriodDateTime
            };
        }
    }
}
