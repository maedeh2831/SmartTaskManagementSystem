/*
| Module      : Database
| Entity      : TeamConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Team.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.ToTable("Teams");

            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Color)
                .HasMaxLength(20);

            builder.Property(x => x.Logo)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(x => new { x.WorkspaceId, x.Name })
                .IsUnique()
                .HasFilter("[ViewState] = 1");

            // Relationships
            builder.HasOne(x => x.Workspace)
                .WithMany(x => x.Teams)
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Members)
                .WithOne(x => x.Team)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}