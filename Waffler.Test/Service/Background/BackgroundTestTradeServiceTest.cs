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
using Waffler.Domain.Message;

namespace Waffler.Test.Service.Background
{
    public class BackgroundTestTradeServiceTest
    {
        private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
        private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
        private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
        private readonly ITradeRuleService _tradeRuleService = Substitute.For<ITradeRuleService>();
        private readonly ITradeService _tradeService = Substitute.For<ITradeService>();
        private readonly ITradeRuleTestQueue _tradeRuleTestQueue = Substitute.For<ITradeRuleTestQueue>();
        private readonly IDatabaseSetupSignal _databaseSetupSignal = Substitute.For<IDatabaseSetupSignal>();
        private readonly BackgroundTestTradeService _backgroundTestTradeService;

        private readonly int TestTradeRuleId = 1;

        public BackgroundTestTradeServiceTest()
        {
            var logger = Substitute.For<ILogger<BackgroundTestTradeService>>();

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
            _serviceScope.ServiceProvider.GetRequiredService<ITradeRuleService>().Returns(_tradeRuleService);
            _serviceScope.ServiceProvider.GetRequiredService<ITradeService>().Returns(_tradeService);

            _backgroundTestTradeService = new BackgroundTestTradeService(logger, _serviceProvider, _tradeRuleTestQueue, _databaseSetupSignal);
        }

