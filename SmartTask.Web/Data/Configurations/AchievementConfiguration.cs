/*
| Module      : Database
| Entity      : AchievementConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Achievement
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
    {
        public void Configure(EntityTypeBuilder<Achievement> builder)
        {
            builder.ToTable("Achievements");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);
            builder.Property(x => x.Description)
                .HasMaxLength(1000);
            builder.Property(x => x.Icon)
                .HasMaxLength(500);
            builder.Property(x => x.Color)
                .HasMaxLength(20);
            builder.Property(x => x.Condition)
                .HasMaxLength(200);

            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasMany(x => x.UserAchievements)
                .WithOne(x => x.Achievement)
                .HasForeignKey(x => x.AchievementId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
