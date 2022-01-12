using Waffler.Service.Infrastructure;
using Xunit;

namespace Waffler.Test.Service.Infrastructure
{
    public class TradeOrderSyncSignalTest
    {
        private readonly TradeOrderSyncSignal _tradeOrderSyncSignal;

        public TradeOrderSyncSignalTest()
        {
            _tradeOrderSyncSignal = new TradeOrderSyncSignal();
        }

        [Fact]
        public void StartSync()
        {
            //Act
            _tradeOrderSyncSignal.StartSync();

            //Assert
            Assert.False(_tradeOrderSyncSignal.IsAbortRequested());
            Assert.True(_tradeOrderSyncSignal.IsActive());
        }

        [Fact]
        public void Abort()
        {
            //Setup
            _tradeOrderSyncSignal.StartSync();

            //Act
            _tradeOrderSyncSignal.Abort();

            //Assert
            Assert.True(_tradeOrderSyncSignal.IsAbortRequested());
            Assert.True(_tradeOrderSyncSignal.IsActive());
        }

        [Fact]
        public void CloseSync()
        {
            //Setup
            _tradeOrderSyncSignal.StartSync();

            //Act
            _tradeOrderSyncSignal.CloseSync();

            //Assert
            Assert.False(_tradeOrderSyncSignal.IsAbortRequested());
            Assert.False(_tradeOrderSyncSignal.IsActive());
        }

        [Fact]
        public void CloseSync_AfterAbort()
        {
            //Setup
            _tradeOrderSyncSignal.StartSync();
            _tradeOrderSyncSignal.Abort();

            //Act
            _tradeOrderSyncSignal.CloseSync();

            //Assert
            Assert.False(_tradeOrderSyncSignal.IsAbortRequested());
            Assert.False(_tradeOrderSyncSignal.IsActive());
        }
    }
}
