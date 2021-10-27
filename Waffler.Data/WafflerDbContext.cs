using Microsoft.EntityFrameworkCore;

using Waffler.Data.ComplexModel;

namespace Waffler.Data
{
    public partial class WafflerDbContext : BaseDbContext
    {
        public WafflerDbContext()
        {
        }

        public WafflerDbContext(DbContextOptions<BaseDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<sp_getCandleSticks_Result> SP_getCandleSticks_Result { get; set; }

        public virtual DbSet<sp_getTradeOrders_Result> SP_getTradeOrders_Result { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<sp_getCandleSticks_Result>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<sp_getTradeOrders_Result>(entity =>
            {
                entity.HasNoKey();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
