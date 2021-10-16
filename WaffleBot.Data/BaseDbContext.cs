using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace WaffleBot.Data
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

        public virtual DbSet<CandleStick> CandleStick { get; set; }
        public virtual DbSet<CandleStickValueType> CandleStickValueType { get; set; }
        public virtual DbSet<ConditionComparator> ConditionComparator { get; set; }
        public virtual DbSet<TradeAction> TradeAction { get; set; }
        public virtual DbSet<TradeConditionOperator> TradeConditionOperator { get; set; }
        public virtual DbSet<TradeOrder> TradeOrder { get; set; }
        public virtual DbSet<TradeOrderStatus> TradeOrderStatus { get; set; }
        public virtual DbSet<TradeRule> TradeRule { get; set; }
        public virtual DbSet<TradeRuleCondition> TradeRuleCondition { get; set; }
        public virtual DbSet<TradeType> TradeType { get; set; }
        public virtual DbSet<WafflerProfile> WafflerProfile { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS02;Initial Catalog=db_waffle;persist security info=True;Integrated Security=SSPI;MultipleActiveResultSets=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CandleStick>(entity =>
            {
                entity.Property(e => e.AvgHighLowPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.AvgOpenClosePrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.ClosePrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.GranularityUnit)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.HighPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.LowPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.OpenPrice).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.PeriodDateTime).HasColumnType("datetime2(0)");

                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.Volume).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.TradeType)
                    .WithMany(p => p.CandleStick)
                    .HasForeignKey(d => d.TradeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("CandleStick_TradeActionFK");
            });

            modelBuilder.Entity<CandleStickValueType>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ConditionComparator>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeAction>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeConditionOperator>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeOrder>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.FilledAmount).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.InstrumentCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TradeOrderStatusId).HasDefaultValueSql("((1))");

                entity.HasOne(d => d.TradeOrderStatus)
                    .WithMany(p => p.TradeOrder)
                    .HasForeignKey(d => d.TradeOrderStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeOrder_TradeOrderStatusFK");

                entity.HasOne(d => d.TradeRule)
                    .WithMany(p => p.TradeOrder)
                    .HasForeignKey(d => d.TradeRuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeOrder_TradeRuleFK");
            });

            modelBuilder.Entity<TradeOrderStatus>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<TradeRule>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(10, 8)");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.TradeAction)
                    .WithMany(p => p.TradeRule)
                    .HasForeignKey(d => d.TradeActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeActionFK");

                entity.HasOne(d => d.TradeConditionOperator)
                    .WithMany(p => p.TradeRule)
                    .HasForeignKey(d => d.TradeConditionOperatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeConditionOperatorFK");

                entity.HasOne(d => d.TradeType)
                    .WithMany(p => p.TradeRule)
                    .HasForeignKey(d => d.TradeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRule_TradeTypeFK");
            });

            modelBuilder.Entity<TradeRuleCondition>(entity =>
            {
                entity.Property(e => e.DeltaPercent).HasColumnType("decimal(4, 2)");

                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CandleStickValueType)
                    .WithMany(p => p.TradeRuleCondition)
                    .HasForeignKey(d => d.CandleStickValueTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_CandleStickValueTypeFK");

                entity.HasOne(d => d.ConditionComparator)
                    .WithMany(p => p.TradeRuleCondition)
                    .HasForeignKey(d => d.ConditionComparatorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_ConditionComparatorFK");

                entity.HasOne(d => d.TradeRule)
                    .WithMany(p => p.TradeRuleCondition)
                    .HasForeignKey(d => d.TradeRuleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("TradeRuleCondition_TradeRuleFK");
            });

            modelBuilder.Entity<TradeType>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<WafflerProfile>(entity =>
            {
                entity.Property(e => e.ApiKey).HasMaxLength(4000);

                entity.Property(e => e.InsertByUser).HasDefaultValueSql("((1))");

                entity.Property(e => e.InsertDate).HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(500);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
