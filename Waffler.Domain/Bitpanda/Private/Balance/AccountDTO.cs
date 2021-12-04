using System.Collections.Generic;

namespace Waffler.Domain.Bitpanda.Private.Balance
{
    public class AccountDTO
    {
        public string Account_id { get; set; }
        public List<BalanceDTO> Balances { get; set; }
    }
}
