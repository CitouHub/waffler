using System.Collections.Generic;

namespace Waffler.Domain.Statistics
{
    public class TradeSelectionDTO
    {
        public List<int> TradeRules { get; set; }
        public List<short> TradeOrderStatuses { get; set; }
    }
}