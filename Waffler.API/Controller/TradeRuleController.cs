﻿using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.API.Security;
using Waffler.Domain;
using Waffler.Domain.Export;
using Waffler.Domain.Message;
using Waffler.Service;
using Waffler.Service.Infrastructure;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    [ApiKey]
    public class TradeRuleController : ControllerBase
    {
        private readonly ITradeRuleService _tradeRuleService;
        private readonly ITradeRuleTestQueue _testTradeRuleQueue;

        public TradeRuleController(
            ITradeRuleService tradeRuleService,
            ITradeRuleTestQueue testTradeRuleQueue)
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

        [HttpGet]
        [Route("export/{tradeRuleId}")]
        public async Task<TradeRuleExportDTO> GetTradeRuleForExport(int tradeRuleId)
        {
            return await _tradeRuleService.GetTradeRuleAsyncForExport(tradeRuleId);
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
        public void AbortTradeRuleTestAsync(int tradeRuleId)
        {
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
