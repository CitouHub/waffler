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
        private readonly IDatabaseSetupSignal _databaseSetupSignal;
        private readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan RequestSpanMinutes = TimeSpan.FromHours(6);
        private readonly string Period = Bitpanda.Period.MINUTES;
        private readonly short PeriodMinutes = 1;
        private readonly int NearEndSaveLimit = 5;
        private readonly int RequestLimit = 180;
        private readonly object StartUpLock = new object();

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundChartSyncService(
            ILogger<BackgroundChartSyncService> logger,
            IServiceProvider serviceProvider,
            IDatabaseSetupSignal databaseSetupSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;
            _logger.LogDebug("Instantiated");
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            if (!cancellationToken.IsCancellationRequested)
            {
                _timer = new Timer(async _ => await FetchCandleStickDataAsync(cancellationToken), null, TimeSpan.FromSeconds(0), SyncInterval);
            }
        }

        public async Task FetchCandleStickDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Syncing candlestick data");
            lock (StartUpLock)
            {
                if (InProgress)
                {
                    _logger.LogWarning($"Syncing candlestick data already in progress");
                    return;
                }
               
                InProgress = true;
            }

            _logger.LogInformation($"Waiting for database to be ready");
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);
            try
            {
                _logger.LogInformation($"Setting up scoped services");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _candleStickSyncSignal = scope.ServiceProvider.GetRequiredService<ICandleStickSyncSignal>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    _logger.LogInformation($"Setting initial parameters");
                    var syncingData = true;
                    var requestCount = 0;
                    var startTime = DateTime.UtcNow;
                    var profile = await _profileService.GetProfileAsync();

                    _logger.LogInformation($"Starting sync");
                    _candleStickSyncSignal.StartSync();

                    if(profile != null)
                    {
                        while (syncingData == true && cancellationToken.IsCancellationRequested == false && _candleStickSyncSignal.IsAbortRequested() == false)
                        {
                            _logger.LogInformation($"Getting last candlestick");
                            var period = (await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow))?.PeriodDateTime ??
                                profile.CandleStickSyncFromDate;
                            period = period.AddMilliseconds(1);

                            _logger.LogInformation($"Fetch data from {period} onward");
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
                                    _logger.LogInformation($"Fetch successfull, {bp_candleSticksDTO.Count()} new candlesticks found");
                                    var cancleSticksDTO = _mapper.Map<List<CandleStickDTO>>(bp_candleSticksDTO);
                                    await _candleStickService.AddCandleSticksAsync(cancleSticksDTO);
                                    _logger.LogInformation($"Data save successfull");
                                }
                                else
                                {
                                    _logger.LogInformation($"Fetch successfull, no new data found, stop sync");
                                    syncingData = false;
                                }
                            }
                            else
                            {
                                _logger.LogInformation($"Fetch failed, API unavailable");
                                syncingData = false;
                            }

                            if (requestCount >= RequestLimit)
                            {
                                var sleepTime = 60 * 1000 - (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                                _logger.LogInformation($"Reached request limit, sleep {sleepTime} ms");
                                Thread.Sleep(sleepTime <= 0 ? 0 : sleepTime);
                                startTime = DateTime.UtcNow;
                                requestCount = 0;
                            }

                            if (_timer != null)
                            {
                                var dueTime = SyncInterval;
                                if (_candleStickSyncSignal.IsAbortRequested())
                                {
                                    dueTime = TimeSpan.FromSeconds(10);
                                }
                                _timer.Change(dueTime, SyncInterval);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to start syncing, no profile");
                    }

                    _logger.LogInformation($"Stopping sync");
                    _candleStickSyncSignal.CloseSync();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }

            InProgress = false;

            _logger.LogInformation($"Syncing candlestick data finished");
        }
    }
}
