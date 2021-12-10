﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class CandleStickValueType
    {
        public CandleStickValueType()
        {
            TradeRuleConditionFromCandleStickValueTypes = new HashSet<TradeRuleCondition>();
            TradeRuleConditionToCandleStickValueTypes = new HashSet<TradeRuleCondition>();
            TradeRules = new HashSet<TradeRule>();
        }

        public short Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TradeRuleCondition> TradeRuleConditionFromCandleStickValueTypes { get; set; }
        public virtual ICollection<TradeRuleCondition> TradeRuleConditionToCandleStickValueTypes { get; set; }
        public virtual ICollection<TradeRule> TradeRules { get; set; }
    }
}
