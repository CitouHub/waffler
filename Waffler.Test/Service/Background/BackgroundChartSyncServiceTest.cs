using System;
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
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly BackgroundChartSyncService _backgroundChartSyncService;

        public BackgroundChartSyncServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundChartSyncService>>();
            var databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();

            _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService(typeof(IServiceScopeFactory)).Returns(_serviceScopeFactory);
            _serviceProvider.GetRequiredService<IServiceScopeFactory>().Returns(_serviceScopeFactory);
            _serviceProvider.CreateScope().Returns(_serviceScope);
            _serviceScope.ServiceProvider.Returns(_serviceProvider);

            _serviceScope.ServiceProvider.GetService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetService<IMapper>().Returns(_mapper);
            _serviceScope.ServiceProvider.GetRequiredService<IProfileService>().Returns(_profileService);
            _serviceScope.ServiceProvider.GetRequiredService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetRequiredService<IBitpandaService>().Returns(_bitpandaService);
            _serviceScope.ServiceProvider.GetRequiredService<IMapper>().Returns(_mapper);

            _backgroundChartSyncService = new BackgroundChartSyncService(_serviceProvider, logger, databaseSetupSignal);
        }

        [Fact]
        public async Task FetchCandleStickDataAsync_NoProfile()
        {
            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.DidNotReceive().GetLastCandleStickAsync(Arg.Any<DateTime>());
        }

        [Fact]
        public async Task FetchCandleStickDataAsync_NoPreviousCandleStickData_NoBitpandaCandleStickData()
        {
            //Setup
            var profile = ProfileHelper.GetProfile();
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfile());

            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetCandleSticks(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Is(Bitpanda.Period.MINUTES), 1,
                Arg.Is<DateTime>(_ => _.Date == profile.CandleStickSyncFromDate.Date),
                Arg.Is<DateTime>(_ => _ > profile.CandleStickSyncFromDate));
            _ = _candleStickService.DidNotReceive().AddCandleSticksAsync(Arg.Any<List<CandleStickDTO>>());
        }

        [Fact]
        public async Task FetchCandleStickDataAsync_PreviousCandleStickData_NoBitpandaCandleStickData()
        {
            //Setup
            var profile = ProfileHelper.GetProfile();
            _profileService.GetProfileAsync().Returns(ProfileHelper.GetProfile());
            var candleStick = CandleStickHelper.GetCandleStick();
            candleStick.PeriodDateTime = DateTime.UtcNow.AddDays(-10);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(candleStick);

            //Act
            await _backgroundChartSyncService.FetchCandleStickDataAsync(new CancellationToken());

            //Asert
            _ = _profileService.Received().GetProfileAsync();
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _bitpandaService.Received().GetCandleSticks(
                Arg.Is(Bitpanda.InstrumentCode.BTC_EUR), Arg.Is(Bitpanda.Period.MINUTES), 1,
                Arg.Is<DateTime>(_ => _.Date == candleStick.PeriodDateTime.Date),
                Arg.Is<DateTime>(_ => _ > candleStick.PeriodDateTime));
            _ = _candleStickService.DidNotReceive().AddCandleSticksAsync(Arg.Any<List<CandleStickDTO>>());
        }
    }
}
