using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Waffler.Domain;
using Waffler.Service;

namespace Waffler.API.Controller
{
    [ApiController]
    [Route("v1/[controller]")]
    public class TradeRuleConditionController : ControllerBase
    {
        private readonly ITradeRuleConditionService _tradeRuleConditionService;

        public TradeRuleConditionController(ITradeRuleConditionService tradeRuleConditionService)
        {
            _tradeRuleConditionService = tradeRuleConditionService;
        }

        [HttpPost]
        [Route("{tradeRuleId}")]
        public async Task<TradeRuleConditionDTO> NewTradeRuleConditionAsync(int tradeRuleId)
        {
            return await _tradeRuleConditionService.NewTradeRuleConditionAsync(tradeRuleId);
        }

        [HttpGet]
        [Route("{tradeRuleId}")]
        public async Task<IEnumerable<TradeRuleConditionDTO>> GetTradeRuleConditionsAsync(int tradeRuleId)
        {
            return await _tradeRuleConditionService.GetTradeRuleConditionsAsync(tradeRuleId);
        }

        [HttpGet]
        [Route("attribute")]
        public async Task<Dictionary<string, List<CommonAttributeDTO>>> GetTradeRuleConditionAttributesAsync()
        {
            return await _tradeRuleConditionService.GetTradeRuleConditionAttributesAsync();
        }

        [HttpPut]
        public async Task<bool> UpdateTradeRulesConditionAsync([FromBody] TradeRuleConditionDTO tradeRuleCondition)
        {
            return await _tradeRuleConditionService.UpdateTradeRuleConditionAsync(tradeRuleCondition);
        }

        [HttpDelete]
        [Route("{tradeRuleConditionId}")]
        public async Task<bool> DeleteTradeRuleConditionAsync(int tradeRuleConditionId)
        {
            return await _tradeRuleConditionService.DeleteTradeRuleConditionAsync(tradeRuleConditionId);
        }
    }
}
