using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace Waffler.Data.Extensions
{
    public static class IndexExtension
    {
        public static async Task RebuildIndex(this WafflerDbContext context, string tableName, string indexName)
        {
            var expr = $"ALTER INDEX {indexName} ON {tableName} REBUILD";
            await context.Database.ExecuteSqlRawAsync(expr);
        }

        public static async Task ReorganizeIndex(this WafflerDbContext context, string tableName, string indexName)
        {
            var expr = $"ALTER INDEX {indexName} ON {tableName} REORGANIZE";
            await context.Database.ExecuteSqlRawAsync(expr);
        }
    }
}