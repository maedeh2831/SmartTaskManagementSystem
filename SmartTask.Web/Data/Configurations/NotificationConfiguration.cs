/*
| Module      : Tracking
| Entity      : Notification
| Purpose     : پیکربندی موجودیت Notification.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.IsRead)
                .HasDefaultValue(false);

            builder.Property(x => x.ReadDate)
                .IsRequired(false);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ApplicationUserId);

            builder.HasIndex(x => x.IsRead);

            builder.HasIndex(x => new { x.ApplicationUserId, x.IsRead });

            builder.HasIndex(x => x.Type);
        }
    }
}