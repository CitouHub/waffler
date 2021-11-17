using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Waffler.Domain;
using Waffler.Domain.Message;

namespace Waffler.Service.Infrastructure
{
    public class TestTradeRuleQueue
    {
        private readonly ConcurrentQueue<TradeTestRequestDTO> _tradeTestRequests;
        private readonly SemaphoreSlim _signal;
        private readonly Dictionary<int, TradeTestStatusDTO> _tradeTestStatus;

        public TestTradeRuleQueue()
        {
            _tradeTestRequests = new ConcurrentQueue<TradeTestRequestDTO>();
            _signal = new SemaphoreSlim(0);
            _tradeTestStatus = new Dictionary<int, TradeTestStatusDTO>();
        }

        public void Queue(TradeTestRequestDTO tradeTestRequest)
        {
            _tradeTestRequests.Enqueue(tradeTestRequest);
            _signal.Release();
        }

        public async Task<TradeTestRequestDTO> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _tradeTestRequests.TryDequeue(out var tradeTestRequest);

            return tradeTestRequest;
        }

        public TradeTestStatusDTO SetStatus(TradeTestRequestDTO tradeRequest)
        {
            if (_tradeTestStatus.ContainsKey(tradeRequest.TradeRuleId) == false)
            {
                _tradeTestStatus.Add(tradeRequest.TradeRuleId, null);
            }

            _tradeTestStatus[tradeRequest.TradeRuleId] = new TradeTestStatusDTO()
            {
                TradeRuleId = tradeRequest.TradeRuleId,
                FromDate = tradeRequest.FromDate,
                ToDate = tradeRequest.ToDate,
                CurrentPositionDate = tradeRequest.FromDate
            };

            return _tradeTestStatus[tradeRequest.TradeRuleId];
        }

        public TradeTestStatusDTO GetStatus(int tradeRuleId)
        {
            if (_tradeTestStatus.ContainsKey(tradeRuleId))
            {
                return _tradeTestStatus[tradeRuleId];
            }

            return null;
        }
    }
}
