using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Waffler.Domain.Bitpanda.Private.Order
{
    public class OrderHistoryDTO
    {
        public List<OrderHistoryEntityDTO> Order_history { get; set; }
    }
}
