/*
| Module      : Database
| Entity      : WorkspaceInvitationConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت WorkspaceInvitation.
*/
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;
namespace SmartTask.Web.Data.Configurations
{
    public class WorkspaceInvitationConfiguration : IEntityTypeConfiguration<WorkspaceInvitation>
    {
        public void Configure(EntityTypeBuilder<WorkspaceInvitation> builder)
        {
            builder.ToTable("WorkspaceInvitations");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Token)
                   .IsUnique();

            builder.HasOne(x => x.Workspace)
                   .WithMany()
                   .HasForeignKey(x => x.WorkspaceId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InvitedUser)
                   .WithMany()
                   .HasForeignKey(x => x.InvitedUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.InvitedByUser)
                   .WithMany()
                   .HasForeignKey(x => x.InvitedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}