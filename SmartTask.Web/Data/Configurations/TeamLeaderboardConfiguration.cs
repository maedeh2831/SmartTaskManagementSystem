/*
| Module      : Database
| Entity      : TeamLeaderboardConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت TeamLeaderboard
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TeamLeaderboardConfiguration : IEntityTypeConfiguration<TeamLeaderboard>
    {
        public void Configure(EntityTypeBuilder<TeamLeaderboard> builder)
        {
            builder.ToTable("TeamLeaderboards");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TeamId)
                .IsRequired();

            builder.Property(x => x.WorkspaceId)
                .IsRequired();

            builder.Property(x => x.TeamRank)
                .IsRequired();

            builder.Property(x => x.TotalTeamPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.AverageTeamLevel)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(x => x.TotalTeamExperience)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.WeeklyPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.MonthlyPoints)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.AverageCompletionRate)
                .HasDefaultValue(0.0);

            builder.Property(x => x.AverageProductivity)
                .HasDefaultValue(0.0);

            builder.Property(x => x.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.CalculatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Foreign keys
            builder.HasOne(x => x.Team)
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Workspace)
                .WithMany()
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(x => new { x.WorkspaceId, x.TeamRank })
                .IsUnique(false)
                .HasName("IX_TeamLeaderboard_WorkspaceId_TeamRank");

            builder.HasIndex(x => x.TeamId)
                .IsUnique(false)
                .HasName("IX_TeamLeaderboard_TeamId");

            builder.HasIndex(x => x.TotalTeamPoints)
                .IsUnique(false)
                .HasName("IX_TeamLeaderboard_TotalTeamPoints");

            builder.HasIndex(x => x.LastUpdated)
                .IsUnique(false)
                .HasName("IX_TeamLeaderboard_LastUpdated");
        }
    }
}
