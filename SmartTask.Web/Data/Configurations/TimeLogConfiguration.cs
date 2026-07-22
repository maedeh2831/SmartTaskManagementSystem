/*
| Module      : Tracking
| Entity      : TimeLog
| Purpose     : پیکربندی موجودیت TimeLog.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TimeLogConfiguration : IEntityTypeConfiguration<TimeLog>
    {
        public void Configure(EntityTypeBuilder<TimeLog> builder)
        {
            builder.ToTable("TimeLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartTime)
                .IsRequired();

            builder.Property(x => x.EndTime);

            builder.Property(x => x.DurationMinutes)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.TimeLogs)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.TimeLogs)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TaskItemId);

            builder.HasIndex(x => x.ApplicationUserId);

            builder.HasIndex(x => x.StartTime);

            builder.HasIndex(x => new { x.TaskItemId, x.ApplicationUserId });
        }
    }
}