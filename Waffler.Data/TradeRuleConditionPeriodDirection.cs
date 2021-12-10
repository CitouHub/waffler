using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class TradeRuleConditionPeriodDirection
    {
        public TradeRuleConditionPeriodDirection()
        {
            TradeRuleConditionFromTradeRuleConditionPeriodDirections = new HashSet<TradeRuleCondition>();
            TradeRuleConditionToTradeRuleConditionPeriodDirections = new HashSet<TradeRuleCondition>();
        }

        public short Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TradeRuleCondition> TradeRuleConditionFromTradeRuleConditionPeriodDirections { get; set; }
        public virtual ICollection<TradeRuleCondition> TradeRuleConditionToTradeRuleConditionPeriodDirections { get; set; }
    }
}
