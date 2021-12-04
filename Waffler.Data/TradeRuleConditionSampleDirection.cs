using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class TradeRuleConditionSampleDirection
    {
        public TradeRuleConditionSampleDirection()
        {
            TradeRuleConditions = new HashSet<TradeRuleCondition>();
        }

        public short Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TradeRuleCondition> TradeRuleConditions { get; set; }
    }
}
