using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ProductivityMetricsConfiguration : IEntityTypeConfiguration<ProductivityMetrics>
    {
        public void Configure(EntityTypeBuilder<ProductivityMetrics> builder)
        {
            builder.ToTable("ProductivityMetrics");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ProductivityScore).HasDefaultValue(0);
            builder.Property(x => x.TaskCompletionRate).HasDefaultValue(0);
            builder.Property(x => x.OnTimeDeliveryRate).HasDefaultValue(0);
            builder.Property(x => x.ConsistencyRate).HasDefaultValue(0);
            builder.Property(x => x.QualityScore).HasDefaultValue(0);
            builder.Property(x => x.TotalTasksAssigned).HasDefaultValue(0);
            builder.Property(x => x.TotalTasksCompleted).HasDefaultValue(0);
            builder.Property(x => x.OnTimeTasksCompleted).HasDefaultValue(0);
            builder.Property(x => x.OverdueTasksCompleted).HasDefaultValue(0);
            builder.Property(x => x.TasksReopened).HasDefaultValue(0);
            builder.Property(x => x.WorkedDaysThisPeriod).HasDefaultValue(0);
            builder.Property(x => x.TotalDaysInPeriod).HasDefaultValue(0);
            builder.Property(x => x.CurrentStreak).HasDefaultValue(0);
            builder.Property(x => x.LongestStreak).HasDefaultValue(0);
            builder.Property(x => x.IsCurrentPeriod).HasDefaultValue(true);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Workspace)
                .WithMany()
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.ScoreHistory)
                .WithOne(x => x.ProductivityMetrics)
                .HasForeignKey(x => x.ProductivityMetricsId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.UserId, x.WorkspaceId, x.IsCurrentPeriod });
        }
    }
}