        [Fact]
        public async Task AbortOngoingTest_NoOngoingTest()
        {
            //Setup
            var status = TradeRuleTestQueueHelper.GetTradeRuleTestStatusDTO();
            status.TradeRuleId = TestTradeRuleId;

            //Act
            await _backgroundTestTradeService.AbortOngoingTest(new CancellationToken(), 1);

            //Asert
            _ = _tradeRuleTestQueue.Received().GetTradeRuleTestStatus(Arg.Is(TestTradeRuleId));
            _tradeRuleTestQueue.DidNotReceive().AbortTest(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleTestQueue.DidNotReceive().AwaitClose(Arg.Any<CancellationToken>(), Arg.Is(TestTradeRuleId));
        }

        [Fact]
        public async Task AbortOngoingTest_PreviousTestCompleted()
        {
            //Setup
            var status = TradeRuleTestQueueHelper.GetTradeRuleTestStatusDTO();
            status.TradeRuleId = TestTradeRuleId;
            _tradeRuleTestQueue.GetTradeRuleTestStatus(Arg.Is(TestTradeRuleId)).Returns(status);

            //Act
            await _backgroundTestTradeService.AbortOngoingTest(new CancellationToken(), 1);

            //Asert
            _ = _tradeRuleTestQueue.Received().GetTradeRuleTestStatus(Arg.Is(TestTradeRuleId));
            _tradeRuleTestQueue.DidNotReceive().AbortTest(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleTestQueue.DidNotReceive().AwaitClose(Arg.Any<CancellationToken>(), Arg.Is(TestTradeRuleId));
        }

        [Fact]
        public async Task AbortOngoingTest_OnGoingTest()
        {
            //Setup
            var status = TradeRuleTestQueueHelper.GetTradeRuleTestStatusDTO();
            status.TradeRuleId = TestTradeRuleId;
            status.FromDate = DateTime.UtcNow;
            status.ToDate = status.FromDate.AddDays(10);
            status.CurrentPositionDate = status.FromDate.AddDays(5);
            _tradeRuleTestQueue.GetTradeRuleTestStatus(Arg.Is(TestTradeRuleId)).Returns(status);

            //Act
            await _backgroundTestTradeService.AbortOngoingTest(new CancellationToken(), 1);

            //Asert
            _ = _tradeRuleTestQueue.Received().GetTradeRuleTestStatus(Arg.Is(TestTradeRuleId));
            _tradeRuleTestQueue.Received().AbortTest(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleTestQueue.Received().AwaitClose(Arg.Any<CancellationToken>(), Arg.Is(TestTradeRuleId));
        }

        [Fact]
        public async Task RunTradeTest_TestNotReady()
        {
            //Setup
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request.TradeRuleId = TestTradeRuleId;
            _tradeService.SetupTestTradeAsync(Arg.Is(TestTradeRuleId)).Returns(false);

            //Act
            await _backgroundTestTradeService.RunTradeTest(new CancellationToken(), request);

            //Asert
            _ = _tradeRuleTestQueue.Received().InitTradeRuleTestRun(request);
            _ = _tradeService.Received().SetupTestTradeAsync(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleService.DidNotReceive().GetTradeRuleAsync(Arg.Any<int>());
            _tradeRuleTestQueue.Received().CloseTest(Arg.Is(TestTradeRuleId));
        }

        [Fact]
        public async Task RunTradeTest_TestAborted()
        {
            //Setup
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request.TradeRuleId = TestTradeRuleId;
            _tradeService.SetupTestTradeAsync(Arg.Is(TestTradeRuleId)).Returns(true);
            var status = TradeRuleTestQueueHelper.GetTradeRuleTestStatusDTO();
            status.TradeRuleId = TestTradeRuleId;
            _tradeRuleTestQueue.InitTradeRuleTestRun(Arg.Is<TradeRuleTestRequestDTO>(_ => _.TradeRuleId == TestTradeRuleId)).Returns(status);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Id = TestTradeRuleId;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _tradeRuleTestQueue.IsTestAborted(Arg.Is(TestTradeRuleId)).Returns(true);

            //Act
            await _backgroundTestTradeService.RunTradeTest(new CancellationToken(), request);

            //Asert
            _ = _tradeRuleTestQueue.Received().InitTradeRuleTestRun(request);
            _ = _tradeService.Received().SetupTestTradeAsync(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleService.Received().GetTradeRuleAsync(Arg.Any<int>());
            _ = _tradeService.DidNotReceive().HandleTradeRuleAsync(Arg.Is<TradeRuleDTO>(_ => _.Id == TestTradeRuleId), Arg.Any<DateTime>());
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is<TradeRuleDTO>(_ => _.Id == TestTradeRuleId));
            _tradeRuleTestQueue.Received().CloseTest(Arg.Is(TestTradeRuleId));
        }

        [Theory]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:05", 1)]
        [InlineData("2021-01-01 12:00", "2021-01-01 15:05", 17)]
        [InlineData("2021-01-01 12:00", "2021-01-02 12:05", 45)]
        public async Task RunTradeTest_TestReady(DateTime fromDate, DateTime toDate, int minuteStep)
        {
            //Setup
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request.TradeRuleId = TestTradeRuleId;
            request.FromDate = fromDate;
            request.ToDate = toDate;
            request.MinuteStep = minuteStep;
            _tradeService.SetupTestTradeAsync(Arg.Is(TestTradeRuleId)).Returns(true);
            var status = TradeRuleTestQueueHelper.GetTradeRuleTestStatusDTO();
            status.TradeRuleId = TestTradeRuleId;
            status.FromDate = request.FromDate;
            status.ToDate = request.ToDate;
            status.CurrentPositionDate = request.FromDate;
            _tradeRuleTestQueue.InitTradeRuleTestRun(Arg.Is<TradeRuleTestRequestDTO>(_ => _.TradeRuleId == TestTradeRuleId)).Returns(status);
            var tradeRule = TradeRuleHelper.GetTradeRuleDTO();
            tradeRule.Id = TestTradeRuleId;
            _tradeRuleService.GetTradeRuleAsync(Arg.Is(TestTradeRuleId)).Returns(tradeRule);
            _tradeRuleTestQueue.IsTestAborted(Arg.Is(TestTradeRuleId)).Returns(false);

            //Act
            await _backgroundTestTradeService.RunTradeTest(new CancellationToken(), request);

            //Asert
            var iterations = (int)Math.Ceiling((request.ToDate.AddMinutes(request.MinuteStep) - request.FromDate).TotalMinutes / request.MinuteStep);
            _ = _tradeRuleTestQueue.Received().InitTradeRuleTestRun(request);
            _ = _tradeService.Received().SetupTestTradeAsync(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleService.Received().GetTradeRuleAsync(Arg.Is(TestTradeRuleId));
            _ = _tradeRuleTestQueue.Received().IsTestAborted(Arg.Is(TestTradeRuleId));
            _ = _tradeService.Received(iterations).HandleTradeRuleAsync(Arg.Is<TradeRuleDTO>(_ => _.Id == TestTradeRuleId), Arg.Any<DateTime>());
            _ = _tradeRuleService.Received().UpdateTradeRuleAsync(Arg.Is<TradeRuleDTO>(_ => _.Id == TestTradeRuleId));
            _tradeRuleTestQueue.Received().CloseTest(Arg.Is(TestTradeRuleId));
        }
    }
}
