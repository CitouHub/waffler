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

#pragma warning disable IDE0063 // Use simple 'using' statement
namespace Waffler.Service.Background
{
    public class BackgroundChartSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundChartSyncService> _logger;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;
        private readonly ICandleStickSyncSignal _candleStickSyncSignal;
        private readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(5);
        private readonly TimeSpan RequestSpanMinutes = TimeSpan.FromHours(6);
        private readonly string Period = Bitpanda.Period.MINUTES;
        private readonly short PeriodMinutes = 1;
        private readonly int RequestLimit = 180;
        private readonly int RequestPeriodSeconds = 60;
        private readonly object StartUpLock = new object();

        private Timer _timer;
        private bool InProgress = false;

        public BackgroundChartSyncService(
            ILogger<BackgroundChartSyncService> logger,
            IServiceProvider serviceProvider,
            IDatabaseSetupSignal databaseSetupSignal,
            ICandleStickSyncSignal candleStickSyncSignal)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _databaseSetupSignal = databaseSetupSignal;
            _candleStickSyncSignal = candleStickSyncSignal;
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
                _logger.LogDebug($"Setting up outer scoped services");
                using (IServiceScope outerScope = _serviceProvider.CreateScope())
                {
                    var _profileService = outerScope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _bitpandaService = outerScope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _mapper = outerScope.ServiceProvider.GetRequiredService<IMapper>();

                    _logger.LogInformation($"Setting initial parameters");
                    var syncActive = true;
                    var requestCount = 0;
                    var startTime = DateTime.UtcNow;
                    var profile = await _profileService.GetProfileAsync();

                    _logger.LogInformation($"Starting sync");
                    _candleStickSyncSignal.StartSync();

                    if (profile != null)
                    {
                        while (syncActive && cancellationToken.IsCancellationRequested == false && _candleStickSyncSignal.IsAbortRequested() == false)
                        {
                            _logger.LogDebug($"Setting up inner scoped services");
                            using (IServiceScope innerScope = _serviceProvider.CreateScope())
                            {
                                var _candleStickService = innerScope.ServiceProvider.GetRequiredService<ICandleStickService>();

                                _logger.LogInformation($"Getting last candlestick");
                                var period = (await _candleStickService.GetLastCandleStickAsync(DateTime.MaxValue))?.PeriodDateTime ??
                                    profile.CandleStickSyncFromDate;
                                var fromDate = period.AddMilliseconds(1);
                                var toDate = fromDate.AddMinutes(RequestSpanMinutes.TotalMinutes);

                                _logger.LogInformation($"Fetch data from {fromDate} to {toDate}");
                                var bp_candleSticksDTO = await _bitpandaService.GetCandleSticksAsync(
                                    Bitpanda.GetInstrumentCode(Variable.TradeType.BTC_EUR),
                                    Period, PeriodMinutes, fromDate, toDate);
                                requestCount++;

                                if (bp_candleSticksDTO != null)
                                {
                                    var latestPeriod = bp_candleSticksDTO.OrderByDescending(_ => _.Time).FirstOrDefault();

                                    if (bp_candleSticksDTO.Any())
                                    {
                                        _logger.LogInformation($"Fetch successfull, {bp_candleSticksDTO.Count()} new candlesticks found");
                                        var cancleSticksDTO = _mapper.Map<List<CandleStickDTO>>(bp_candleSticksDTO);
                                        await _candleStickService.AddCandleSticksAsync(cancleSticksDTO);
                                        _logger.LogDebug($"Data save successfull");
                                    }
                                    else
                                    {
                                        _logger.LogInformation($"Fetch successfull, no new data found, stop sync");
                                        _candleStickSyncSignal.SyncComplete();
                                        syncActive = false;
                                    }
                                }
                                else
                                {
                                    _logger.LogInformation($"Fetch failed, API unavailable");
                                    syncActive = false;
                                }

                                if (requestCount >= RequestLimit)
                                {
                                    var sleepTime = RequestPeriodSeconds * 1000 - (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
                                    if (sleepTime > 0)
                                    {
                                        _logger.LogInformation($"Reached request limit, sleep {sleepTime} ms");
                                        _candleStickSyncSignal.Throttle(true);
                                        Thread.Sleep(sleepTime);
                                        _candleStickSyncSignal.Throttle(false);
                                    }
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
                _logger.LogError(e, $"Unexpected exception");
            }

            InProgress = false;

            _logger.LogInformation($"Syncing candlestick data finished");
        }
    }
}
