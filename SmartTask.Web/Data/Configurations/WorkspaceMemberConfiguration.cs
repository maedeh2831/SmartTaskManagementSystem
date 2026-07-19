/*
| Module      : Database
| Entity      : WorkspaceMemberConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت WorkspaceMember.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class WorkspaceMemberConfiguration : IEntityTypeConfiguration<WorkspaceMember>
    {
        public void Configure(EntityTypeBuilder<WorkspaceMember> builder)
        {
            builder.ToTable("WorkspaceMembers");

            builder.HasKey(x => x.Id);

            // Indexes
            builder.HasIndex(x => new { x.WorkspaceId, x.ApplicationUserId })
                   .IsUnique();

            // Relationships
            builder.HasOne(x => x.Workspace)
                   .WithMany(x => x.Members)
                   .HasForeignKey(x => x.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                   .WithMany(x => x.WorkspaceMemberships)
                   .HasForeignKey(x => x.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}