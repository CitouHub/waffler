using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;

namespace Waffler.Test.Helper
{
    public static class DatabaseHelper
    {
        public static Mock<DbSet<TEntity>> GetMockDbSet<TEntity>(IQueryable<TEntity> querayble) where TEntity : class
        {
            var dbSet = new Mock<DbSet<TEntity>>();
            dbSet.As<IQueryable<TEntity>>().Setup(m => m.Provider).Returns(querayble.Provider);
            dbSet.As<IQueryable<TEntity>>().Setup(m => m.Expression).Returns(querayble.Expression);
            dbSet.As<IQueryable<TEntity>>().Setup(m => m.ElementType).Returns(querayble.ElementType);
            dbSet.As<IQueryable<TEntity>>().Setup(m => m.GetEnumerator()).Returns(querayble.GetEnumerator());

            return dbSet;
        }
    }
}
