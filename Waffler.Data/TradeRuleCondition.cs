﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace Waffler.Data
{
    public partial class TradeRuleCondition
    {
        public int Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public int TradeRuleId { get; set; }
        public short CandleStickValueTypeId { get; set; }
        public short TradeRuleConditionComparatorId { get; set; }
        public short TradeRuleConditionSampleDirectionId { get; set; }
        public int FromMinutesOffset { get; set; }
        public int ToMinutesOffset { get; set; }
        public int FromMinutesSample { get; set; }
        public int ToMinutesSample { get; set; }
        public decimal DeltaPercent { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }

        public virtual CandleStickValueType CandleStickValueType { get; set; }
        public virtual TradeRule TradeRule { get; set; }
        public virtual TradeRuleConditionComparator TradeRuleConditionComparator { get; set; }
        public virtual TradeRuleConditionSampleDirection TradeRuleConditionSampleDirection { get; set; }
    }
}