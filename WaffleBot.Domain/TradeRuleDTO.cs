using System;
using System.Collections.Generic;

namespace WaffleBot.Domain
{
    public class TradeRuleDTO
    {
        public int Id { get; set; }
        public short TradeActionId { get; set; }
        public short TradeTypeId { get; set; }
        public short TradeConditionOperatorId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int TradeMinIntervalMinutes { get; set; }
        public DateTime LastTrigger { get; set; }
        public bool? IsActive { get; set; }

        public List<TradeRuleConditionDTO> TradeRuleConditions { get; set; }
    }
}
