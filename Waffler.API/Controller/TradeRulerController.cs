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
        private readonly TestTradeRuleQueue _testTradeRuleQueue;

        public TradeRuleController(
            ITradeRuleService tradeRuleService,
            TestTradeRuleQueue testTradeRuleQueue)
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
        public void TestTradeRuleAsync([FromBody] TradeTestRequestDTO tradeRequest)
        {
            _testTradeRuleQueue.Queue(tradeRequest);
        }

        [HttpPost]
        [Route("test/status/{tradeRuleId}")]
        public TradeTestStatusDTO GetTestTradeRuleStatusAsync(int tradeRuleId)
        {
            return _testTradeRuleQueue.GetStatus(tradeRuleId);
        }
    }
}
