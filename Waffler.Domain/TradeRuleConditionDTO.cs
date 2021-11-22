using Waffler.Common;

namespace Waffler.Domain
{
    public class TradeRuleConditionDTO
    {
        public int Id { get; set; }
        public int TradeRuleId { get; set; }
        public short TradeRuleConditionComparatorId { get; set; }
        public string TradeRuleConditionComparatorName { get; set; }
        public short TradeRuleConditionSampleDirectionId { get; set; }
        public string TradeRuleConditionSampleDirectionName { get; set; }
        public short CandleStickValueTypeId { get; set; }
        public string CandleStickValueTypeName { get; set; }
        public int FromMinutesOffset { get; set; }
        public int ToMinutesOffset { get; set; }
        public int FromMinutesSample { get; set; }
        public int ToMinutesSample { get; set; }
        public decimal DeltaPercent { get; set; }
        public string Description { get; set; }
        public bool IsOn { get; set; }

        //Consolidation of span and sampling for ease of use
        public Variable.TimeUnit SpanTimeUnit
        {
            get
            {
                if(FromMinutesOffset % (24*60) == 0)
                {
                    return Variable.TimeUnit.Day;
                } 
                else if(FromMinutesOffset % 60 == 0)
                {
                    return Variable.TimeUnit.Hour;
                }

                return Variable.TimeUnit.Minute;
            }
        }

        public int FromTime
        {
            get
            {
                switch(SpanTimeUnit)
                {
                    case Variable.TimeUnit.Day:
                        return -1 * FromMinutesOffset / (24 * 60);
                    case Variable.TimeUnit.Hour:
                        return -1 * FromMinutesOffset / 60;
                }

                return -1 * FromMinutesOffset;
            }
        }

        public int ToTime
        {
            get
            {
                switch (SpanTimeUnit)
                {
                    case Variable.TimeUnit.Day:
                        return -1 * ToMinutesOffset / (24 * 60);
                    case Variable.TimeUnit.Hour:
                        return -1 * ToMinutesOffset / 60;
                }

                return -1 * ToMinutesOffset;
            }
        }

        public Variable.TimeUnit SampleTimeUnit
        {
            get
            {
                if (FromMinutesSample % (24 * 60) == 0)
                {
                    return Variable.TimeUnit.Day;
                }
                else if (FromMinutesSample % 60 == 0)
                {
                    return Variable.TimeUnit.Hour;
                }

                return Variable.TimeUnit.Minute;
            }
        }

        public int FromSample
        {
            get
            {
                switch (SampleTimeUnit)
                {
                    case Variable.TimeUnit.Day:
                        return FromMinutesSample / (24 * 60);
                    case Variable.TimeUnit.Hour:
                        return FromMinutesSample / 60;
                }

                return FromMinutesSample;
            }
        }

        public int ToSample
        {
            get
            {
                switch (SampleTimeUnit)
                {
                    case Variable.TimeUnit.Day:
                        return ToMinutesSample / (24 * 60);
                    case Variable.TimeUnit.Hour:
                        return ToMinutesSample / 60;
                }

                return ToMinutesSample;
            }
        }
    }
}
