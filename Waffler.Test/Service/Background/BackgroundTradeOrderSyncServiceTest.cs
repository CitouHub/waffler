﻿using System;
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
using Waffler.Service.Util;

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
        private readonly ICandleStickService _candleStickService = Substitute.For<ICandleStickService>();
        private readonly BackgroundTradeOrderSyncService _backgroundTradeOrderSyncService;

        public BackgroundTradeOrderSyncServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundTradeOrderSyncService>>();
            var databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();
            var tradeOrderSyncSignal = Substitute.For<ITradeOrderSyncSignal>();

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
            _serviceScope.ServiceProvider.GetService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetService<IMapper>().Returns(_mapper);
            _serviceScope.ServiceProvider.GetRequiredService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetRequiredService<ITradeOrderService>().Returns(_tradeOrderService);
            _serviceScope.ServiceProvider.GetRequiredService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetRequiredService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetRequiredService<IMapper>().Returns(_mapper);

            _backgroundTradeOrderSyncService = new BackgroundTradeOrderSyncService(logger, _serviceProvider, databaseSetupSignal, tradeOrderSyncSignal);
        }

        [Fact]
        public async Task FetchOrderData_NoProfile()
        {
            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
        }

        [Fact]
        public async Task FetchOrderData_NoApiKey()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = null;
            _profileService.GetProfileAsync().Returns(profile);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(CandleStickHelper.GetCandleStickDTO());

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
        }

        [Fact]
        public async Task FetchOrderData_DataNotSynced()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            var lastCandleStick = CandleStickHelper.GetCandleStickDTO();
            lastCandleStick.PeriodDateTime = DateTime.UtcNow.AddMinutes(-1 * DataSyncHandler.ValidSyncOffser.TotalMinutes);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(lastCandleStick);

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeOrderService.DidNotReceive().GetTradeOrderSyncPositionAsync();
        }

        [Fact]
        public async Task FetchOrderData_NoPreviousTradeOrderData_NoBitpandaOrderData()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(CandleStickHelper.GetCandleStickDTO());

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
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
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            var tradeOrder = TradeOrderHelper.GetTradeOrderDTO();
            tradeOrder.OrderDateTime = DateTime.UtcNow.AddDays(-10);
            _tradeOrderService.GetTradeOrderSyncPositionAsync().Returns(tradeOrder.OrderDateTime);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(CandleStickHelper.GetCandleStickDTO());

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
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
            var profile = ProfileHelper.GetProfileDTO();
            profile.ApiKey = "Test key";
            _profileService.GetProfileAsync().Returns(profile);
            _bitpandaService.GetOrdersAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR),
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate))
                .Returns(Enumerable.Repeat(BitpandaHelper.GetOrder(), nbrOfOrders).ToList());
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(CandleStickHelper.GetCandleStickDTO());

            //Act
            await _backgroundTradeOrderSyncService.FetchOrderDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeOrderService.Received().GetTradeOrderSyncPositionAsync();
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
