using Microsoft.EntityFrameworkCore;

using Waffler.Data.ComplexModel;

#pragma warning disable IDE1006 // Naming Styles
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

        public virtual DbSet<sp_getCandleSticks_Result> sp_getCandleSticks_Result { get; set; }

        public virtual DbSet<sp_getTradeOrders_Result> sp_getTradeOrders_Result { get; set; }

        public virtual DbSet<sp_getTradeRuleBuyStatistics_Result> sp_getTradeRuleBuyStatistics_Result { get; set; }

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
                entity.Property(e => e.Volume).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<sp_getTradeOrders_Result>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 8)");
                entity.Property(e => e.FilledAmount).HasColumnType("decimal(10, 8)");
            });

            modelBuilder.Entity<sp_getTradeRuleBuyStatistics_Result>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 8)");
                entity.Property(e => e.TotalFilledAmount).HasColumnType("decimal(10, 8)");
                entity.Property(e => e.FilledPercent).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.TotalInvested).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.AveragePrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ValueIncrease).HasColumnType("decimal(5, 2)");
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
