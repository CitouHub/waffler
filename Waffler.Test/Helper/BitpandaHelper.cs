using System;
using System.Collections.Generic;
using System.Linq;
using Waffler.Common;
using Waffler.Domain.Bitpanda.Private.Order;
using Waffler.Domain.Bitpanda.Public;

namespace Waffler.Test.Helper
{
    public static class BitpandaHelper
    {
        public static OrderSubmittedDTO GetOrderSubmitted()
        {
            return new OrderSubmittedDTO()
            {
                Account_id = Guid.NewGuid().ToString(),
                Amount = 0,
                Client_id = Guid.NewGuid().ToString(),
                Filled_amount = 0,
                Instrument_code = Bitpanda.InstrumentCode.BTC_EUR,
                Order_id = Guid.NewGuid().ToString(),
                Price = 0,
                Side = Bitpanda.Side.BUY,
                Time = DateTime.UtcNow,
                Type = Bitpanda.OrderType.LIMIT
            };
        }

        public static List<CandleStickDTO> GetCandleSticks()
        {
            return Enumerable.Repeat(new CandleStickDTO()
            {
                Close = 1000,
                High = 1000,
                Instrument_Code = Bitpanda.InstrumentCode.BTC_EUR,
                Last_Sequence = 0,
                Low = 1000,
                Open = 1000,
                Time = DateTime.UtcNow,
                Total_Amount = 1000,
                Volume = 1000
            }, 30).ToList();
        }
    }
}
