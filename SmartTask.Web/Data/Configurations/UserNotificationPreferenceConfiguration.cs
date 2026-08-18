using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;
namespace SmartTask.Web.Data.Configurations
{
    public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
    {
        public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ApplicationUserId, x.NotificationType })
                   .IsUnique();

            builder.HasOne(x => x.ApplicationUser)
                   .WithMany(u => u.NotificationPreferences)
                   .HasForeignKey(x => x.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}