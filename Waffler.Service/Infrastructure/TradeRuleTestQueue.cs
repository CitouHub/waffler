using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Waffler.Domain;
using Waffler.Domain.Message;

namespace Waffler.Service.Infrastructure
{
    public class TradeRuleTestQueue
    {
        private readonly ConcurrentQueue<TradeRuleTestRequestDTO> _tradeRuleTestRequests;
        private readonly SemaphoreSlim _queueSignal;
        private readonly Dictionary<int, TradeRuleTestRunDTO> _tradeRuleTestRun;

        public TradeRuleTestQueue()
        {
            _tradeRuleTestRequests = new ConcurrentQueue<TradeRuleTestRequestDTO>();
            _queueSignal = new SemaphoreSlim(0);
            _tradeRuleTestRun = new Dictionary<int, TradeRuleTestRunDTO>();
        }

        public void QueueTest(TradeRuleTestRequestDTO tradeRuleTestRequest)
        {
            _tradeRuleTestRequests.Enqueue(tradeRuleTestRequest);
            _queueSignal.Release();
        }

        public async Task<TradeRuleTestRequestDTO> DequeueTestAsync(CancellationToken cancellationToken)
        {
            await _queueSignal.WaitAsync(cancellationToken);
            _tradeRuleTestRequests.TryDequeue(out var tradeTestRequest);

            return tradeTestRequest;
        }

        public void AbortTest(int tradeRuleId)
        {
            if (_tradeRuleTestRun.ContainsKey(tradeRuleId))
            {
                _tradeRuleTestRun[tradeRuleId].Abort = true;
            } 
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
