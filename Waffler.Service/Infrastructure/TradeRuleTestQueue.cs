using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Waffler.Domain;
using Waffler.Domain.Message;

namespace Waffler.Service.Infrastructure
{
    public interface ITradeRuleTestQueue
    {
        void QueueTest(TradeRuleTestRequestDTO tradeRuleTestRequest);
        Task<TradeRuleTestRequestDTO> DequeueTestAsync(CancellationToken cancellationToken);
        bool AbortTest(int tradeRuleId);
        bool IsTestAborted(int tradeRuleId);
        Task AwaitClose(CancellationToken cancellationToken, int tradeRuleId);
        TradeRuleTestStatusDTO InitTradeRuleTestRun(TradeRuleTestRequestDTO tradeRuleTestRequest);
        TradeRuleTestStatusDTO GetTradeRuleTestStatus(int tradeRuleId);
        void CloseTest(int tradeRuleId);
    }

    public class TradeRuleTestQueue : ITradeRuleTestQueue
    {
        private readonly ILogger<TradeRuleTestQueue> _logger;
        private readonly ConcurrentQueue<TradeRuleTestRequestDTO> _tradeRuleTestRequests;
        private readonly SemaphoreSlim _queueSignal;
        private readonly Dictionary<int, TradeRuleTestRunDTO> _tradeRuleTestRun;

        public TradeRuleTestQueue(ILogger<TradeRuleTestQueue> logger)
        {
            _logger = logger;
            _tradeRuleTestRequests = new ConcurrentQueue<TradeRuleTestRequestDTO>();
            _queueSignal = new SemaphoreSlim(0);
            _tradeRuleTestRun = new Dictionary<int, TradeRuleTestRunDTO>();
            _logger.LogDebug($"Instantiated");
        }

        public void QueueTest(TradeRuleTestRequestDTO tradeRuleTestRequest)
        {
            tradeRuleTestRequest.FromDate = tradeRuleTestRequest.FromDate.Date;
            tradeRuleTestRequest.ToDate = tradeRuleTestRequest.ToDate.AddDays(1).Date.AddMinutes(-1);
            _tradeRuleTestRequests.Enqueue(tradeRuleTestRequest);
            _queueSignal.Release();
        }

        public async Task<TradeRuleTestRequestDTO> DequeueTestAsync(CancellationToken cancellationToken)
        {
            await _queueSignal.WaitAsync(cancellationToken);
            _tradeRuleTestRequests.TryDequeue(out var tradeTestRequest);

            return tradeTestRequest;
        }

        public bool AbortTest(int tradeRuleId)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleId))
            {
                _tradeRuleTestRun[tradeRuleId].Abort = true;

                return true;
            }

            return false;
        }

        public bool IsTestAborted(int tradeRuleId)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleId))
            {
                return _tradeRuleTestRun[tradeRuleId].Abort;
            }

            return false;
        }

        public async Task AwaitClose(CancellationToken cancellationToken, int tradeRuleId)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleId))
            {
                await _tradeRuleTestRun[tradeRuleId].CloseSignal.WaitAsync(cancellationToken);
            }
        }

        public TradeRuleTestStatusDTO InitTradeRuleTestRun(TradeRuleTestRequestDTO tradeRuleTestRequest)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleTestRequest.TradeRuleId) == false)
            {
                _tradeRuleTestRun.Add(tradeRuleTestRequest.TradeRuleId, null);
            }

            _tradeRuleTestRun[tradeRuleTestRequest.TradeRuleId] = new TradeRuleTestRunDTO
            {
                TradeRuleTestStatus = new TradeRuleTestStatusDTO()
                {
                    TradeRuleId = tradeRuleTestRequest.TradeRuleId,
                    FromDate = tradeRuleTestRequest.FromDate,
                    ToDate = tradeRuleTestRequest.ToDate,
                    CurrentPositionDate = tradeRuleTestRequest.FromDate
                },
                Abort = false,
                CloseSignal = new SemaphoreSlim(0)
            };

            return _tradeRuleTestRun[tradeRuleTestRequest.TradeRuleId].TradeRuleTestStatus;
        }

        public TradeRuleTestStatusDTO GetTradeRuleTestStatus(int tradeRuleId)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleId))
            {
                return _tradeRuleTestRun[tradeRuleId].TradeRuleTestStatus;
            }

            return null;
        }

        public void CloseTest(int tradeRuleId)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleId))
            {
                _tradeRuleTestRun[tradeRuleId].TradeRuleTestStatus.Aborted = _tradeRuleTestRun[tradeRuleId].Abort;
                _tradeRuleTestRun[tradeRuleId].CloseSignal.Release();
            }
        }
    }
}
