using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ProductivityScoreHistoryConfiguration : IEntityTypeConfiguration<ProductivityScoreHistory>
    {
        public void Configure(EntityTypeBuilder<ProductivityScoreHistory> builder)
        {
            builder.ToTable("ProductivityScoreHistories");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductivityScore).HasDefaultValue(0);
            builder.Property(x => x.TaskCompletionRate).HasDefaultValue(0);
            builder.Property(x => x.OnTimeDeliveryRate).HasDefaultValue(0);
            builder.Property(x => x.ConsistencyRate).HasDefaultValue(0);
            builder.Property(x => x.QualityScore).HasDefaultValue(0);
            builder.Property(x => x.TasksCompletedThisPeriod).HasDefaultValue(0);
            builder.Property(x => x.OnTimeTasksThisPeriod).HasDefaultValue(0);
            builder.Property(x => x.CurrentStreak).HasDefaultValue(0);
            builder.Property(x => x.PeriodType).HasDefaultValue("Daily");

            builder.HasOne(x => x.ProductivityMetrics)
                .WithMany(x => x.ScoreHistory)
                .HasForeignKey(x => x.ProductivityMetricsId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.UserId, x.SnapshotDate });
        }
    }
}
