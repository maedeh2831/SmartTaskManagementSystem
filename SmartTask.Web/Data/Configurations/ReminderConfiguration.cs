/*
| Module      : Tracking
| Entity      : Reminder
| Purpose     : پیکربندی موجودیت Reminder.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
    {
        public void Configure(EntityTypeBuilder<Reminder> builder)
        {
            builder.ToTable("Reminders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.ReminderDate)
                .IsRequired();

            builder.Property(x => x.IsSent)
                .HasDefaultValue(false);

            builder.HasOne(x => x.TaskItem)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.TaskItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.TaskItemId);

            builder.HasIndex(x => x.ApplicationUserId);

            builder.HasIndex(x => x.ReminderDate);

            builder.HasIndex(x => new { x.ApplicationUserId, x.IsSent });
        }
    }
}