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
        private readonly ITradeOrderSyncSignal _tradeOrderSyncSignal = Substitute.For<ITradeOrderSyncSignal>();
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

            _backgroundTradeOrderSyncService = new BackgroundTradeOrderSyncService(logger, _serviceProvider, databaseSetupSignal, _tradeOrderSyncSignal);
        }

        [Fact]
        public async Task FetchOrderData_NoProfile()
        {
            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
            _ = _tradeOrderService.DidNotReceive().SetTradeOrderSyncPositionAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchOrderData_AbortRequested()
        {
            //Setup
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());
            _tradeOrderSyncSignal.IsAbortRequested().Returns(true);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
            _ = _bitpandaService.DidNotReceive().GetOrdersAsync(Arg.Any<string>(), Arg.Any<DateTime>(), Arg.Any<DateTime>());
            _ = _tradeOrderService.DidNotReceive().SetTradeOrderSyncPositionAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchOrderData_NoApiKey()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = null;
            _profileService.GetProfileAsync().Returns(profile);
            _tradeOrderService.GetTradeOrderSyncPositionAsync().Returns(DateTime.UtcNow);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
            _ = _tradeOrderService.DidNotReceive().SetTradeOrderSyncPositionAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchOrderData_NoPreviousTradeOrderData_NoBitpandaOrderData()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            _tradeOrderService.GetTradeOrderSyncPositionAsync().Returns(profile.CandleStickSyncFromDate);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
            _ = _bitpandaService.Received().GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate));
            _ = _tradeOrderService.DidNotReceive().TradeOrderExistsAsync(Arg.Any<Guid>());
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _tradeOrderService.Received().SetTradeOrderSyncPositionAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchOrderData_PreviousTradeOrderData_NoBitpandaOrderData()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            var tradeOrder = TradeOrderHelper.GetTradeOrderDTO();
            tradeOrder.OrderDateTime = DateTime.UtcNow.AddDays(-10);
            _tradeOrderService.GetTradeOrderSyncPositionAsync().Returns(tradeOrder.OrderDateTime);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
            _ = _bitpandaService.Received().GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == tradeOrder.OrderDateTime.Date),
                Arg.Is<DateTime>(_ => _ > tradeOrder.OrderDateTime));
            _ = _tradeOrderService.DidNotReceive().TradeOrderExistsAsync(Arg.Any<Guid>());
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _tradeOrderService.Received().SetTradeOrderSyncPositionAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchOrderData_BitpandaOrderData()
        {
            //Setup
            var nbrOfOrders = 10;
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            _bitpandaService.GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate))
                .Returns(Enumerable.Repeat(BitpandaHelper.GetOrder(), nbrOfOrders).ToList());
            _tradeOrderService.GetTradeOrderSyncPositionAsync().Returns(profile.CandleStickSyncFromDate);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            var daysInterval = _backgroundTradeOrderSyncService.FetchDaysInterval;
            var nbrOfLoops = (int)Math.Ceiling((decimal)(DateTime.UtcNow - profile.CandleStickSyncFromDate).TotalDays / (decimal)daysInterval);
            _ = _profileService.Received(1).GetProfileAsync();
            _ = _tradeOrderService.Received(1).GetTradeOrderSyncPositionAsync();
            _ = _bitpandaService.Received(nbrOfLoops).GetOrdersAsync(Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Any<DateTime>(), Arg.Any<DateTime>());
            _ = _tradeOrderService.Received(nbrOfOrders).TradeOrderExistsAsync(Arg.Any<Guid>());
            _ = _tradeOrderService.Received(nbrOfOrders).AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _tradeOrderService.Received(1).SetTradeOrderSyncPositionAsync(Arg.Is<DateTime>(_ => _.Date >= DateTime.Now.Date));
        }

        [Fact]
        public async Task FetchOrderData_ExistingOrder()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            var order = BitpandaHelper.GetOrder();
            _bitpandaService.GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate))
                .Returns(Enumerable.Repeat(order, 1).ToList());
            _tradeOrderService.GetTradeOrderSyncPositionAsync().Returns(profile.CandleStickSyncFromDate);
            _tradeOrderService.TradeOrderExistsAsync(Arg.Is(new Guid(order.Order_id))).Returns(true);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
            _ = _bitpandaService.Received().GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate));
            _ = _tradeOrderService.Received().TradeOrderExistsAsync(Arg.Any<Guid>());
            _ = _tradeOrderService.DidNotReceive().AddTradeOrderAsync(Arg.Any<TradeOrderDTO>());
            _ = _tradeOrderService.Received().SetTradeOrderSyncPositionAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task UpdateOrderData_NoProfile()
        {
            //Act
            await _backgroundTradeOrderSyncService.UpdateOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
        }

        [Fact]
        public async Task UpdateOrderData_NoApiKey()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = null;
            _profileService.GetProfileAsync().Returns(profile);

            //Act
            await _backgroundTradeOrderSyncService.UpdateOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
        }

        [Fact]
        public async Task UpdateOrderData_NoActiveOrders()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);

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
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
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
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
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
