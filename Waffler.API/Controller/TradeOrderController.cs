using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.Domain;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
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
    }
}
