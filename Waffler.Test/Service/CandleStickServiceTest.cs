using AutoMapper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Test.Helper;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Waffler.Domain.Bitpanda.Private.Balance;
using System.Collections.Generic;
using Waffler.Common;

namespace Waffler.Test.Service
{
    public class CandleStickServiceTest
    {
        private readonly ILogger<CandleStickService> _logger = Substitute.For<ILogger<CandleStickService>>();
        private readonly IMapper _mapper; 

        public CandleStickServiceTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        public async Task AddCandleSticks_Added(int nbrOfCandleSticks)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var candleStickService = new CandleStickService(_logger, context, _mapper);

            //Act
            var candleSticks = Enumerable.Repeat(CandleStickHelper.GetCandleStickDTO(), nbrOfCandleSticks).ToList();
            await candleStickService.AddCandleSticksAsync(candleSticks);

            //Assert
            Assert.Equal(nbrOfCandleSticks, context.CandleSticks.Count());
        }

        [Fact]
        public async Task GetLastCandleStick_None()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var candleStick1 = CandleStickHelper.GetCandleStick();
            var candleStick2 = CandleStickHelper.GetCandleStick();
            candleStick1.PeriodDateTime = DateTime.UtcNow;
            candleStick2.PeriodDateTime = candleStick1.PeriodDateTime.AddMinutes(1);
            context.CandleSticks.Add(candleStick1);
            context.CandleSticks.Add(candleStick2);
            context.SaveChanges();
            var candleStickService = new CandleStickService(_logger, context, _mapper);

            //Act
            var lastCandleStick = await candleStickService.GetFirstCandleStickAsync(DateTime.UtcNow.AddMinutes(2));

            //Assert
            Assert.Null(lastCandleStick);
        }

        [Theory]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-01 12:01", "2021-01-01 12:01")]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-02 12:00", "2021-01-01 12:01")]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-01 12:00", "2021-01-01 12:00")]
        public async Task GetLastCandleStick(DateTime period1, DateTime period2, DateTime toPeriodDateTime, DateTime expectedPeriod)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var candleStick1 = CandleStickHelper.GetCandleStick();
            var candleStick2 = CandleStickHelper.GetCandleStick();
            candleStick1.PeriodDateTime = period1;
            candleStick2.PeriodDateTime = period2;
            context.CandleSticks.Add(candleStick1);
            context.CandleSticks.Add(candleStick2);
            context.SaveChanges();
            var candleStickService = new CandleStickService(_logger, context, _mapper);

            //Act
            var lastCandleStick = await candleStickService.GetLastCandleStickAsync(toPeriodDateTime);

            //Assert
            Assert.Equal(expectedPeriod, lastCandleStick.PeriodDateTime);
        }

        [Fact]
        public async Task GetFirstCandleStick_None()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var candleStick1 = CandleStickHelper.GetCandleStick();
            var candleStick2 = CandleStickHelper.GetCandleStick();
            candleStick1.PeriodDateTime = DateTime.UtcNow;
            candleStick2.PeriodDateTime = candleStick1.PeriodDateTime.AddMinutes(1);
            context.CandleSticks.Add(candleStick1);
            context.CandleSticks.Add(candleStick2);
            context.SaveChanges();
            var candleStickService = new CandleStickService(_logger, context, _mapper);

            //Act
            var firstCandleStick = await candleStickService.GetFirstCandleStickAsync(DateTime.UtcNow.AddMinutes(2));

            //Assert
            Assert.Null(firstCandleStick);
        }

        [Theory]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-01 12:00", "2021-01-01 12:00")]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2020-12-31 12:00", "2021-01-01 12:00")]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-01 12:01", "2021-01-01 12:01")]
        public async Task GetFirstCandleStick(DateTime period1, DateTime period2, DateTime fromPeriodDateTime, DateTime expectedPeriod)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var candleStick1 = CandleStickHelper.GetCandleStick();
            var candleStick2 = CandleStickHelper.GetCandleStick();
            candleStick2.PeriodDateTime = period1;
            candleStick1.PeriodDateTime = period2;
            context.CandleSticks.Add(candleStick1);
            context.CandleSticks.Add(candleStick2);
            context.SaveChanges();
            var candleStickService = new CandleStickService(_logger, context, _mapper);

            //Act
            var firstCandleStick = await candleStickService.GetFirstCandleStickAsync(fromPeriodDateTime);

            //Assert
            Assert.Equal(expectedPeriod, firstCandleStick.PeriodDateTime);
        }
    }
}
