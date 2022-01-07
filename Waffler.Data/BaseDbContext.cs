using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Waffler.Data
{
    public partial class BaseDbContext : DbContext
    {
        public BaseDbContext()
        {
        }

        public BaseDbContext(DbContextOptions<BaseDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CandleStick> CandleSticks { get; set; }
        public virtual DbSet<CandleStickValueType> CandleStickValueTypes { get; set; }
        public virtual DbSet<DatabaseMigration> DatabaseMigrations { get; set; }
        public virtual DbSet<TradeAction> TradeActions { get; set; }
        public virtual DbSet<TradeConditionOperator> TradeConditionOperators { get; set; }
        public virtual DbSet<TradeOrder> TradeOrders { get; set; }
        public virtual DbSet<TradeOrderStatus> TradeOrderStatuses { get; set; }
        public virtual DbSet<TradeRule> TradeRules { get; set; }
        public virtual DbSet<TradeRuleCondition> TradeRuleConditions { get; set; }
        public virtual DbSet<TradeRuleConditionComparator> TradeRuleConditionComparators { get; set; }
        public virtual DbSet<TradeRuleConditionPeriodDirection> TradeRuleConditionPeriodDirections { get; set; }
        public virtual DbSet<TradeRuleStatus> TradeRuleStatuses { get; set; }
        public virtual DbSet<TradeType> TradeTypes { get; set; }
        public virtual DbSet<WafflerProfile> WafflerProfiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS02;Initial Catalog=Waffler;persist security info=True;Integrated Security=SSPI;MultipleActiveResultSets=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<CandleStick>(entity =>
            {
                entity.ToTable("CandleStick");

                entity.HasIndex(e => e.PeriodDateTime, "IdxCandleStick_PeriodDateTime")
                    .IsUnique();

                entity.Property(e => e.AvgHighLowPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.AvgOpenClosePrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ClosePrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.HighPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.LowPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.OpenPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Volume).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.TradeType)
                    .WithMany(p => p.CandleSticks)
                    .HasForeignKey(d => d.TradeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CandleStick_TradeActionFK");
            });

            modelBuilder.Entity<CandleStickValueType>(entity =>
            {
                entity.ToTable("CandleStickValueType");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<DatabaseMigration>(entity =>
            {
                entity.ToTable("DatabaseMigration");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ScriptName)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<TradeAction>(entity =>
            {
                entity.ToTable("TradeAction");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeConditionOperator>(entity =>
            {
                entity.ToTable("TradeConditionOperator");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeOrder>(entity =>
            {
                entity.ToTable("TradeOrder");

                entity.HasIndex(e => e.OrderId, "IdxTradeOrder_OrderId")
                    .IsUnique();

                entity.Property(e => e.Amount).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.FilledAmount).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.OrderDateTime).HasPrecision(0);

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TradeOrderStatusId).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.TradeAction)
                    .WithMany(p => p.TradeOrders)
                    .HasForeignKey(d => d.TradeActionId)
                    .HasConstraintName("TradeOrder_TradeActionFK");

                entity.HasOne(d => d.TradeOrderStatus)
                    .WithMany(p => p.TradeOrders)
                    .HasForeignKey(d => d.TradeOrderStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeOrder_TradeOrderStatusFK");

                entity.HasOne(d => d.TradeRule)
                    .WithMany(p => p.TradeOrders)
                    .HasForeignKey(d => d.TradeRuleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("TradeOrder_TradeRuleFK");
            });

            modelBuilder.Entity<TradeOrderStatus>(entity =>
            {
                entity.ToTable("TradeOrderStatus");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeRule>(entity =>
            {
                entity.ToTable("TradeRule");

                entity.Property(e => e.Amount).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.LastTrigger)
                    .HasPrecision(0)
                    .HasDefaultValueSql("('1900-01-01')");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PriceDeltaPercent).HasColumnType("decimal(6, 4)");

                entity.Property(e => e.TradeRuleStatusId).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CandleStickValueType)
                    .WithMany(p => p.TradeRules)
                    .HasForeignKey(d => d.CandleStickValueTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_CandleStickValueTypeFK");

                entity.HasOne(d => d.TradeAction)
                    .WithMany(p => p.TradeRules)
                    .HasForeignKey(d => d.TradeActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeActionFK");

                entity.HasOne(d => d.TradeConditionOperator)
                    .WithMany(p => p.TradeRules)
                    .HasForeignKey(d => d.TradeConditionOperatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeConditionOperatorFK");

                entity.HasOne(d => d.TradeRuleStatus)
                    .WithMany(p => p.TradeRules)
                    .HasForeignKey(d => d.TradeRuleStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeRuleStatusFK");

                entity.HasOne(d => d.TradeType)
                    .WithMany(p => p.TradeRules)
                    .HasForeignKey(d => d.TradeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeTypeFK");
            });

            modelBuilder.Entity<TradeRuleCondition>(entity =>
            {
                entity.ToTable("TradeRuleCondition");

                entity.Property(e => e.DeltaPercent).HasColumnType("decimal(6, 4)");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.HasOne(d => d.FromCandleStickValueType)
                    .WithMany(p => p.TradeRuleConditionFromCandleStickValueTypes)
                    .HasForeignKey(d => d.FromCandleStickValueTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_FromCandleStickValueTypeFK");

                entity.HasOne(d => d.FromTradeRuleConditionPeriodDirection)
                    .WithMany(p => p.TradeRuleConditionFromTradeRuleConditionPeriodDirections)
                    .HasForeignKey(d => d.FromTradeRuleConditionPeriodDirectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_FromTradeRuleConditionPeriodDirectionFK");

                entity.HasOne(d => d.ToCandleStickValueType)
                    .WithMany(p => p.TradeRuleConditionToCandleStickValueTypes)
                    .HasForeignKey(d => d.ToCandleStickValueTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_ToCandleStickValueTypeFK");

                entity.HasOne(d => d.ToTradeRuleConditionPeriodDirection)
                    .WithMany(p => p.TradeRuleConditionToTradeRuleConditionPeriodDirections)
                    .HasForeignKey(d => d.ToTradeRuleConditionPeriodDirectionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_ToTradeRuleConditionPeriodDirectionFK");

                entity.HasOne(d => d.TradeRuleConditionComparator)
                    .WithMany(p => p.TradeRuleConditions)
                    .HasForeignKey(d => d.TradeRuleConditionComparatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_TradeRuleConditionComparatorFK");

                entity.HasOne(d => d.TradeRule)
                    .WithMany(p => p.TradeRuleConditions)
                    .HasForeignKey(d => d.TradeRuleId)
                    .HasConstraintName("TradeRuleCondition_TradeRuleFK");
            });

            modelBuilder.Entity<TradeRuleConditionComparator>(entity =>
            {
                entity.ToTable("TradeRuleConditionComparator");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeRuleConditionPeriodDirection>(entity =>
            {
                entity.ToTable("TradeRuleConditionPeriodDirection");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeRuleStatus>(entity =>
            {
                entity.ToTable("TradeRuleStatus");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeType>(entity =>
            {
                entity.ToTable("TradeType");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<WafflerProfile>(entity =>
            {
                entity.ToTable("WafflerProfile");

                entity.Property(e => e.ApiKey).HasMaxLength(4000);

                entity.Property(e => e.CandleStickSyncFromDate).HasColumnType("date");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.SessionKey).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
