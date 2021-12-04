using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class TradeRuleStatus
    {
        public TradeRuleStatus()
        {
            TradeRules = new HashSet<TradeRule>();
        }

        public short Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TradeRule> TradeRules { get; set; }
    }
}
