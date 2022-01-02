using System;
using System.Collections.Generic;

#nullable disable

namespace Waffler.Data
{
    public partial class DatabaseMigration
    {
        public int Id { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertByUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateByUser { get; set; }
        public string ScriptName { get; set; }
    }
}
