using System;
using System.Collections.Generic;
using System.Linq;

using Waffler.Common;
using Waffler.Domain.Bitpanda.Private.Balance;
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

        public static List<CandleStickDTO> GetCandleSticks(int candleSticks)
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
            }, candleSticks).ToList();
        }

        public static AccountDTO GetAccount()
        {
            return new AccountDTO()
            {
                Account_id = Guid.NewGuid().ToString(),
                Balances = new List<BalanceDTO>()
                {
                    new BalanceDTO()
                    {
                        Account_id = Guid.NewGuid().ToString(),
                        Available = 0,
                        Change = 0,
                        Currency_code = Bitpanda.CurrencyCode.EUR,
                        Locked = 0,
                        Sequence = 0,
                        Time = DateTime.UtcNow
                    },
                    new BalanceDTO()
                    {
                        Account_id = Guid.NewGuid().ToString(),
                        Available = 0,
                        Change = 0,
                        Currency_code = Bitpanda.CurrencyCode.BTC,
                        Locked = 0,
                        Sequence = 0,
                        Time = DateTime.UtcNow
                    }
                }
            };
        }

        public static OrderHistoryDTO GetOrders(int orders)
        {
            return new OrderHistoryDTO()
            {
                Order_history = Enumerable.Repeat(GetOrderHistoryEntity(), orders).ToList()
            };
        }

        public static OrderHistoryEntityDTO GetOrderHistoryEntity()
        {
            return new OrderHistoryEntityDTO()
            {
                Order = GetOrder()
            };
        }

        public static OrderDTO GetOrder()
        {
            return new OrderDTO()
            {
                Account_id = Guid.NewGuid().ToString(),
                Amount = 0,
                Average_price = 0,
                Client_id = Guid.NewGuid().ToString(),
                Filled_amount = 0,
                Instrument_code = Bitpanda.InstrumentCode.BTC_EUR,
                Order_book_sequence = 0,
                Order_id = Guid.NewGuid().ToString(),
                Price = 0,
                Sequence = 0,
                Side = Bitpanda.Side.BUY,
                Status = Bitpanda.Status.FILLED,
                Time = DateTime.UtcNow,
                Time_last_updated = DateTime.UtcNow,
                Type = Bitpanda.OrderType.LIMIT
            };
        }
    }
}
