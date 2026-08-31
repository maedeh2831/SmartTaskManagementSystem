/*
| Module      : Database
| Entity      : UserProgressionConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت UserProgression
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class UserProgressionConfiguration : IEntityTypeConfiguration<UserProgression>
    {
        public void Configure(EntityTypeBuilder<UserProgression> builder)
        {
            builder.ToTable("UserProgressions");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.CurrentLevel)
                .HasDefaultValue(1);
            builder.Property(x => x.TotalExperience)
                .HasDefaultValue(0);
            builder.Property(x => x.ExperienceForNextLevel)
                .HasDefaultValue(1000);
            builder.Property(x => x.TasksCompleted)
                .HasDefaultValue(0);
            builder.Property(x => x.ProjectsCompleted)
                .HasDefaultValue(0);
            builder.Property(x => x.SprintsCompleted)
                .HasDefaultValue(0);

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            builder.HasOne(x => x.User)
                .WithOne()
                .HasForeignKey<UserProgression>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.Achievements)
                .WithOne(x => x.UserProgression)
                .HasForeignKey(x => x.UserProgressionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.WalletTransactions)
                .WithOne(x => x.UserProgression)
                .HasForeignKey(x => x.UserProgressionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
