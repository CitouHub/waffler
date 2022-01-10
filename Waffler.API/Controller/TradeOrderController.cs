using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.API.Security;
using Waffler.Domain;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    [ApiKey]
    public class TradeOrderController : ControllerBase
    {
        private readonly ITradeOrderService _tradeOrderService;

        public TradeOrderController(ITradeOrderService tradeOrderService)
        {
            _tradeOrderService = tradeOrderService;
        }

        [HttpGet]
        public async Task<IEnumerable<TradeOrderDTO>> GetTradeOrdersAsync(DateTime from, DateTime to)
        {
            return await _tradeOrderService.GetTradeOrdersAsync(from, to);
        }

        [HttpGet]
        [Route("any/{tradeRuleId}")]
        public async Task<bool> AnyTradeOrders(int tradeRuleId)
        {
            return await _tradeOrderService.AnyTradeOrders(tradeRuleId);
        }

        [HttpGet]
        [Route("status")]
        public async Task<IEnumerable<CommonAttributeDTO>> GetTradeOrderStatusesAsync()
        {
            return await _tradeOrderService.GetTradeOrderStatusesAsync();
        }

        [HttpPut]
        [Route("{tradeOrderId}/traderule/{tradeRuleId}")]
        public async Task<bool> SetTradeRuleAsync(int tradeOrderId, int tradeRuleId)
        {
            return await _tradeOrderService.SetTradeRuleAsync(tradeOrderId, tradeRuleId);
        }

        [HttpDelete]
        [Route("test/{tradeRuleId}")]
        public async Task DeleteTestTradeOrdersAsync(int tradeRuleId)
        {
            await _tradeOrderService.DeleteTestTradeOrdersAsync(tradeRuleId);
        }
    }
}
