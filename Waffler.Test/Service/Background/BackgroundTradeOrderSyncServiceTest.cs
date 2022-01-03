using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using AutoMapper;
using NSubstitute;

using Waffler.Common;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Service.Background;
using Waffler.Service.Infrastructure;
using Waffler.Test.Helper;

namespace Waffler.Test.Service.Background
{
    public class BackgroundTradeOrderSyncServiceTest
    {
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly IProfileService _profileService = Substitute.For<IProfileService>();
        private readonly ITradeOrderService _tradeOrderService = Substitute.For<ITradeOrderService>();
        private readonly IBitpandaService _bitpandaService = Substitute.For<IBitpandaService>();
        private readonly BackgroundTradeOrderSyncService _backgroundTradeOrderSyncService;

        public BackgroundTradeOrderSyncServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundTradeOrderSyncService>>();
            var databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();

            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            var _mapper = mapperConfig.CreateMapper();

            _serviceScope.ServiceProvider.GetService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetService<ITradeOrderService>().Returns(_tradeOrderService);
            _serviceScope.ServiceProvider.GetService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetService<IMapper>().Returns(_mapper);
            _serviceScope.ServiceProvider.GetRequiredService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetRequiredService<ITradeOrderService>().Returns(_tradeOrderService);
            _serviceScope.ServiceProvider.GetRequiredService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetRequiredService<IMapper>().Returns(_mapper);

            _backgroundTradeOrderSyncService = new BackgroundTradeOrderSyncService(logger, _serviceProvider, databaseSetupSignal);
        }

        [Fact]
        public async Task FetchOrderData_NoProfile()
        {
            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.DidNotReceive().GetLastTradeOrderAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchOrderData_NoPreviousTradeOrderData_NoBitpandaOrderData()
        {
            //Setup
            var profile = ProfileHelper.GetProfile();
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetLastTradeOrderAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate));
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
        }

        [Fact]
        public async Task FetchOrderData_PreviousTradeOrderData_NoBitpandaOrderData()
        {
            //Setup
            var profile = ProfileHelper.GetProfile();
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());
            var tradeOrder = TradeOrderHelper.GetTradeOrderDTO();
            tradeOrder.OrderDateTime = DateTime.UtcNow.AddDays(-10);
            _tradeOrderService.GetLastTradeOrderAsync(Arg.Any<DateTime>()).Returns(tradeOrder);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetLastTradeOrderAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == tradeOrder.OrderDateTime.Date),
                Arg.Is<DateTime>(_ => _ > tradeOrder.OrderDateTime));
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
        }

        [Fact]
        internal async Task FetchOrderData_BitpandaOrderData()
        {
            //Setup
            var nbrOfOrders = 10;
            var profile = ProfileHelper.GetProfile();
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());
            _bitpandaService.GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate))
                .Returns(Enumerable.Repeat(BitpandaHelper.GetOrder(), nbrOfOrders).ToList());

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetLastTradeOrderAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate));
            _ = _tradeOrderService.Received(nbrOfOrders).AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
        }

        [Fact]
        public async Task UpdateOrderData_NoProfile()
        {
            //Act
            await _backgroundTradeOrderSyncService.UpdateOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.DidNotReceive().GetLastTradeOrderAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task UpdateOrderData_NoActiveOrders()
        {
            //Setup
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());

            //Act
            await _backgroundTradeOrderSyncService.UpdateOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetActiveTradeOrdersAsync();
            _ = _bitpandaService.DidNotReceive().GetOrdersAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
        }

        [Fact]
        public async Task UpdateOrderData_ActiveOrder_NoBitpandaOrderData()
        {
            //Setup
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());
            var tradeOrder = TradeOrderHelper.GetTradeOrderDTO();
            _tradeOrderService.GetActiveTradeOrdersAsync()
                .Returns(Enumerable.Repeat(tradeOrder, 1).ToList());

            //Act
            await _backgroundTradeOrderSyncService.UpdateOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetActiveTradeOrdersAsync();
            _ = _bitpandaService.Received().GetOrderAsync(Arg.Is(tradeOrder.OrderId));
            _ = _tradeOrderService.DidNotReceive().UpdateTradeOrderAsync(Arg.Any<TradeOrderDTO>());
        }

        [Fact]
        internal async Task UpdateOrderData_ActiveOrder_BitpandaOrderData()
        {
            //Setup
            var profile = ProfileHelper.GetProfile();
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());
            var tradeOrderDTO = TradeOrderHelper.GetTradeOrderDTO();
            _tradeOrderService.GetActiveTradeOrdersAsync()
                .Returns(Enumerable.Repeat(tradeOrderDTO, 1).ToList());
            var tradeOrder = BitpandaHelper.GetOrder();
            tradeOrder.Order_id = tradeOrderDTO.OrderId.ToString();
            _ = _bitpandaService.GetOrderAsync(Arg.Is(tradeOrderDTO.OrderId))
                .Returns(tradeOrder);

            //Act
            await _backgroundTradeOrderSyncService.UpdateOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetActiveTradeOrdersAsync();
            _ = _bitpandaService.Received().GetOrderAsync(Arg.Is(tradeOrderDTO.OrderId));
            _ = _tradeOrderService.Received().UpdateTradeOrderAsync(Arg.Is<TradeOrderDTO>(_ => _.OrderId == tradeOrderDTO.OrderId));
        }
    }
}
