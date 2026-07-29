/*
| Module      : Database
| Entity      : ProjectConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Project.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Key)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Color)
                .HasMaxLength(20);

            builder.Property(x => x.Icon)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(x => new { x.WorkspaceId, x.Key })
                .IsUnique();

            // Relationships
            builder.HasOne(x => x.Workspace)
                .WithMany(x => x.Projects)
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Members)
                .WithOne(x => x.Project)
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}