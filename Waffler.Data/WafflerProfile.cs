using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

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
        public int CandleStickSyncOffsetDays { get; set; }
    }
}
