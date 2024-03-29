﻿using System;
using System.Collections.Generic;
using System.Linq;
using Waffler.Common;
using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Test.Helper
{
    public static class CandleStickHelper
    {
        public static CandleStickDTO GetCandleStickDTO()
        {
            return new CandleStickDTO()
            {
                ClosePrice = 1000,
                HighPrice = 1000,
                LowPrice = 1000,
                OpenPrice = 1000,
                PeriodDateTime = DateTime.UtcNow,
                TotalAmount = 1000,
                TradeTypeId = (short)Variable.TradeType.BTC_EUR,
                Volume = 1000
            };
        }

        public static CandleStick GetCandleStick()
        {
            return new CandleStick()
            {
                ClosePrice = 1000,
                HighPrice = 1000,
                LowPrice = 1000,
                OpenPrice = 1000,
                PeriodDateTime = DateTime.UtcNow,
                TotalAmount = 1000,
                TradeTypeId = (short)Variable.TradeType.BTC_EUR,
                Volume = 1000
            };
        }
    }
}
