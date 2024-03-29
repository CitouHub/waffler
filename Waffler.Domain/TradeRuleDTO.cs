﻿using System;
using System.Collections.Generic;

using Waffler.Common;
using Waffler.Domain.Converter;

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
        public short CandleStickValueTypeId { get; set; }
        public string CandleStickValueTypeName { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal PriceDeltaPercent { get; set; }
        public int TradeMinIntervalMinutes { get; set; }
        public int? TradeOrderExpirationMinutes { get; set; }
        public DateTime LastTrigger { get; set; }

        public List<TradeRuleConditionDTO> TradeRuleConditions { get; set; }

        //Consolidation of time interval 
        public Variable.TimeUnit IntervalTimeUnit { get { return TimeUnitFormatConverter.GetTimeUnit(TradeMinIntervalMinutes); } }
        public int TradeMinInterval { get { return TimeUnitFormatConverter.GetTimeValue(IntervalTimeUnit, TradeMinIntervalMinutes); } }
        public Variable.TimeUnit OrderExpirationTimeUnit { get { return TimeUnitFormatConverter.GetTimeUnit(TradeOrderExpirationMinutes); } }
        public int OrderExpiration { get { return TimeUnitFormatConverter.GetTimeValue(OrderExpirationTimeUnit, TradeOrderExpirationMinutes); } }
    }
}
