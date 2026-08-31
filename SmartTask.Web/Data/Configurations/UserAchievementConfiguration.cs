/*
| Module      : Database
| Entity      : UserAchievementConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت UserAchievement
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
    {
        public void Configure(EntityTypeBuilder<UserAchievement> builder)
        {
            builder.ToTable("UserAchievements");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.UserId, x.AchievementId })
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.UserProgression)
                .WithMany(x => x.Achievements)
                .HasForeignKey(x => x.UserProgressionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Achievement)
                .WithMany(x => x.UserAchievements)
                .HasForeignKey(x => x.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
