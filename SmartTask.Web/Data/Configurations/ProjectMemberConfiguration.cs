/*
| Module      : Database
| Entity      : ProjectMemberConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت ProjectMember.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
    {
        public void Configure(EntityTypeBuilder<ProjectMember> builder)
        {
            builder.Property(x => x.WeeklyCapacityHours)
            .HasDefaultValue(40);

            builder.ToTable("ProjectMembers");

            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.JoinedDate)
                   .HasDefaultValueSql("GETDATE()");

            // Indexes
            builder.HasIndex(x => new { x.ProjectId, x.ApplicationUserId })
                   .IsUnique();

            // Relationships
            builder.HasOne(x => x.Project)
                   .WithMany(x => x.Members)
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                   .WithMany(x => x.ProjectMemberships)
                   .HasForeignKey(x => x.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}