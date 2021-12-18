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
    public class BackgroundOrderSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundOrderSyncService> _logger;
        private readonly DatabaseSetupSignal _databaseSetupSignal;
        private readonly TimeSpan RequestPeriod = TimeSpan.FromMinutes(5);

        private Timer _fetchTimer;
        private Timer _updateTimer;
        private bool FetchInProgress = false;
        private bool UpdateInProgress = false;

        public BackgroundOrderSyncService(
            IServiceProvider serviceProvider,
            ILogger<BackgroundOrderSyncService> logger,
            DatabaseSetupSignal databaseSetupSignal)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _databaseSetupSignal = databaseSetupSignal;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await _databaseSetupSignal.AwaitDatabaseReadyAsync(cancellationToken);

            if(!cancellationToken.IsCancellationRequested)
            {
                _fetchTimer = new Timer(async _ => await FetchOrderDataAsync(cancellationToken), null, TimeSpan.FromSeconds(0), RequestPeriod);
                _updateTimer = new Timer(async _ => await UpdateOrderDataAsync(cancellationToken), null, TimeSpan.FromSeconds(0), RequestPeriod);
            }
        }

        private async Task UpdateOrderDataAsync(CancellationToken cancellationToken)
        {
            if (UpdateInProgress)
            {
                return;
            }

            UpdateInProgress = true;
            try
            {
                _logger.LogInformation($"Syncing order data");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _logger.LogInformation($"- Setting up scoped services");
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _tradeOrderService = scope.ServiceProvider.GetRequiredService<ITradeOrderService>();
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    var profile = await _profileService.GetProfileAsync();

                    if (profile != null && cancellationToken.IsCancellationRequested == false)
                    {
                        _logger.LogInformation($"- Getting trade orders");
                        var orders = await _tradeOrderService.GetActiveTradeOrdersAsync();

                        foreach(var order in orders) 
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            _logger.LogInformation($"- Updating trade order {order}");
                            var bp_order = await _bitpandaService.GetOrderAsync(order.OrderId);

                            if(bp_order != null)
                            {
                                var tradeOrderDTO = _mapper.Map<TradeOrderDTO>(bp_order);
                                tradeOrderDTO.Id = order.Id;
                                tradeOrderDTO.TradeRuleId = order.TradeRuleId;
                                await _tradeOrderService.UpdateTradeOrderAsync(tradeOrderDTO);
                                _logger.LogInformation($"- Trade order {order} updated");
                            }
                            else
                            {
                                _logger.LogWarning($"- Trade order {order.OrderId} not found");
                            }

                            _updateTimer.Change(RequestPeriod, RequestPeriod);
                        }
                    }
                }
                _logger.LogInformation($"Syncing order data finished");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
            FetchInProgress = false;
        }

        private async Task FetchOrderDataAsync(CancellationToken cancellationToken)
        {
            if (FetchInProgress)
            {
                return;
            }

            FetchInProgress = true;
            try
            {
                _logger.LogInformation($"Syncing order data");
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    _logger.LogInformation($"- Setting up scoped services");
                    var _profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
                    var _tradeOrderService = scope.ServiceProvider.GetRequiredService<ITradeOrderService>();
                    var _candleStickService = scope.ServiceProvider.GetRequiredService<ICandleStickService>();
                    var _bitpandaService = scope.ServiceProvider.GetRequiredService<IBitpandaService>();
                    var _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

                    var profile = await _profileService.GetProfileAsync();

                    if (profile != null && cancellationToken.IsCancellationRequested == false)
                    {
                        _logger.LogInformation($"- Getting last order");
                        var period = (await _tradeOrderService.GetLastTradeOrderAsync(DateTime.UtcNow))?.OrderDateTime ??
                            profile.CandleStickSyncFromDate;
                        period = period.AddSeconds(1);

                        _logger.LogInformation($"- Fetch order data history");
                        var bp_orders = await _bitpandaService.GetOrdersAsync(
                            Bitpanda.GetInstrumentCode(TradeType.BTC_EUR),
                            period, DateTime.UtcNow);

                        if (bp_orders != null && bp_orders.Any() && cancellationToken.IsCancellationRequested == false)
                        {
                            _logger.LogInformation($"- Fetch successfull, {bp_orders.Count()} orders found");
                            var tradeOrdersDTO = _mapper.Map<List<TradeOrderDTO>>(bp_orders);
                            foreach(var tradeOrder in tradeOrdersDTO)
                            {
                                if(cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                await _tradeOrderService.AddTradeOrderAsync(tradeOrder);
                                _logger.LogInformation($"- Trade order {tradeOrder} added");

                                _fetchTimer.Change(RequestPeriod, RequestPeriod);
                            }
                            
                            _logger.LogInformation($"- Data save successfull");
                        }
                    }
                }
                _logger.LogInformation($"Syncing order data finished");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected exception {e.Message} {e.StackTrace}", e);
            }
            FetchInProgress = false;
        }
    }
}
