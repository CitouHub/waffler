using Waffler.Service.Infrastructure;
using Xunit;

namespace Waffler.Test.Service.Infrastructure
{
    public class CandleStickSyncSignalTest
    {
        private readonly CandleStickSyncSignal _candleStickSyncSignal;

        public CandleStickSyncSignalTest()
        {
            _candleStickSyncSignal = new CandleStickSyncSignal();
        }

        [Fact]
        public void StartSync()
        {
            //Act
            _candleStickSyncSignal.StartSync();

            //Assert
            Assert.False(_candleStickSyncSignal.IsAbortRequested());
            Assert.True(_candleStickSyncSignal.IsActive());
        }

        [Fact]
        public void Abort()
        {
            //Setup
            _candleStickSyncSignal.StartSync();

            //Act
            _candleStickSyncSignal.Abort();

            //Assert
            Assert.True(_candleStickSyncSignal.IsAbortRequested());
            Assert.True(_candleStickSyncSignal.IsActive());
        }

        [Fact]
        public void CloseSync()
        {
            //Setup
            _candleStickSyncSignal.StartSync();

            //Act
            _candleStickSyncSignal.CloseSync();

            //Assert
            Assert.False(_candleStickSyncSignal.IsAbortRequested());
            Assert.False(_candleStickSyncSignal.IsActive());
        }

        [Fact]
        public void CloseSync_AfterAbort()
        {
            //Setup
            _candleStickSyncSignal.StartSync();
            _candleStickSyncSignal.Abort();

            //Act
            _candleStickSyncSignal.CloseSync();

            //Assert
            Assert.False(_candleStickSyncSignal.IsAbortRequested());
            Assert.False(_candleStickSyncSignal.IsActive());
        }
    }
}
