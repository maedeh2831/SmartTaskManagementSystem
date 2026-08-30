/*
| Module      : Database
| Entity      : LeaderboardConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Leaderboard
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class LeaderboardConfiguration : IEntityTypeConfiguration<Leaderboard>
    {
        public void Configure(EntityTypeBuilder<Leaderboard> builder)
        {
            builder.ToTable("Leaderboards");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.GlobalRank)
                .IsRequired();

            builder.Property(x => x.WorkspaceRank)
                .IsRequired();

            builder.Property(x => x.TotalPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.CurrentLevel)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.TotalExperience)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.WeeklyPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.MonthlyPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.CalculatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign keys
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Workspace)
                .WithMany()
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => new { x.WorkspaceId, x.WorkspaceRank })
                .IsUnique(false)
                .HasName("IX_Leaderboard_WorkspaceId_WorkspaceRank");

            builder.HasIndex(x => x.GlobalRank)
                .IsUnique(false)
                .HasName("IX_Leaderboard_GlobalRank");

            builder.HasIndex(x => new { x.UserId, x.WorkspaceId })
                .IsUnique(false)
                .HasName("IX_Leaderboard_UserId_WorkspaceId");

            builder.HasIndex(x => x.TotalPoints)
                .IsUnique(false)
                .HasName("IX_Leaderboard_TotalPoints");

            builder.HasIndex(x => x.LastUpdated)
                .IsUnique(false)
                .HasName("IX_Leaderboard_LastUpdated");
        }
    }
}
