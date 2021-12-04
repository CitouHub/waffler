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

                entity.Property(e => e.HighPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.LowPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.OpenPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ClosePrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.AvgHighLowPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.AvgOpenClosePrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Volume).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<sp_getTradeOrders_Result>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 8)");
                entity.Property(e => e.FilledAmount).HasColumnType("decimal(10, 8)");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
