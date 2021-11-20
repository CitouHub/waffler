using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Waffler.Data
{
    public partial class TradeRule
    {
        public TradeRule()
        {
            TradeOrder = new HashSet<TradeOrder>();
            TradeRuleCondition = new HashSet<TradeRuleCondition>();
        }

        public int Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public short TradeActionId { get; set; }
        public short TradeTypeId { get; set; }
        public short TradeConditionOperatorId { get; set; }
        public short TradeRuleStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int TradeMinIntervalMinutes { get; set; }
        public DateTime LastTrigger { get; set; }
        public bool TestTradeInProgress { get; set; }

        public virtual TradeAction TradeAction { get; set; }
        public virtual TradeConditionOperator TradeConditionOperator { get; set; }
        public virtual TradeRuleStatus TradeRuleStatus { get; set; }
        public virtual TradeType TradeType { get; set; }
        public virtual ICollection<TradeOrder> TradeOrder { get; set; }
        public virtual ICollection<TradeRuleCondition> TradeRuleCondition { get; set; }
    }
}
