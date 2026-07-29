/*
| Module      : Database
| Entity      : ProjectTeamConfiguration
| Purpose     : تنظیمات دیتابیس رابطه چند‌به‌چند Project و Team.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ProjectTeamConfiguration : IEntityTypeConfiguration<ProjectTeam>
    {
        public void Configure(EntityTypeBuilder<ProjectTeam> builder)
        {
            builder.ToTable("ProjectTeams");

            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ProjectId, x.TeamId })
                .IsUnique();

            builder.HasOne(x => x.Project)
                .WithMany(x => x.ProjectTeams)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Team)
                .WithMany(x => x.ProjectTeams)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}