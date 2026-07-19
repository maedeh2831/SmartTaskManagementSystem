/*
| Module      : Database
| Entity      : UserRoleConfiguration
| Purpose     : تنظیمات دیتابیس موجودیت UserRole.
*/

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Data.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(x => x.Id);

            // Indexes
            builder.HasIndex(x => new { x.ApplicationUserId, x.RoleId })
                   .IsUnique();

            // Relationships
            builder.HasOne(x => x.ApplicationUser)
                   .WithMany(x => x.UserRoles)
                   .HasForeignKey(x => x.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Role)
                   .WithMany(x => x.UserRoles)
                   .HasForeignKey(x => x.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}