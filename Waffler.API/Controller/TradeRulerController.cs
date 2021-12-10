using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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

        [HttpPost]
        public async Task<TradeRuleDTO> NewTradeRuleAsync()
        {
            return await _tradeRuleService.NewTradeRuleAsync();
        }

        [HttpPost]
        [Route("copy/{tradeRuleId}")]
        public async Task<bool> CopyTradeRuleAsync(int tradeRuleId)
        {
            return await _tradeRuleService.CopyTradeRuleAsync(tradeRuleId);
        }

        [HttpGet]
        public async Task<IEnumerable<TradeRuleDTO>> GetTradeRulesAsync()
        {
            return await _tradeRuleService.GetTradeRulesAsync();
        }

        [HttpGet]
        [Route("{tradeRuleId}")]
        public async Task<TradeRuleDTO> GetTradeRule(int tradeRuleId)
        {
            return await _tradeRuleService.GetTradeRuleAsync(tradeRuleId);
        }

        [HttpPost]
        [Route("import")]
        public async Task<bool> ImportTradeRule([FromBody] TradeRuleDTO tradeRule)
        {
            return await _tradeRuleService.AddTradeRuleAsync(tradeRule);
        }

        [HttpGet]
        [Route("attribute")]
        public async Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleAttributesAsync()
        {
            return await _tradeRuleService.GetTradeRuleAttributesAsync();
        }

        [HttpPut]
        public async Task<bool> UpdateTradeRulesAsync([FromBody] TradeRuleDTO tradeRule)
        {
            return await _tradeRuleService.UpdateTradeRuleAsync(tradeRule);
        }

        [HttpPost]
        [Route("test/start")]
        public void StartTradeRuleTestAsync([FromBody] TradeRuleTestRequestDTO tradeRequest)
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
        public async Task AbortTradeRuleTestAsync(int tradeRuleId)
        {
            var tradeRule = await _tradeRuleService.GetTradeRuleAsync(tradeRuleId);
            tradeRule.TestTradeInProgress = false;
            await _tradeRuleService.UpdateTradeRuleAsync(tradeRule);
            _testTradeRuleQueue.AbortTest(tradeRuleId);
        }

        [HttpDelete]
        [Route("{tradeRuleId}")]
        public async Task<bool> DeleteTradeRuleAsync(int tradeRuleId)
        {
            return await _tradeRuleService.DeleteTradeRuleAsync(tradeRuleId);
        }
    }
}
