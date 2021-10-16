using Microsoft.EntityFrameworkCore;

using WaffleBot.Data.ComplexModel;

namespace WaffleBot.Data
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

        public virtual DbSet<sp_getPriceTrends_Result> SP_getPriceTrends_Result { get; set; }

        public virtual DbSet<sp_getPriceStatistics_Result> SP_getPriceStatistics_Result { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<sp_getPriceTrends_Result>(entity =>
            {
                entity.HasNoKey();
            });

            modelBuilder.Entity<sp_getPriceStatistics_Result>(entity =>
            {
                entity.HasNoKey();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
