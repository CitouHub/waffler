using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using AutoMapper;
using NSubstitute;

using Waffler.Domain;
using Waffler.Service;
using Waffler.Service.Background;
using Waffler.Service.Infrastructure;
using Waffler.Test.Helper;
using System.Linq;

namespace Waffler.Test.Service.Background
{
    public class BackgroundTradeServiceTest
    {
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly ITradeRuleService _tradeRuleService = Substitute.For<ITradeRuleService>();
        private readonly ITradeService _tradeService = Substitute.For<ITradeService>();
        private readonly ICandleStickService _candleStickService = Substitute.For<ICandleStickService>();
        private readonly IDatabaseSetupSignal _databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();
        private readonly BackgroundTradeService _backgroundTradeService;

        public BackgroundTradeServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundTradeService>>();

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

            _serviceScope.ServiceProvider.GetService<ITradeRuleService>().Returns(_tradeRuleService);
            _serviceScope.ServiceProvider.GetService<ITradeService>().Returns(_tradeService);
            _serviceScope.ServiceProvider.GetService<ICandleStickService>().Returns(_candleStickService);
            _serviceScope.ServiceProvider.GetRequiredService<ITradeRuleService>().Returns(_tradeRuleService);
            _serviceScope.ServiceProvider.GetRequiredService<ITradeService>().Returns(_tradeService);
            _serviceScope.ServiceProvider.GetRequiredService<ICandleStickService>().Returns(_candleStickService);

            _backgroundTradeService = new BackgroundTradeService(logger, _serviceProvider, _databaseSetupSignal);
        }

        [Fact]
        public async Task HandleTradeRules_DataSyncedNotSynced()
        {
            //Setup
            var lastCandleStick = CandleStickHelper.GetCandleStickDTO();
            lastCandleStick.PeriodDateTime = DateTime.UtcNow.AddMinutes(-1 * _backgroundTradeService.ValidSyncOffser.TotalMinutes);
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(lastCandleStick);

            //Act
            await _backgroundTradeService.HandleTradeRulesAsync(new CancellationToken());

            //Asert
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeRuleService.DidNotReceive().GetTradeRulesAsync();
        }

        [Fact]
        public async Task HandleTradeRules_TestInProgress()
        {
            //Setup
            var lastCandleStick = CandleStickHelper.GetCandleStickDTO();
            lastCandleStick.PeriodDateTime = DateTime.UtcNow;
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(lastCandleStick);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.TestTradeInProgress = true;
            _tradeRuleService.GetTradeRulesAsync().Returns(Enumerable.Repeat(tradeRule, 1).ToList());

            //Act
            await _backgroundTradeService.HandleTradeRulesAsync(new CancellationToken());

            //Asert
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeRuleService.Received().GetTradeRulesAsync();
            _ = _tradeService.DidNotReceive().HandleTradeRuleAsync(Arg.Any<TradeRuleDTO>(), Arg.Any<DateTime>());
        }

        [Fact]
        public async Task HandleTradeRules()
        {
            //Setup
            var lastCandleStick = CandleStickHelper.GetCandleStickDTO();
            lastCandleStick.PeriodDateTime = DateTime.UtcNow;
            _candleStickService.GetLastCandleStickAsync(Arg.Any<DateTime>()).Returns(lastCandleStick);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            _tradeRuleService.GetTradeRulesAsync().Returns(Enumerable.Repeat(tradeRule, 1).ToList());

            //Act
            await _backgroundTradeService.HandleTradeRulesAsync(new CancellationToken());

            //Asert
            _ = _candleStickService.Received().GetLastCandleStickAsync(Arg.Any<DateTime>());
            _ = _tradeRuleService.Received().GetTradeRulesAsync();
            _ = _tradeService.Received().HandleTradeRuleAsync(Arg.Is(tradeRule), Arg.Is(lastCandleStick.PeriodDateTime));
        }
    }
}
