﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace WaffleBot.Data
{
    public partial class CandleStickValueType
    {
        public CandleStickValueType()
        {
            TradeRuleCondition = new HashSet<TradeRuleCondition>();
        }

        public short Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<TradeRuleCondition> TradeRuleCondition { get; set; }
    }
}
