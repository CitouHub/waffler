using System;
using System.Collections.Generic;

namespace Waffler.Domain
{
    public class TradeRuleDTO
    {
        public int Id { get; set; }
        public short TradeActionId { get; set; }
        public string TradeActionName { get; set; }
        public short TradeTypeId { get; set; }
        public string TradeTypeName { get; set; }
        public short TradeConditionOperatorId { get; set; }
        public string TradeConditionOperatorName { get; set; }
        public short TradeRuleStatusId { get; set; }
        public string TradeRuleStatusName { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public int TradeMinIntervalMinutes { get; set; }
        public DateTime LastTrigger { get; set; }
        public bool TestTradeInProgress { get; set; }

        public List<TradeRuleConditionDTO> TradeRuleConditions { get; set; }
    }
}
