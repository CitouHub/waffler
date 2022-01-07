using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class WafflerProfile
    {
        public short Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public DateTime CandleStickSyncFromDate { get; set; }
        public string SessionKey { get; set; }
    }
}
