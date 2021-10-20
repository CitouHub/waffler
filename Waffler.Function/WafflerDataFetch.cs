using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Waffler.Common;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Function.Util;
using static Waffler.Common.Variable;

namespace Waffler.Function
{
    public class WafflerDataFetch
    {
        private readonly IMapper _mapper;
        private readonly IBitpandaService _bitpandaService;
        private readonly ICandleStickService _candleStickService;

        public WafflerDataFetch(
            IMapper mapper,
            IBitpandaService bitpandaService,
            ICandleStickService candleStickService)
        {
            _mapper = mapper;
            _bitpandaService = bitpandaService;
            _candleStickService = candleStickService;
        }

        [FunctionName("WafflerDataFetch")]
        [DebugDisable]
        public async Task RunWafflerDataFetchAsync([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Syncing waffle candle stick data");
            var syncingData = true;
            var requestMinutes = 840;
            var defaultStart = -60 * 24 * 90; //If no data exists then start 30 days back
            var requestCount = 0;
            var requestMinuteLimit = 180;
            var startTime = DateTime.UtcNow;

            while (syncingData)
            {
                var period = (await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow))?.PeriodDateTime ?? DateTime.UtcNow.AddMinutes(defaultStart);
                period = period.AddMinutes(1);
                log.LogInformation($"- Fetch data from {period} onward");

                var bp_cancleSticksDTO = await _bitpandaService.GetCandleSticks(
                    Bitpanda.GetInstrumentCode(TradeType.BTC_EUR),
                    Bitpanda.Period.MINUTES, 1, period, period.AddMinutes(requestMinutes));
                requestCount++;

                if(bp_cancleSticksDTO != null)
                {
                    if (bp_cancleSticksDTO.Any())
                    {
                        log.LogInformation($"- Fetch successfull, {bp_cancleSticksDTO.Count()} new candlesticks found");
                        var cancleSticksDTO = _mapper.Map<List<CandleStickDTO>>(bp_cancleSticksDTO);
                        await _candleStickService.AddCandleSticksAsync(cancleSticksDTO);
                        log.LogInformation($"- Data save successfull");
                    }
                    else
                    {
                        log.LogInformation($"- Fetch successfull, no new data found, stop sync");
                        syncingData = false;
                    }
                }
                else
                {
                    log.LogInformation($"- Fetch failed, API unavailable");
                    syncingData = false;
                }

                if(requestCount >= requestMinuteLimit)
                {
                    var sleepTime = 60 * 1000 - (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                    log.LogInformation($"- Reached request limit, sleep {sleepTime} ms");
                    Thread.Sleep(sleepTime <= 0 ? 0 : sleepTime);
                    startTime = DateTime.UtcNow;
                    requestCount = 0;
                }
            }

            log.LogInformation($"Syncing waffle candle stick data finished");
        }
    }
}
