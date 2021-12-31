using Microsoft.EntityFrameworkCore;
using System;

using Waffler.Data;

namespace Waffler.Test.Helper
{
    public static class DatabaseHelper
    {
        public static WafflerDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<BaseDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new WafflerDbContext(options);
        }
    }
}
