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
using Waffler.Data;

namespace Waffler.Test.Service
{
    public class TradeOrderServiceTest
    {
        private readonly ILogger<TradeOrderService> _logger = Substitute.For<ILogger<TradeOrderService>>();
        private readonly IMapper _mapper; 

        public TradeOrderServiceTest()
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new AutoMapperProfile());
            });
            _mapper = mapperConfig.CreateMapper();
        }

        [Fact]
        public async Task AddTradeOrder()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var tradeOrder = TradeOrderHelper.GetTradeOrderDTO();
            await tradeOrderService.AddTradeOrderAsync(tradeOrder);

            //Assert
            Assert.NotNull(context.TradeOrders.FirstOrDefault(_ => _.OrderId == tradeOrder.OrderId));
        }

        [Theory]
        [InlineData("2021-01-10 12:00", "2021-01-11 12:00", "2021-01-10 11:58", "2021-01-10 11:59", 0)]
        [InlineData("2021-01-10 12:00", "2021-01-11 12:00", "2021-01-10 11:59", "2021-01-10 12:00", 1)]
        [InlineData("2021-01-10 12:00", "2021-01-11 12:00", "2021-01-10 12:00", "2021-01-11 12:00", 2)]
        [InlineData("2021-01-10 12:00", "2021-01-11 12:00", "2021-01-10 12:01", "2021-01-11 12:01", 1)]
        [InlineData("2021-01-10 12:00", "2021-01-11 12:00", "2021-01-11 12:01", "2021-01-11 12:02", 0)]
        public async Task GetTradeOrders(DateTime from, DateTime to, DateTime order1Date, DateTime order2Date, int expectedOrders)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrder1 = TradeOrderHelper.GetTradeOrder();
            tradeOrder1.OrderDateTime = order1Date;
            var tradeOrder2 = TradeOrderHelper.GetTradeOrder();
            tradeOrder2.OrderDateTime = order2Date;
            context.TradeOrders.Add(tradeOrder1);
            context.TradeOrders.Add(tradeOrder2);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var tradeOrders = await tradeOrderService.GetTradeOrdersAsync(from, to);

            //Assert
            Assert.Equal(expectedOrders, tradeOrders.Count);
        }

        [Theory]
        [InlineData(Variable.TradeOrderStatus.Open, false, false)]
        [InlineData(Variable.TradeOrderStatus.StopTriggered, false, false)]
        [InlineData(Variable.TradeOrderStatus.Filled, false, false)]
        [InlineData(Variable.TradeOrderStatus.FilledFully, false, false)]
        [InlineData(Variable.TradeOrderStatus.FilledClosed, false, false)]
        [InlineData(Variable.TradeOrderStatus.FilledRejected, false, false)]
        [InlineData(Variable.TradeOrderStatus.Rejected, false, false)]
        [InlineData(Variable.TradeOrderStatus.Closed, false, false)]
        [InlineData(Variable.TradeOrderStatus.Failed, false, false)]
        [InlineData(Variable.TradeOrderStatus.Test, false, false)]
        [InlineData(Variable.TradeOrderStatus.Open, true, true)]
        [InlineData(Variable.TradeOrderStatus.StopTriggered, true, true)]
        [InlineData(Variable.TradeOrderStatus.Filled, true, true)]
        [InlineData(Variable.TradeOrderStatus.FilledFully, true, true)]
        [InlineData(Variable.TradeOrderStatus.FilledClosed, true, true)]
        [InlineData(Variable.TradeOrderStatus.FilledRejected, true, true)]
        [InlineData(Variable.TradeOrderStatus.Rejected, true, true)]
        [InlineData(Variable.TradeOrderStatus.Closed, true, true)]
        [InlineData(Variable.TradeOrderStatus.Failed, true, true)]
        [InlineData(Variable.TradeOrderStatus.Test, true, false)]
        public async Task GetActiveTradeOrders(Variable.TradeOrderStatus tradeOrderStatus, bool isActive, bool expectedActive)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeOrderStatusId = (short)tradeOrderStatus;
            tradeOrder.IsActive = isActive;
            context.TradeOrders.Add(tradeOrder);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var tradeOrders = await tradeOrderService.GetActiveTradeOrdersAsync();

            //Assert
            if(expectedActive)
            {
                Assert.Single(tradeOrders);
                Assert.Equal(tradeOrder.OrderId, tradeOrders[0].OrderId);
            } 
            else
            {
                Assert.Empty(tradeOrders);
            }
        }

        [Theory]
        [InlineData(Variable.TradeOrderStatus.Open, false)]
        [InlineData(Variable.TradeOrderStatus.StopTriggered, false)]
        [InlineData(Variable.TradeOrderStatus.Filled, false)]
        [InlineData(Variable.TradeOrderStatus.FilledFully, false)]
        [InlineData(Variable.TradeOrderStatus.FilledClosed, false)]
        [InlineData(Variable.TradeOrderStatus.FilledRejected, false)]
        [InlineData(Variable.TradeOrderStatus.Rejected, false)]
        [InlineData(Variable.TradeOrderStatus.Closed, false)]
        [InlineData(Variable.TradeOrderStatus.Failed, false)]
        [InlineData(Variable.TradeOrderStatus.Test, true)]
        public async Task RemoveTestTradeOrders_Single(Variable.TradeOrderStatus tradeOrderStatus, bool expectedRemoved)
        {
            //Setup
            var tradeRuleId = 1;
            var context = DatabaseHelper.GetContext();
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.TradeRuleId = tradeRuleId;
            tradeOrder.TradeOrderStatusId = (short)tradeOrderStatus;
            context.TradeOrders.Add(tradeOrder);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var ordersRemoved = await tradeOrderService.RemoveTestTradeOrdersAsync(tradeRuleId);

            //Assert
            if (expectedRemoved)
            {
                Assert.Empty(context.TradeOrders.ToList());
                Assert.Equal(1, ordersRemoved);
            }
            else
            {
                Assert.NotNull(context.TradeOrders.FirstOrDefault(_ => _.OrderId == tradeOrder.OrderId));
            }
        }

        [Fact]
        public async Task RemoveTestTradeOrders_Multi()
        {
            //Setup
            var tradeRuleId = 1;
            var context = DatabaseHelper.GetContext();
            var tradeOrder1 = TradeOrderHelper.GetTradeOrder();
            var tradeOrder2 = TradeOrderHelper.GetTradeOrder();
            var tradeOrder3 = TradeOrderHelper.GetTradeOrder();
            tradeOrder1.TradeRuleId = tradeRuleId;
            tradeOrder1.TradeOrderStatusId = (short)Variable.TradeOrderStatus.Open;
            tradeOrder2.TradeRuleId = tradeRuleId;
            tradeOrder2.TradeOrderStatusId = (short)Variable.TradeOrderStatus.Test;
            tradeOrder3.TradeRuleId = tradeRuleId;
            tradeOrder3.TradeOrderStatusId = (short)Variable.TradeOrderStatus.FilledFully;
            context.TradeOrders.Add(tradeOrder1);
            context.TradeOrders.Add(tradeOrder2);
            context.TradeOrders.Add(tradeOrder3);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var ordersRemoved = await tradeOrderService.RemoveTestTradeOrdersAsync(tradeRuleId);

            //Assert
            Assert.Equal(1, ordersRemoved);
            Assert.NotNull(context.TradeOrders.FirstOrDefault(_ => _.OrderId == tradeOrder1.OrderId));
            Assert.Null(context.TradeOrders.FirstOrDefault(_ => _.OrderId == tradeOrder2.OrderId));
            Assert.NotNull(context.TradeOrders.FirstOrDefault(_ => _.OrderId == tradeOrder3.OrderId));
        }

        [Theory]
        [InlineData(Variable.TradeOrderStatus.Open, false)]
        [InlineData(Variable.TradeOrderStatus.StopTriggered, false)]
        [InlineData(Variable.TradeOrderStatus.Filled, false)]
        [InlineData(Variable.TradeOrderStatus.FilledFully, false)]
        [InlineData(Variable.TradeOrderStatus.FilledClosed, false)]
        [InlineData(Variable.TradeOrderStatus.FilledRejected, false)]
        [InlineData(Variable.TradeOrderStatus.Rejected, false)]
        [InlineData(Variable.TradeOrderStatus.Closed, false)]
        [InlineData(Variable.TradeOrderStatus.Failed, false)]
        [InlineData(Variable.TradeOrderStatus.Test, true)]
        public async Task GetLastTradeOrder_ExcludeTestOrders(Variable.TradeOrderStatus tradeOrderStatus, bool expectedExclude)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrder = TradeOrderHelper.GetTradeOrder();
            tradeOrder.OrderDateTime = DateTime.UtcNow;
            tradeOrder.TradeOrderStatusId = (short)tradeOrderStatus;
            context.TradeOrders.Add(tradeOrder);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var lastTradeOrder = await tradeOrderService.GetLastTradeOrderAsync(tradeOrder.OrderDateTime);

            //Assert
            if (expectedExclude)
            {
                Assert.Null(lastTradeOrder);
            }
            else
            {
                Assert.Equal(tradeOrder.OrderId, lastTradeOrder.OrderId);
            }
        }

        [Theory]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-01 12:01", "2021-01-01 12:01")]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-02 12:00", "2021-01-01 12:01")]
        [InlineData("2021-01-01 12:00", "2021-01-01 12:01", "2021-01-01 12:00", "2021-01-01 12:00")]
        public async Task GetLastCandleStick(DateTime orderDate1, DateTime orderDate2, DateTime toPeriodDateTime, DateTime expectedPeriod)
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrder1 = TradeOrderHelper.GetTradeOrder();
            var tradeOrder2 = TradeOrderHelper.GetTradeOrder();
            tradeOrder1.OrderDateTime = orderDate1;
            tradeOrder1.TradeOrderStatusId = (short)Variable.TradeOrderStatus.Open;
            tradeOrder2.OrderDateTime = orderDate2;
            tradeOrder2.TradeOrderStatusId = (short)Variable.TradeOrderStatus.Open;
            context.TradeOrders.Add(tradeOrder1);
            context.TradeOrders.Add(tradeOrder2);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var tradeOrder = await tradeOrderService.GetLastTradeOrderAsync(toPeriodDateTime);

            //Assert
            Assert.Equal(expectedPeriod, tradeOrder.OrderDateTime);
        }

        [Fact]
        public async Task UpdateTradeOrder_WrongTradeOrderId()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrderDTO = TradeOrderHelper.GetTradeOrderDTO();
            tradeOrderDTO.Id = 1;
            var tradeOrder = _mapper.Map<TradeOrder>(tradeOrderDTO);
            context.TradeOrders.Add(tradeOrder);
            context.SaveChanges();
            tradeOrderDTO.Id = 2;
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            var success = await tradeOrderService.UpdateTradeOrderAsync(tradeOrderDTO);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public async Task UpdateTradeOrder_Updated()
        {
            //Setup
            var context = DatabaseHelper.GetContext();
            var tradeOrderDTO = TradeOrderHelper.GetTradeOrderDTO();
            tradeOrderDTO.Id = 1;
            var tradeOrder = _mapper.Map<TradeOrder>(tradeOrderDTO);
            context.TradeOrders.Add(tradeOrder);
            context.SaveChanges();
            var tradeOrderService = new TradeOrderService(_logger, context, _mapper);

            //Act
            tradeOrderDTO.OrderId = Guid.NewGuid();
            var success = await tradeOrderService.UpdateTradeOrderAsync(tradeOrderDTO);

            //Assert
            Assert.True(success);
            Assert.NotNull(context.TradeOrders.FirstOrDefault(_ => _.OrderId == tradeOrderDTO.OrderId));
        }
    }
}
