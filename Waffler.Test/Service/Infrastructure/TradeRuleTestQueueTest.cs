using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waffler.Service.Infrastructure;
using Waffler.Test.Helper;
using Xunit;

namespace Waffler.Test.Service.Infrastructure
{
    public class TradeRuleTestQueueTest
    {
        [Fact]
        public void GetTradeRuleTestStatus_NoTest()
        {
            //Setup
            var queue = new TradeRuleTestQueue();

            //Act
            var status = queue.GetTradeRuleTestStatus(-1);

            //Assert
            Assert.Null(status);
        }

        [Fact]
        public void GetTradeRuleTestStatus_NewTest_NoPreviousTests()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);

            //Act
            var status = queue.GetTradeRuleTestStatus(request.TradeRuleId);

            //Assert
            Assert.NotNull(status);
            Assert.Equal(request.FromDate, status.FromDate);
            Assert.Equal(request.FromDate, status.CurrentPositionDate);
            Assert.Equal(request.ToDate, status.ToDate);
            Assert.False(status.Aborted);
        }

        [Fact]
        public void GetTradeRuleTestStatus_NewTest_PreviousTests()
        {
            //Setup
            var testTradeRuleId = 1;
            var queue = new TradeRuleTestQueue();
            var request1 = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request1.TradeRuleId = testTradeRuleId;
            request1.FromDate = DateTime.UtcNow.AddMinutes(-10);
            request1.ToDate = DateTime.UtcNow.AddMinutes(-5);
            var request2 = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request2.TradeRuleId = testTradeRuleId;
            request2.FromDate = DateTime.UtcNow.AddMinutes(-20);
            request2.ToDate = DateTime.UtcNow.AddMinutes(-10);
            queue.InitTradeRuleTestRun(request1);
            queue.InitTradeRuleTestRun(request2);

            //Act
            var status = queue.GetTradeRuleTestStatus(testTradeRuleId);

            //Assert
            Assert.NotNull(status);
            Assert.Equal(request2.FromDate, status.FromDate);
            Assert.Equal(request2.FromDate, status.CurrentPositionDate);
            Assert.Equal(request2.ToDate, status.ToDate);
            Assert.False(status.Aborted);
        }

        [Fact]
        public void AbortTest_False()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);

            //Act
            var success = queue.AbortTest(-1);

            //Assert
            Assert.False(success);
        }

        [Fact]
        public void AbortTest_True()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);

            //Act
            var success = queue.AbortTest(request.TradeRuleId);

            //Assert
            Assert.True(success);
        }

        [Fact]
        public void IsTestAborted_False()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);

            //Act
            var aborted = queue.IsTestAborted(request.TradeRuleId);

            //Assert
            Assert.False(aborted);
        }

        [Fact]
        public void IsTestAborted_True()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);
            queue.AbortTest(request.TradeRuleId);

            //Act
            var aborted = queue.IsTestAborted(request.TradeRuleId);

            //Assert
            Assert.True(aborted);
        }

        [Fact]
        public void IsTestAborted_MultiTests()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request1 = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request1.TradeRuleId = 1;
            var request2 = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            request2.TradeRuleId = 2;
            queue.InitTradeRuleTestRun(request1);
            queue.InitTradeRuleTestRun(request2);
            queue.AbortTest(request2.TradeRuleId);

            //Act
            var aborted1 = queue.IsTestAborted(request1.TradeRuleId);
            var aborted2 = queue.IsTestAborted(request2.TradeRuleId);

            //Assert
            Assert.False(aborted1);
            Assert.True(aborted2);
        }

        [Fact]
        public void CloseTest_NotAborted()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);

            //Act
            queue.CloseTest(request.TradeRuleId);

            //Assert
            var status = queue.GetTradeRuleTestStatus(request.TradeRuleId);
            Assert.False(status.Aborted);
        }

        [Fact]
        public void CloseTest_Aborted()
        {
            //Setup
            var queue = new TradeRuleTestQueue();
            var request = TradeRuleTestQueueHelper.GetTradeRuleTestRequestDTO();
            queue.InitTradeRuleTestRun(request);

            //Act
            queue.AbortTest(request.TradeRuleId);
            queue.CloseTest(request.TradeRuleId);

            //Assert
            var status = queue.GetTradeRuleTestStatus(request.TradeRuleId);
            Assert.True(status.Aborted);
        }
    }
}
