/*
| Module      : Database
| Entity      : TeamMemberConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت TeamMember.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
    {
        public void Configure(EntityTypeBuilder<TeamMember> builder)
        {
            builder.ToTable("TeamMembers");

            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.JoinedDate)
                .HasDefaultValueSql("GETDATE()");

            // Indexes
            builder.HasIndex(x => new { x.TeamId, x.ApplicationUserId })
                .IsUnique();

            // Relationships
            builder.HasOne(x => x.Team)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ApplicationUser)
                .WithMany(x => x.TeamMemberships)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}