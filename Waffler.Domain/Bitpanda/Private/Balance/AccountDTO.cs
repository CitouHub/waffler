using System.Collections.Generic;

namespace Waffler.Domain.Bitpanda.Private.Balance
{
    public class AccountDTO
    {
        public string account_id { get; set; }
        public List<BalanceDTO> balances { get; set; }
    }
}
