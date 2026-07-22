/*
| Module      : Tracking
| Entity      : ActivityLog
| Purpose     : پیکربندی موجودیت ActivityLog.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
    {
        public void Configure(EntityTypeBuilder<ActivityLog> builder)
        {
            builder.ToTable("ActivityLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.Property(x => x.ActivityDate)
                .IsRequired();

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.ActivityLogs)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.ActivityLogs)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.ApplicationUserId);

            builder.HasIndex(x => x.TaskItemId);

            builder.HasIndex(x => x.ActivityDate);

            builder.HasIndex(x => new { x.ApplicationUserId, x.ActivityDate });
        }
    }
}