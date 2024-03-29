﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
    public class BackgroundChartSyncServiceTest
    {
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly IProfileService _profileService = Substitute.For<IProfileService>();
        private readonly ICandleStickService _candleStickService = Substitute.For<ICandleStickService>();
        private readonly IBitpandaService _bitpandaService = Substitute.For<IBitpandaService>();
        private readonly IDatabaseSetupSignal _databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();
        private readonly ICandleStickSyncSignal _candleStickSyncSignal = Substitute.For<ICandleStickSyncSignal>();
        private readonly BackgroundChartSyncService _backgroundChartSyncService;

        public BackgroundChartSyncServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundChartSyncService>>();

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
            _serviceScope.ServiceProvider.GetService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetService<IMapper>().Returns(_mapper);
            _serviceScope.ServiceProvider.GetRequiredService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetRequiredService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetRequiredService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetRequiredService<IMapper>().Returns(_mapper);

            _backgroundChartSyncService = new BackgroundChartSyncService(logger, _serviceProvider, _databaseSetupSignal, _candleStickSyncSignal);
        }

        [Fact]
        public async Task FetchCandleStickData_NoProfile()
        {
            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _candleStickSyncSignal.Received().StartSync();
            _candleStickSyncSignal.Received().CloseSync();
        }

        [Fact]
        public async Task FetchCandleStickData_AbortRequested()
        {
            //Setup
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfileDTO());
            _candleStickSyncSignal.IsAbortRequested().Returns(true);

            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _candleStickSyncSignal.Received().StartSync();
            _candleStickSyncSignal.Received().IsAbortRequested();
            _candleStickSyncSignal.Received().CloseSync();
        }

        [Fact]
        public async Task FetchCandleStickData_NoPreviousCandleStickData_NoBitpandaCandleStickData()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            _profileService.GetProfileAsync().Returns(profile);

            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetCandleSticksAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Is(Bitpanda.Period.MINUTES), 1,
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate));
            _ = _candleStickService.DidNotReceive().AddCandleSticksAsync(Arg.Any<List<CandleStickDTO>>());
            _candleStickSyncSignal.Received().StartSync();
            _candleStickSyncSignal.Received().IsAbortRequested();
            _candleStickSyncSignal.Received().CloseSync();
        }

        [Fact]
        public async Task FetchCandleStickData_PreviousCandleStickData_NoBitpandaCandleStickData()
        {
            //Setup
            var profile = ProfileHelper.GetProfileDTO();
            _profileService.GetProfileAsync().Returns(profile);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = DateTime.UtcNow.AddDays(-10);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(candleStick);

            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetCandleSticksAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Is(Bitpanda.Period.MINUTES), 1,
                Arg.Is<DateTime>(_ => _.Date == candleStick.PeriodDateTime.Date),
                Arg.Is<DateTime>(_ => _ > candleStick.PeriodDateTime));
            _ = _candleStickService.DidNotReceive().AddCandleSticksAsync(Arg.Any<List<CandleStickDTO>>());
            _candleStickSyncSignal.Received().StartSync();
            _candleStickSyncSignal.Received().IsAbortRequested();
            _candleStickSyncSignal.Received().CloseSync();
        }

        [Fact]
        public async Task FetchCandleStickData_BitpandaCandleStickData()
        {
            //Setup
            var nbrOfCandleSticks = 10;
            var profile = ProfileHelper.GetProfileDTO();
            _profileService.GetProfileAsync().Returns(profile);
            var candleStick = CandleStickHelper.GetCandleStickDTO();
            candleStick.PeriodDateTime = DateTime.UtcNow;
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(_ => candleStick);

            _bitpandaService.GetCandleSticksAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Is(Bitpanda.Period.MINUTES), 1,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>())
                .Returns(
                    _ => BitpandaHelper.GetCandleSticks(nbrOfCandleSticks), 
                    _ => BitpandaHelper.GetCandleSticks(0));

            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received(1).GetProfileAsync();
            _ = _candleStickService.Received(1).GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received(2).GetCandleSticksAsync(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Is(Bitpanda.Period.MINUTES), 1,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>());
            _ = _candleStickService.Received(1).AddCandleSticksAsync(Arg.Is<List<CandleStickDTO>>(_ => _.Count == nbrOfCandleSticks));
            _candleStickSyncSignal.Received(1).StartSync();
            _candleStickSyncSignal.Received(2).IsAbortRequested();
            _candleStickSyncSignal.Received(1).SyncComplete();
            _candleStickSyncSignal.Received(1).CloseSync();
        }
    }
}
