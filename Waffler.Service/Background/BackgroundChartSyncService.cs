﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using AutoMapper;

using Waffler.Common;
using Waffler.Domain;
using static Waffler.Common.Variable;

namespace Waffler.Service.Background
{
    public class BackgroundChartSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundChartSyncService> _logger;
        private readonly TimeSpan RequestPeriod = TimeSpan.FromMinutes(5);
        private readonly TimeSpan RequestMinutes = TimeSpan.FromHours(4);

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundChartSyncService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundChartSyncService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(async _ => await FetchCandleStickDataAsync(cancellationToken), null, TimeSpan.Zero, RequestPeriod);
            return Task.CompletedTask;
        }

        private async Task FetchCandleStickDataAsync(CancellationToken cancellationToken)
        {
            if (InProgress)
            {
                return;
            }

            InProgress = true;
            try
            {
                _logger.LogInformation($"Syncing candlestick data");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _logger.LogInformation($"- Setting up scoped services");
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    var syncingData = true;
                    var requestCount = 0;
                    var requestMinuteLimit = 180; //This limit is related to the throtteling limit at Bitpanda
                    var saveLimit = 10; //This limit is due to the fact that Bitpanda sometimes return the same candlestick entity when reaching the current time
                    var startTime = DateTime.UtcNow;
                    var profile = await _profileService.GetProfileAsync();

                    while (profile != null && syncingData && cancellationToken.IsCancellationRequested == false)
                    {
                        _logger.LogInformation($"- Getting last candlestick");
                        var period = (await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow))?.PeriodDateTime ??
                            profile.CandleStickSyncFromDate;
                        period = period.AddMilliseconds(1);

                        _logger.LogInformation($"- Fetch data from {period} onward");
                        var bp_cancleSticksDTO = await _bitpandaService.GetCandleSticks(
                            Bitpanda.GetInstrumentCode(TradeType.BTC_EUR),
                            Bitpanda.Period.MINUTES, 1, period, period.AddMinutes(RequestMinutes.TotalMinutes));
                        requestCount++;

                        if (bp_cancleSticksDTO != null)
                        {
                            if (bp_cancleSticksDTO.Count() >= saveLimit)
                            {
                                _logger.LogInformation($"- Fetch successfull, {bp_cancleSticksDTO.Count()} new candlesticks found");
                                var cancleSticksDTO = _mapper.Map<List<CandleStickDTO>>(bp_cancleSticksDTO);
                                await _candleStickService.AddCandleSticksAsync(cancleSticksDTO);
                                _logger.LogInformation($"- Data save successfull");
                            }
                            else
                            {
                                _logger.LogInformation($"- Fetch successfull, no new data found, stop sync");
                                syncingData = false;
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"- Fetch failed, API unavailable");
                            syncingData = false;
                        }

                        if (requestCount >= requestMinuteLimit)
                        {
                            var sleepTime = 60 * 1000 - (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                            _logger.LogInformation($"- Reached request limit, sleep {sleepTime} ms");
                            Thread.Sleep(sleepTime <= 0 ? 0 : sleepTime);
                            startTime = DateTime.UtcNow;
                            requestCount = 0;
                        }

                        _timer.Change(RequestPeriod, RequestPeriod);
                    }
                }
                _logger.LogInformation($"Syncing candlestick data finished");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception", e);
            }
            InProgress = false;
        }
    }
}