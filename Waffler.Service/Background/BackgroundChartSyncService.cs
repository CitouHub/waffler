using System;
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
using Waffler.Service.Infrastructure;
using static Waffler.Common.Variable;

namespace Waffler.Service.Background
{
    public class BackgroundChartSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundChartSyncService> _logger;
        private readonly DatabaseSetupSignal _databaseSetupSignal;
        private readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan RequestSpanMinutes = TimeSpan.FromHours(6);
        private readonly string Period = Bitpanda.Period.MINUTES;
        private readonly short PeriodMinutes = 1;
        private readonly int NearEndSaveLimit = 5;
        private readonly int RequestLimit = 180;

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundChartSyncService(
            ILogger<BackgroundChartSyncService> logger,
            IServiceProvider serviceProvider,
            DatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("BackgroundChartSyncService instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            if(!cancellationToken.IsCancellationRequested)
            {
                _timer = new Timer(async _ => await FetchCandleStickDataAsync(cancellationToken), null, TimeSpan.FromSeconds(0), SyncInterval);
            }
        }

        public async Task FetchCandleStickDataAsync(CancellationToken cancellationToken)
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
                    var startTime = DateTime.UtcNow;
                    var profile = await _profileService.GetProfileAsync();

                    while (profile != null && syncingData && cancellationToken.IsCancellationRequested == false)
                    {
                        _logger.LogInformation($"- Getting last candlestick");
                        var period = (await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow))?.PeriodDateTime ??
                            profile.CandleStickSyncFromDate;
                        period = period.AddMilliseconds(1);

                        _logger.LogInformation($"- Fetch data from {period} onward");
                        var bp_candleSticksDTO = await _bitpandaService.GetCandleSticksAsync(
                            Bitpanda.GetInstrumentCode(TradeType.BTC_EUR),
                            Period, PeriodMinutes, period, period.AddMinutes(RequestSpanMinutes.TotalMinutes));
                        requestCount++;

                        if (bp_candleSticksDTO != null)
                        {
                            var latestPeriod = bp_candleSticksDTO.OrderByDescending(_ => _.Time).FirstOrDefault();
                            var saveLimit = latestPeriod == null || (DateTime.UtcNow - latestPeriod.Time).TotalMinutes > 60 ? 0 : NearEndSaveLimit;

                            if (bp_candleSticksDTO.Count() > saveLimit)
                            {
                                _logger.LogInformation($"- Fetch successfull, {bp_candleSticksDTO.Count()} new candlesticks found");
                                var cancleSticksDTO = _mapper.Map<List<CandleStickDTO>>(bp_candleSticksDTO);
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

                        if (requestCount >= RequestLimit)
                        {
                            var sleepTime = 60 * 1000 - (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                            _logger.LogInformation($"- Reached request limit, sleep {sleepTime} ms");
                            Thread.Sleep(sleepTime <= 0 ? 0 : sleepTime);
                            startTime = DateTime.UtcNow;
                            requestCount = 0;
                        }

                        if(_timer != null)
                        {
                            _timer.Change(SyncInterval, SyncInterval);
                        }
                    }
                }
                _logger.LogInformation($"Syncing candlestick data finished");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
            InProgress = false;
        }
    }
}
