using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.API.Security;
using Waffler.Domain;
using Waffler.Service;
using Waffler.Service.Infrastructure;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    [ApiKey]
    public class TradeOrderController : ControllerBase
    {
        private readonly ITradeOrderService _tradeOrderService;
        private readonly IProfileService _profileService;
        private readonly ITradeOrderSyncSignal _tradeOrderSyncSignal;

        public TradeOrderController(ITradeOrderService tradeOrderService, IProfileService profileService, ITradeOrderSyncSignal tradeOrderSyncSignal)
        {
            _tradeOrderService = tradeOrderService;
            _profileService = profileService;
            _tradeOrderSyncSignal = tradeOrderSyncSignal;
        }

        [HttpGet]
        public async Task<IEnumerable<TradeOrderDTO>> GetTradeOrdersAsync(DateTime from, DateTime to)
        {
            return await _tradeOrderService.GetTradeOrdersAsync(from, to);
        }

        [HttpGet]
        [Route("any/{tradeRuleId}")]
        public async Task<bool> AnyTradeOrdersAsync(int tradeRuleId)
        {
            return await _tradeOrderService.AnyTradeOrdersAsync(tradeRuleId);
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

        [HttpPost]
        [Route("sync/reset")]
        public async Task ResetTradeOrderAsync()
        {
            if (_tradeOrderSyncSignal.IsActive())
            {
                _tradeOrderSyncSignal.Abort();
                await _tradeOrderSyncSignal.AwaitAbortAsync();
            }

            var profile = await _profileService.GetProfileAsync();
            await _tradeOrderService.SetTradeOrderSyncPositionAsync(profile.CandleStickSyncFromDate);
        }
    }
}
