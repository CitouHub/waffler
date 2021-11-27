using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waffler.Data.Extensions
{
    public static class TableExtension
    {
        public static async Task TruncateTable(this WafflerDbContext context, string tableName)
        {
            var expr = $"TRUNCATE TABLE {tableName}";
            await context.Database.ExecuteSqlRawAsync(expr);
        }
    }
}
