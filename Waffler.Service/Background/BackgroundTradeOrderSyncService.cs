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
using Waffler.Service.Util;

namespace Waffler.Service.Background
{
    public class BackgroundTradeOrderSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTradeOrderSyncService> _logger;
        private readonly IDatabaseSetupSignal _databaseSetupSignal;
        private readonly TimeSpan RequestPeriod = TimeSpan.FromMinutes(5);
        private readonly object FetchStartLock = new object();
        private readonly object UpdateStartLock = new object();

        private Timer _fetchTimer;
        private Timer _updateTimer;
        private bool FetchInProgress = false;
        private bool UpdateInProgress = false;

        public BackgroundTradeOrderSyncService(
            ILogger<BackgroundTradeOrderSyncService> logger,
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
                _fetchTimer = new Timer(async _ => await FetchOrderDataAsync(cancellationToken), null, TimeSpan.FromSeconds(0), RequestPeriod);
                _updateTimer = new Timer(async _ => await UpdateOrderDataAsync(cancellationToken), null, TimeSpan.FromSeconds(0), RequestPeriod);
            }
        }

        public async Task FetchOrderDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Fetch trade orders");
            lock (FetchStartLock)
            {
                if (FetchInProgress)
                {
                    _logger.LogWarning($"Fetch trade orders already in progress");
                    return;
                }

                FetchInProgress = true;
            }

            _logger.LogInformation($"Waiting for database to be ready");
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);
            try
            {
                _logger.LogInformation($"Setting up scoped services");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _tradeOrderService = scope.ServiceProvider.GetRequiredService<ITradeOrderService>();
                    var _bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    _logger.LogInformation($"Preparing fetch");
                    var profile = await _profileService.GetProfileAsync();
                    var lastCandleStick = await _candleStickService.GetLastCandleStickAsync(DateTime.UtcNow);

                    if (profile != null && string.IsNullOrEmpty(profile.ApiKey) == false &&
                        DataSyncHandler.IsDataSynced(lastCandleStick) &&
                        cancellationToken.IsCancellationRequested == false)
                    {
                        _logger.LogInformation($"Getting last order");
                        var period = (await _tradeOrderService.GetLastTradeOrderAsync(DateTime.UtcNow))?.OrderDateTime ??
                            profile.CandleStickSyncFromDate;
                        period = period.AddSeconds(1);

                        _logger.LogInformation($"Fetch order data history");
                        var bp_orders = await _bitpandaService.GetOrdersAsync(
                            Bitpanda.GetInstrumentCode(TradeType.BTC_EUR),
                            period, DateTime.UtcNow);

                        if (bp_orders != null && bp_orders.Any() && cancellationToken.IsCancellationRequested == false)
                        {
                            _logger.LogInformation($"Fetch successfull, {bp_orders.Count()} orders found");
                            var tradeOrdersDTO = _mapper.Map<List<TradeOrderDTO>>(bp_orders);
                            foreach (var tradeOrder in tradeOrdersDTO)
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                await _tradeOrderService.AddTradeOrderAsync(tradeOrder);
                                _logger.LogInformation($"Trade order {tradeOrder} added");

                                if (_fetchTimer != null)
                                {
                                    _fetchTimer.Change(RequestPeriod, RequestPeriod);
                                }
                            }

                            _logger.LogInformation($"Trade order save successfull");
                        } 
                        else
                        {
                            _logger.LogInformation($"No new trade orders could be found");
                        }
                    } 
                    else
                    {
                        _logger.LogWarning($"Fetch trade orders could not be started");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected exception");
            }

            FetchInProgress = false;

            _logger.LogInformation($"Fetch trade orders finished");
        }

        public async Task UpdateOrderDataAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Update trade orders");
            lock (UpdateStartLock)
            {
                if (UpdateInProgress)
                {
                    _logger.LogWarning($"Update trade orders already in progress");
                    return;
                }

                UpdateInProgress = true;
            }

            _logger.LogInformation($"Waiting for database to be ready");
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);
            try
            {
                _logger.LogInformation($"Setting up scoped services");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _tradeOrderService = scope.ServiceProvider.GetRequiredService<ITradeOrderService>();
                    var _bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    _logger.LogInformation($"Preparing update");
                    var profile = await _profileService.GetProfileAsync();

                    if (profile != null && string.IsNullOrEmpty(profile.ApiKey) == false &&
                        cancellationToken.IsCancellationRequested == false)
                    {
                        _logger.LogInformation($"Getting trade orders");
                        var orders = await _tradeOrderService.GetActiveTradeOrdersAsync();

                        foreach (var order in orders)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            _logger.LogInformation($"Updating trade order {order}");
                            var bp_order = await _bitpandaService.GetOrderAsync(order.OrderId);

                            if (bp_order != null)
                            {
                                var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(bp_order);
                                tradeOrderDTO.Id = order.Id;
                                tradeOrderDTO.TradeRuleId = order.TradeRuleId;
                                await _tradeOrderService.UpdateTradeOrderAsync(tradeOrderDTO);
                                _logger.LogInformation($"Trade order {tradeOrderDTO} updated");
                            }
                            else
                            {
                                _logger.LogWarning($"Trade order {order.OrderId} not found");
                            }

                            _updateTimer.Change(RequestPeriod, RequestPeriod);
                        }
                    } 
                    else
                    {
                        _logger.LogWarning($"Update trade orders could not be started");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected exception");
            }

            FetchInProgress = false;

            _logger.LogInformation($"Update trade orders finished");
        }
    }
}
