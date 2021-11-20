using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.Domain;
using Waffler.Domain.Message;
using Waffler.Service;
using Waffler.Service.Infrastructure;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    public class TradeRuleController : ControllerBase
    {
        private readonly ITradeRuleService _tradeRuleService;
        private readonly TradeRuleTestQueue _testTradeRuleQueue;

        public TradeRuleController(
            ITradeRuleService tradeRuleService,
            TradeRuleTestQueue testTradeRuleQueue)
        {
            _tradeRuleService = tradeRuleService;
            _testTradeRuleQueue = testTradeRuleQueue;
        }

        [HttpGet]
        public async Task<IEnumerable<TradeRuleDTO>> GetTradeRulesAsync()
        {
            return await _tradeRuleService.GetTradeRulesAsync();
        }

        [HttpPost]
        [Route("test")]
        public void TestTradeRuleAsync([FromBody] TradeRuleTestRequestDTO tradeRequest)
        {
            _testTradeRuleQueue.QueueTest(tradeRequest);
        }

        [HttpGet]
        [Route("test/status/{tradeRuleId}")]
        public TradeRuleTestStatusDTO GetTradeRuleTestStatusAsync(int tradeRuleId)
        {
            return _testTradeRuleQueue.GetTradeRuleTestStatus(tradeRuleId);
        }

        [HttpPost]
        [Route("test/abort/{tradeRuleId}")]
        public void AbortTradeRuleTestAsync(int tradeRuleId)
        {
            _testTradeRuleQueue.AbortTest(tradeRuleId);
        }
    }
}
