/*
| Module      : Database
| Entity      : WorkspaceConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت Workspace.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");

            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.Logo)
                .HasMaxLength(500);

            builder.Property(x => x.Color)
                .HasMaxLength(20);

            // Relationships
            builder.HasMany(x => x.Members)
                   .WithOne(x => x.Workspace)
                   .HasForeignKey(x => x.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Teams)
                   .WithOne(x => x.Workspace)
                   .HasForeignKey(x => x.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Projects)
                   .WithOne(x => x.Workspace)
                   .HasForeignKey(x => x.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}