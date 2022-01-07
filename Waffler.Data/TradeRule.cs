using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class TradeRule
    {
        public TradeRule()
        {
            TradeOrders = new HashSet<TradeOrder>();
            TradeRuleConditions = new HashSet<TradeRuleCondition>();
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
        public short CandleStickValueTypeId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal PriceDeltaPercent { get; set; }
        public int TradeMinIntervalMinutes { get; set; }
        public int? TradeOrderExpirationMinutes { get; set; }
        public DateTime LastTrigger { get; set; }
        public bool IsDeleted { get; set; }

        public virtual CandleStickValueType CandleStickValueType { get; set; }
        public virtual TradeAction TradeAction { get; set; }
        public virtual TradeConditionOperator TradeConditionOperator { get; set; }
        public virtual TradeRuleStatus TradeRuleStatus { get; set; }
        public virtual TradeType TradeType { get; set; }
        public virtual ICollection<TradeOrder> TradeOrders { get; set; }
        public virtual ICollection<TradeRuleCondition> TradeRuleConditions { get; set; }
    }
}
